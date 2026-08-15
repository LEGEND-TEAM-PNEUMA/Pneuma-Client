using Pneuma.Unit;
using UnityEngine;

/// <summary>
/// 시계토끼 전용 행동 우선순위 규칙을 담당한다.
/// 취약(Debuff)을 사용한 다음 턴은 반드시 공격을 선택한다.
/// FixedTurn(예: 7턴 고정 공격, 8턴 도주)이 항상 우선이므로
/// 이 규칙이 턴 고정 행동을 덮어쓰는 일은 없다.
/// </summary>
public sealed class ClockRabbitActionController : EnemyActionController
{
    protected override EnemyActionRuntime FindForcedFollowUpAction(
        int targetTurn,
        int currentPhase)
    {
        EnemyActionRuntime lastExecutedAction =
            FindActionUsedAtTurn(targetTurn - 1);

        if (lastExecutedAction == null ||
            lastExecutedAction.Data.ActionType != EnemyActionType.Debuff)
        {
            return null;
        }

        EnemyActionRuntime forcedAttack =
            FindRandomPoolAttack(targetTurn, currentPhase);

        if (forcedAttack == null)
        {
            Debug.LogWarning(
                $"[ClockRabbitActionController] {name}: " +
                $"{targetTurn}턴 취약 이후 강제 공격 대상을 찾지 못했습니다. " +
                "RandomPool 선택으로 대체합니다.");

            return null;
        }

        Debug.Log(
            $"[ClockRabbitActionController] {name} {targetTurn}턴: " +
            $"직전 턴 취약 사용으로 공격 강제 선택 " +
            $"({forcedAttack.Data.ActionName})");

        return forcedAttack;
    }
}
