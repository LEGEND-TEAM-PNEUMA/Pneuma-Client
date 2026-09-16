namespace Battle
{
    public enum BattleState
    {
        None,        // 초기 상태
        Start,       // 전투 시작 연출(배틀 인트로)
        PlayerTurn, // 플레이어 턴 (드로우, 행동 선택)
        Action,      // 플레이어가 행동을 선택한 후 실제 효과와 연출을 처리하는 상태
        EnemyTurn,  // 적 턴
        Victory,         // 승리
        Defeat,         // 패배
        Escape
    }
}