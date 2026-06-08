namespace Battle
{
    public enum BattleState
    {
        None,        // 초기 상태
        START,       // 전투 시작 연출(배틀 인트로)
        PLAYERTURN, // 플레이어 턴 (드로우, 행동 선택)
        ACTION,      // 플레이어가 행동을 선택한 후 실제 효과와 연출을 처리하는 상태
        ENEMYTURN,  // 적 턴
        VICTORY,         // 승리
        DEFEAT         // 패배
    }
}