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

    // 런타임 공격력 보너스
    private int attackPowerBonus;

    public int AttackPowerBonus => attackPowerBonus;

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

        switch (actionData.ActionType)
        {
            case EnemyActionType.Attack:
                animationController?.PlayAttack();
                break;

            case EnemyActionType.Buff:
                attackPowerBonus++;
                // 공격력 강화는 별도 애니메이션을 사용하지 않는다.
                Debug.Log(
                    $"[ClockRabbitActionExecutor] {source.EnemyName} 공격력 강화 " +
                    $"+1 (현재 누적: +{attackPowerBonus})");
                break;

            case EnemyActionType.Debuff:
                // 시계토끼의 취약 부여 행동은 공격 애니메이션을 사용한다.
                animationController?.PlayAttack();
                break;

            case EnemyActionType.Unique:
                // 시계토끼의 Unique 행동은 현재 도주만 존재한다.
                animationController?.PlayRun();
                break;
        }

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

        // 현재 도주 처리 방식은 다음 커밋에서 별도로 수정한다.
        if (actionData.ActionType == EnemyActionType.Unique)
        {
            Debug.Log(
                $"[ClockRabbitActionExecutor] {source.EnemyName} 도주 - " +
                "보상 없는 승리 처리를 위해 즉시 사망 처리합니다.");

            source.Kill();
        }
    }
}