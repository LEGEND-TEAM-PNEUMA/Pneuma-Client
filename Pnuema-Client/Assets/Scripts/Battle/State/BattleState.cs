namespace Pneuma.Battle.State
{
    public enum BattleState
    {
        None,
        BattleStart,
        PlayerTurn,
        Action, // 공격, 스킬 사용 등 연출이 진행 중인 상태
        EnemyTurn,
        Victory,
        Defeat
    }
}