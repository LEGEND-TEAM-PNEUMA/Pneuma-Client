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
    }
}