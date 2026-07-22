using System;
using System.Collections;
using System.Collections.Generic;
using Pneuma.Unit;
using UnityEngine;
//행동 선택, 예견, 실행 흐름 관리
public class EnemyActionController : MonoBehaviour
{
    [SerializeField] private Enemy owner;
    [SerializeField] private EnemyBehaviorData behaviorData;
    [SerializeField] private EnemyActionExecutor actionExecutor;

    private readonly List<EnemyActionRuntime> runtimeActions = new();

    public EnemyActionRuntime PredictedAction { get; private set; }

    public event Action<EnemyActionData> OnActionPredicted;

    public void Initialize(Enemy enemy, EnemyBehaviorData behaviorData)
    {
        owner = enemy;
        this.behaviorData = behaviorData;
        runtimeActions.Clear();

        if (behaviorData == null)
        {
            Debug.LogError(
                $"[EnemyActionController] {name}: BehaviorData가 없습니다.");
            return;
        }

        foreach (EnemyActionData actionData in behaviorData.Actions)
        {
            runtimeActions.Add(new EnemyActionRuntime(actionData));
        }
    }

    public void PredictAction(int targetTurn, int currentPhase)
    {
        PredictedAction = SelectAction(targetTurn, currentPhase);

        if (PredictedAction == null)
        {
            Debug.LogWarning(
                $"[EnemyActionController] {name}: " +
                $"{targetTurn}턴에 선택 가능한 행동이 없습니다.");

            OnActionPredicted?.Invoke(null);
            return;
        }

        Debug.Log(
            $"[EnemyActionController] {name} 예견 행동: " +
            $"{PredictedAction.Data.ActionName}");

        OnActionPredicted?.Invoke(PredictedAction.Data);
    }

    public IEnumerator ExecutePredictedAction(
        int currentTurn,
        Player target)
    {
        if (PredictedAction == null)
        {
            Debug.LogWarning(
                $"[EnemyActionController] {name}: 예견 행동이 없습니다.");
            yield break;
        }

        EnemyActionRuntime executingAction = PredictedAction;
        PredictedAction = null;

        if (actionExecutor == null)
        {
            Debug.LogError(
                $"[EnemyActionController] {name}: " +
                "ActionExecutor가 연결되지 않았습니다.");
            yield break;
        }

        yield return actionExecutor.Execute(
            owner,
            target,
            executingAction.Data);

        executingAction.MarkUsed(currentTurn);
    }

    private EnemyActionRuntime SelectAction(
        int targetTurn,
        int currentPhase)
    {
        EnemyActionRuntime fixedAction =
            FindFixedAction(targetTurn, currentPhase);

        if (fixedAction != null)
            return fixedAction;

        return SelectRandomAction(targetTurn, currentPhase);
    }

    private EnemyActionRuntime FindFixedAction(
        int targetTurn,
        int currentPhase)
    {
        foreach (EnemyActionRuntime action in runtimeActions)
        {
            if (action.Data.SelectionType !=
                EnemyActionSelectionType.FixedTurn)
            {
                continue;
            }

            if (action.Data.FixedTurn != targetTurn)
                continue;

            if (!action.IsAvailable(targetTurn, currentPhase))
                continue;

            return action;
        }

        return null;
    }

    private EnemyActionRuntime SelectRandomAction(
        int targetTurn,
        int currentPhase)
    {
        List<EnemyActionRuntime> candidates = new();

        foreach (EnemyActionRuntime action in runtimeActions)
        {
            if (action.Data.SelectionType !=
                EnemyActionSelectionType.RandomPool)
            {
                continue;
            }

            if (!action.IsAvailable(targetTurn, currentPhase))
                continue;

            candidates.Add(action);
        }

        if (candidates.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(
            0,
            candidates.Count);

        return candidates[randomIndex];
    }
}