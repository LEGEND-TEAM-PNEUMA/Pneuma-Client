using System.Collections;
using Pneuma.Unit;
using UnityEngine;

public sealed class ClockRabbitActionExecutor : EnemyActionExecutor
{
    [Header("Animation")]
    [SerializeField]
    private RabbitAnimationController animationController;

    [Header("Temporary Test")]
    [SerializeField, Min(0f)]
    private float actionDelay = 0.5f;

    private void Awake()
    {
        if (animationController == null)
        {
            animationController =
                GetComponent<RabbitAnimationController>();
        }
    }

    public override IEnumerator Execute(
        Enemy source,
        Player target,
        EnemyActionData actionData)
    {
        if (source == null || source.IsDead)
        {
            yield break;
        }

        if (target == null || target.IsDead)
        {
            yield break;
        }

        if (actionData == null)
        {
            Debug.LogError(
                "[ClockRabbitActionExecutor] ActionData가 없습니다.");

            yield break;
        }

        animationController?.PlayAttack();

        if (actionDelay > 0f)
        {
            yield return new WaitForSeconds(actionDelay);
        }

        Debug.Log(
            $"[ClockRabbitActionExecutor] " +
            $"{source.EnemyName} 행동 실행: " +
            $"{actionData.ActionName}, " +
            $"SkillGroupId: {actionData.SkillGroupId}");

        // TODO:
        // SkillGroup 실행 시스템이 연결되면
        // actionData.SkillGroupId를 전달하여 실제 효과를 실행한다.

        // 도주(Unique) 처리: 보상 없는 승리로 취급하기 위해
        // 즉시 사망 처리해서 BattleManager의 기존 승리 판정(전멸)을 그대로 태운다.
        // 시계토끼는 Unique 타입 행동이 도주 하나뿐이라는 전제로 작성됨.
        // 이후 상태이상/소환 등 다른 Unique 행동이 추가되면
        // SkillGroupId 등으로 조건을 더 구체화해야 한다.
        if (actionData.ActionType == EnemyActionType.Unique)
        {
            Debug.Log(
                $"[ClockRabbitActionExecutor] {source.EnemyName} 도주 - " +
                "보상 없는 승리 처리를 위해 즉시 사망 처리합니다.");

            source.Kill();
        }
    }
}