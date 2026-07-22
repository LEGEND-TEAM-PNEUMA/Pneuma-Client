using Pneuma.Unit;
using UnityEngine;
//행동 사용 횟수, 마지막 사용 턴 등 전투 중 상태
public class EnemyActionRuntime
{
    public EnemyActionData Data { get; }

    public int UseCount { get; private set; }
    public int LastUsedTurn { get; private set; } = -1;

    public EnemyActionRuntime(EnemyActionData data)
    {
        Data = data;
    }

    public bool IsAvailable(int targetTurn, int currentPhase) // 현재 턴과 페이즈에서 사용 가능한지 여부 확인
    {
        if (Data.Phase != currentPhase)
            return false;

        if (targetTurn < Data.MinAppearTurn)
            return false;

        if (Data.MaxAppearCount > 0 &&
            UseCount >= Data.MaxAppearCount)
        {
            return false;
        }

        if (LastUsedTurn >= 0 &&
            targetTurn - LastUsedTurn <= Data.CooldownTurns)
        {
            return false;
        }

        return true;
    }

    public void MarkUsed(int turn) // 사용 횟수 증가, 마지막 사용 턴 갱신
    {
        UseCount++;
        LastUsedTurn = turn;
    }
}