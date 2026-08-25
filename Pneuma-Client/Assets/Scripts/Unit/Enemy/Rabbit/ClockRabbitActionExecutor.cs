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

    [SerializeField, Min(0)]
    private int baseAttackDamage = 3;


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
        // 애니메이션 → 대기 → 실제 효과

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

        // 1. 행동 애니메이션
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

        // 2. 행동 지연
        if (actionDelay > 0f)
        {
            yield return new WaitForSeconds(actionDelay);
        }

        Debug.Log(
            $"[ClockRabbitActionExecutor] " +
            $"{source.EnemyName} 행동 실행: " +
            $"{actionData.ActionName}, " +
            $"SkillGroupId: {actionData.SkillGroupId}");

         // 2. 실제 행동 효과
        switch (actionData.ActionType)
        {
            case EnemyActionType.Attack:
            {
                int finalDamage =
                    baseAttackDamage + attackPowerBonus;

                target.TakeDamage(finalDamage);

                Debug.Log(
                    $"[ClockRabbitActionExecutor] {source.EnemyName} 공격: " +
                    $"{baseAttackDamage} + 강화 {attackPowerBonus} " +
                    $"= {finalDamage} 피해");

                break;
            }

            case EnemyActionType.Buff:
                attackPowerBonus++;

                Debug.Log(
                    $"[ClockRabbitActionExecutor] {source.EnemyName} 공격력 강화 " +
                    $"+1 (현재 누적: +{attackPowerBonus})");

                break;

            case EnemyActionType.Debuff:
                // 다음 커밋에서 취약 적용
                break;

            case EnemyActionType.Unique:
                // 아직 기존 임시 도주 처리 유지
                source.Kill();
                break;
        }
    }
}