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

    protected readonly List<EnemyActionRuntime> runtimeActions = new();

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
            $"[EnemyActionController] {name} {targetTurn}턴 예견 행동: " +
            $"{PredictedAction.Data.ActionName} " +
            $"(Type: {PredictedAction.Data.SelectionType}, " +
            $"Phase: {currentPhase})");

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

        Debug.Log(
            $"[EnemyActionController] {name} {currentTurn}턴 행동 실행 시작: " +
            $"{executingAction.Data.ActionName}");

        yield return actionExecutor.Execute(
            owner,
            target,
            executingAction.Data);

        executingAction.MarkUsed(currentTurn);

        Debug.Log(
            $"[EnemyActionController] {name} {currentTurn}턴 행동 실행 완료: " +
            $"{executingAction.Data.ActionName} " +
            $"(UseCount: {executingAction.UseCount}, " +
            $"LastUsedTurn: {executingAction.LastUsedTurn})");
    }

    private EnemyActionRuntime SelectAction(
        int targetTurn,
        int currentPhase)
    {
        EnemyActionRuntime fixedAction =
            FindFixedAction(targetTurn, currentPhase);

        if (fixedAction != null)
            return fixedAction;

        // 몬스터별 특수 우선순위 규칙(예: 시계토끼의 "취약 다음 공격").
        // 기본 구현은 아무 규칙도 강제하지 않는다.
        EnemyActionRuntime forcedFollowUpAction =
            FindForcedFollowUpAction(targetTurn, currentPhase);

        if (forcedFollowUpAction != null)
            return forcedFollowUpAction;

        return SelectRandomAction(targetTurn, currentPhase);
    }

    /// <summary>
    /// 몬스터별로 특정 조건에서 행동을 강제하고 싶을 때 하위 클래스에서 재정의한다.
    /// 기본 구현은 항상 null을 반환해 아무 것도 강제하지 않는다(FixedTurn/RandomPool만 사용).
    /// FixedTurn보다는 우선순위가 낮게 호출되므로, 재정의하더라도 턴 고정 행동을 덮어쓰지 않는다.
    /// </summary>
    protected virtual EnemyActionRuntime FindForcedFollowUpAction(
        int targetTurn,
        int currentPhase)
    {
        return null;
    }

    protected EnemyActionRuntime FindActionUsedAtTurn(int turn)
    {
        foreach (EnemyActionRuntime action in runtimeActions)
        {
            if (action.LastUsedTurn == turn)
                return action;
        }

        return null;
    }

    protected EnemyActionRuntime FindRandomPoolAttack(
        int targetTurn,
        int currentPhase)
    {
        foreach (EnemyActionRuntime action in runtimeActions)
        {
            if (action.Data.SelectionType !=
                EnemyActionSelectionType.RandomPool)
            {
                continue;
            }

            if (action.Data.ActionType != EnemyActionType.Attack)
                continue;

            if (!action.IsAvailable(targetTurn, currentPhase))
                continue;

            return action;
        }

        return null;
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

        Debug.Log(
            $"[EnemyActionController] {name} {targetTurn}턴 후보 " +
            $"({candidates.Count}개): " +
            $"{string.Join(", ", candidates.ConvertAll(c => c.Data.ActionName))}");

        int randomIndex = UnityEngine.Random.Range(
            0,
            candidates.Count);

        return candidates[randomIndex];
    }
}