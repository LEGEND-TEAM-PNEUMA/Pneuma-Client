using UnityEngine;

namespace Pneuma.Unit
{
    /// <summary>
    /// Enemy 전용 데이터와 전투 상태를 관리한다.
    /// HP, Shield, Death 처리는 CharacterBase에서 담당한다.
    /// </summary>
    public class Enemy : CharacterBase
    {
        [Header("Enemy Data")]
        [SerializeField] private EnemyData enemyData;

        public EnemyData EnemyData => enemyData;

        public string EnemyId => enemyData != null ? enemyData.EnemyId : string.Empty;
        public string EnemyName => enemyData != null ? enemyData.EnemyName : string.Empty;

        protected override void Awake()
        {
            if (enemyData == null)
            {
                Debug.LogError("[Enemy] EnemyData가 연결되지 않았습니다.");
                base.Awake();
                return;
            }

            InitializeStatus(enemyData.MaxHp);
        }

        public void OnEnemyTurnStarted() // 적 턴 시작 시 예정되어 있던 행동 수행 후 다음 행동을 큐에 넣고 턴 종료
        {
            Debug.Log($"[Enemy] {EnemyName} Turn Started");
            // 행동 수행과 동시에 행동 이모지 페이드 아웃
            // 행동 수행 후 다음 행동 이모지 페이드 인 및 다음 행동 큐에 넣기
            // 턴 종료
        }
    }
}