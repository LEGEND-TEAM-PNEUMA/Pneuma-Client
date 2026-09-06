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

        // 도주는 별도 처리
        if (actionData.ActionType == EnemyActionType.Unique)
        {
            yield return ExecuteEscape(source);
            yield break;
        }

        PlayActionAnimation(actionData);

        if (actionDelay > 0f)
        {
            yield return new WaitForSeconds(actionDelay);
        }

        ApplyActionEffect(
            source,
            target,
            actionData);

        Debug.Log(
            $"[ClockRabbitActionExecutor] " +
            $"{source.EnemyName} 행동 실행: " +
            $"{actionData.ActionName}, " +
            $"SkillGroupId: {actionData.SkillGroupId}");
    }

    // 애니메이션 처리
    private void PlayActionAnimation(
    EnemyActionData actionData)
    {
        switch (actionData.ActionType)
        {
            case EnemyActionType.Attack:
                animationController?.PlayAttack();
                break;

            case EnemyActionType.Buff:
                // 공격력 강화는 별도 애니메이션 없음
                break;

            case EnemyActionType.Debuff:
                animationController?.PlayAttack();
                break;
        }
    }

    // 실제 효과 처리
    private void ApplyActionEffect(
    Enemy source,
    Player target,
    EnemyActionData actionData)
    {
        switch (actionData.ActionType)
        {
            case EnemyActionType.Attack:
                ExecuteAttack(source, target);
                break;

            case EnemyActionType.Buff:
                ExecuteAttackBuff(source);
                break;

            case EnemyActionType.Debuff:
                ExecuteVulnerable(source, target);
                break;
        }
    }

    private void ExecuteAttack(
    Enemy source,
    Player target)
    {
        int finalDamage =
            baseAttackDamage + attackPowerBonus;

        target.TakeDamage(finalDamage);

        Debug.Log(
            $"[ClockRabbitActionExecutor] {source.EnemyName} 공격: " +
            $"{baseAttackDamage} + 강화 {attackPowerBonus} " +
            $"= {finalDamage} 피해");
    }

    private void ExecuteAttackBuff(Enemy source)
    {
        attackPowerBonus++;

        Debug.Log(
            $"[ClockRabbitActionExecutor] {source.EnemyName} 공격력 강화 " +
            $"+1 (현재 누적: +{attackPowerBonus})");
    }

    private void ExecuteVulnerable(
    Enemy source,
    Player target)
    {
        target.ApplyVulnerable();

        Debug.Log(
            $"[ClockRabbitActionExecutor] " +
            $"{source.EnemyName}이 " +
            $"{target.CharacterName}에게 취약 부여");
    }

    private IEnumerator ExecuteEscape(Enemy source)
    {
        Debug.Log(
            $"[ClockRabbitActionExecutor] " +
            $"{source.EnemyName} 도주 시작");

        if (animationController != null)
        {
            yield return animationController.PlayRunAndExit();
        }

        if (!source.IsDead)
        {
            source.Escape();
        }
    }
}