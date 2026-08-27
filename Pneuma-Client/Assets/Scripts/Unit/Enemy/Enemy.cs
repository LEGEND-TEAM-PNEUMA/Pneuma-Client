using System.Collections;
using UnityEngine;

namespace Pneuma.Unit
{
    /// <summary>
    /// Enemy 전용 데이터와 전투 상태를 관리한다.
    /// HP, Shield, Death 처리는 CharacterBase에서 담당한다.
    /// 행동 선택과 실행은 EnemyActionController에 위임한다.
    /// </summary>
    public class Enemy : CharacterBase
    {
        [Header("Enemy Data")]
        [SerializeField] private EnemyData enemyData;
        
        [Header("Enemy Action")]
        [SerializeField] private EnemyActionController actionController;

        public EnemyData EnemyData => enemyData;

        public string EnemyId => enemyData != null ? enemyData.EnemyId : string.Empty;
        public string EnemyName => enemyData != null ? enemyData.EnemyName : string.Empty;

        public int CurrentPhase { get; private set; } = 1;

        protected override void Awake()
        {
            if (enemyData == null)
            {
                Debug.LogError("[Enemy] EnemyData가 연결되지 않았습니다.");
                base.Awake();
                return;
            }

            InitializeStatus(enemyData.MaxHp);

            if (actionController == null)
            {
                Debug.LogError(
                    $"[Enemy] {EnemyName}: EnemyActionController가 연결되지 않았습니다.");
                return;
            }

            if (enemyData.BehaviorData == null)
            {
                Debug.LogError(
                    $"[Enemy] {EnemyName}: EnemyBehaviorData가 연결되지 않았습니다.");
                return;
            }

            actionController.Initialize(
                this,
                enemyData.BehaviorData);
        }

        /// <summary>
        /// 지정한 턴에 실행할 행동을 미리 결정한다.
        /// 플레이어가 해당 행동을 확인할 수 있도록 예견 UI도 갱신한다.
        /// </summary>
        public void PredictAction(int targetTurn)
        {
            if (IsDead)
                return;

            if (actionController == null)
                return;

            actionController.PredictAction(
                targetTurn,
                CurrentPhase);
        }

        /// <summary>
        /// 이전에 예견해 둔 행동을 실행한다.
        /// 공격 애니메이션과 효과 연출이 끝날 때까지 대기할 수 있도록
        /// 코루틴으로 제공한다.
        /// </summary>
        public IEnumerator ExecutePredictedAction(
            int currentTurn,
            Player target)
        {
            if (IsDead)
                yield break;

            if (target == null || target.IsDead)
                yield break;

            if (actionController == null)
                yield break;

            yield return actionController.ExecutePredictedAction(
                currentTurn,
                target);
        }

        /// <summary>
        /// 몬스터의 행동 페이즈를 변경한다.
        /// 시계 토끼는 1페이즈만 사용한다.
        /// </summary>
        public void ChangePhase(int newPhase)
        {
            if (newPhase < 1)
            {
                Debug.LogWarning(
                    $"[Enemy] {EnemyName}: Phase는 1 이상이어야 합니다.");
                return;
            }

            if (CurrentPhase == newPhase)
                return;

            CurrentPhase = newPhase;

            Debug.Log(
                $"[Enemy] {EnemyName} Phase Changed: {CurrentPhase}");
        }

        // OnEnemyTurnStarted 메서드 제거 : 역할 분리 
    }
}