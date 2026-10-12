using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using Pneuma.Card.Management;
using Pneuma.UI.Card;
using Pneuma.Unit;
using UnityEngine;

namespace Pneuma.UI.Combat
{
    /// <summary>
    /// 드래그 앤 드롭 카드 UI(CardManager·CardHandView)를 실제 전투에 연결합니다.
    ///
    /// 역할 분담
    ///  - CardManager   : 카드 더미 모델 (뽑기·버리기·사용 이동)
    ///  - CardHandView  : 손패 카드 오브젝트 생성·제거, 드롭 사용 요청 전달
    ///  - 이 컴포넌트    : "써도 되는가" 판단(턴·에너지·처리 중) + 모션·효과 타이밍
    ///
    /// BattleManager는 BattleCardSystem 타입으로만 이 컴포넌트를 알기 때문에,
    /// 버튼형 BattleCardController와 서로 교체해도 BattleManager 코드는 바뀌지 않습니다.
    /// </summary>
    public class UIBattleCardController : BattleCardSystem
    {
        [Header("Card UI")]
        [SerializeField] private CardManager cardManager;
        [SerializeField] private CardHandView handView;

        [Header("Battle")]
        [Tooltip("비워두면 씬에서 찾습니다.")]
        [SerializeField] private BattleManager battleManager;

        [Tooltip("비워두면 살아 있는 첫 번째 적을 대상으로 합니다.")]
        [SerializeField] private Enemy targetEnemy;

        [Header("Character Motion")]
        [SerializeField] private LeilaAnimationController playerAnimation;

        [Header("Card Resolve Timing")]
        [Tooltip("공격 모션 시작 후 피해가 들어가기까지의 시간(초)")]
        [SerializeField, Min(0f)] private float attackHitDelay = 0.5f;

        [Tooltip("버프 모션 시작 후 효과가 적용되기까지의 시간(초)")]
        [SerializeField, Min(0f)] private float buffEffectDelay = 0.6f;

        [Tooltip("효과 적용 후 다음 카드를 쓸 수 있을 때까지의 시간(초)")]
        [SerializeField, Min(0f)] private float recoveryDelay = 0.5f;

        private readonly CardExecutor executor = new CardExecutor();
        private bool isResolvingCard;
        private Player subscribedPlayer;

        public override bool IsResolvingCard => isResolvingCard;

        /// <summary>
        /// 카드 사용이 거절되었을 때 이유와 함께 발행됩니다. (HUD 안내 문구용)
        /// </summary>
        public event Action<string> OnCardRejected;

        /// <summary>
        /// 카드 처리가 시작(true)·종료(false)될 때 발행됩니다. (턴 종료 버튼 잠금용)
        /// </summary>
        public event Action<bool> OnResolvingChanged;

        private Player CurrentPlayer => battleManager != null ? battleManager.CurrentPlayer : null;

        private void Awake()
        {
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();

            if (cardManager == null)
            {
                Debug.LogError("[UIBattleCardController] CardManager가 연결되지 않았습니다.");
                return;
            }

            // CardManager.Start의 자동 시작을 끈다.
            // 모든 Awake는 모든 Start보다 먼저 실행되므로 여기서 꺼야 덱이 두 번 만들어지지 않는다.
            cardManager.DisableAutoStart();
        }

        private void OnEnable()
        {
            if (handView != null)
            {
                handView.OnCardUseRequested += HandleCardUseRequested;
                handView.OnHandChanged += RefreshCardInteractables;
            }
            else
            {
                Debug.LogError("[UIBattleCardController] CardHandView가 연결되지 않았습니다.");
            }

            if (battleManager != null)
                battleManager.OnBattleStateChanged += HandleBattleStateChanged;

            SubscribePlayer();
        }

        private void OnDisable()
        {
            if (handView != null)
            {
                handView.OnCardUseRequested -= HandleCardUseRequested;
                handView.OnHandChanged -= RefreshCardInteractables;
            }

            if (battleManager != null)
                battleManager.OnBattleStateChanged -= HandleBattleStateChanged;

            UnsubscribePlayer();
        }

        #region BattleCardSystem

        public override void InitializeBattleDeck()
        {
            if (cardManager == null) return;

            cardManager.InitializeDeck();
        }

        public override void DrawForPlayerTurn()
        {
            if (cardManager == null) return;

            cardManager.DrawTurnHand();
            RefreshCardInteractables();
        }

        public override void DiscardHand()
        {
            if (cardManager == null) return;

            cardManager.DiscardHand();
        }

        #endregion

        // 카드가 사용 영역에 드롭되었을 때. 여기서 거절하면 카드는 손패 자리로 돌아간다.
        private void HandleCardUseRequested(RuntimeCard card)
        {
            if (card == null || card.Data == null) return;

            if (isResolvingCard)
            {
                Reject("다른 카드를 처리하는 중입니다.");
                return;
            }

            if (!IsPlayerTurn())
            {
                Reject("지금은 카드를 사용할 수 없습니다.");
                return;
            }

            int cost = card.Data.CardCost;
            Player player = CurrentPlayer;

            if (player != null && !player.TryUseEnergy(cost))
            {
                Reject("에너지가 부족합니다.");
                return;
            }

            // 손패에서 빼면 CardHandView가 같은 호출 안에서 카드 오브젝트를 사용 연출과 함께 치운다.
            if (!cardManager.UseCard(card))
            {
                // 손패에 없는 카드였다면 이미 쓴 에너지를 돌려준다.
                if (player != null) player.RecoverEnergy(cost);

                Debug.LogWarning($"[UIBattleCardController] 손패에 없는 카드입니다: {card.CardName}");
                return;
            }

            StartCoroutine(ResolveCardRoutine(card));
        }

        /// <summary>
        /// 카드 한 장의 연출과 효과를 순서대로 처리합니다.
        /// 모션 → 타격 시점 대기 → 효과 적용 → 마무리 대기 → 입력 잠금 해제
        /// </summary>
        private IEnumerator ResolveCardRoutine(RuntimeCard card)
        {
            SetResolving(true);

            float effectDelay = PlayCardMotion(card.Data);

            if (effectDelay > 0f)
                yield return new WaitForSeconds(effectDelay);

            executor.Execute(card.Data, ResolveTarget());

            if (recoveryDelay > 0f)
                yield return new WaitForSeconds(recoveryDelay);

            SetResolving(false);

            Debug.Log($"[UIBattleCardController] 카드 처리 완료: {card.CardName}");
        }

        private void SetResolving(bool value)
        {
            isResolvingCard = value;
            RefreshCardInteractables();
            OnResolvingChanged?.Invoke(value);
        }

        /// <summary>
        /// 카드 타입에 맞는 플레이어 모션을 재생하고, 효과 적용까지 기다릴 시간을 반환합니다.
        /// </summary>
        private float PlayCardMotion(CardData data)
        {
            if (data == null) return 0f;

            if (playerAnimation == null)
            {
                Debug.LogWarning("[UIBattleCardController] Player 애니메이션이 연결되지 않았습니다.");
                return 0f;
            }

            switch (data.CardType)
            {
                case CardType.Attack:
                    playerAnimation.PlayAttack();
                    return attackHitDelay;

                default:
                    playerAnimation.PlayBuff();
                    return buffEffectDelay;
            }
        }

        // 지정된 대상이 쓰러졌으면 살아 있는 다음 적을 고른다.
        private Enemy ResolveTarget()
        {
            if (targetEnemy != null && !targetEnemy.IsDead)
                return targetEnemy;

            if (battleManager == null) return null;

            IReadOnlyList<Enemy> enemies = battleManager.Enemies;

            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != null && !enemies[i].IsDead)
                    return enemies[i];
            }

            return null;
        }

        /// <summary>
        /// 손패 카드마다 사용 가능 여부를 다시 표시합니다.
        ///  - 코스트 부족 : 반투명 + 집을 수 없음
        ///  - 처리 중/적 턴 : 반투명 없이 집을 수만 없음 (모든 카드가 깜빡이는 것을 막기 위해)
        /// </summary>
        private void RefreshCardInteractables()
        {
            if (handView == null) return;

            Player player = CurrentPlayer;
            bool canAct = IsPlayerTurn() && !isResolvingCard;
            IReadOnlyList<UICardData> cards = handView.HandCards;

            for (int i = 0; i < cards.Count; i++)
            {
                UICardData cardObject = cards[i];
                if (cardObject == null || cardObject.RuntimeCard == null) continue;

                CardData data = cardObject.RuntimeCard.Data;
                int cost = data != null ? data.CardCost : 0;
                bool affordable = player == null || player.CanUseEnergy(cost);

                if (cardObject.Interaction != null)
                    cardObject.Interaction.SetInteractable(affordable);

                if (!canAct && cardObject.CardDrag != null)
                    cardObject.CardDrag.Interactable = false;
            }
        }

        private bool IsPlayerTurn()
        {
            return battleManager == null || battleManager.CurrentState == BattleState.PlayerTurn;
        }

        private void Reject(string reason)
        {
            Debug.Log($"[UIBattleCardController] 카드 사용 거절: {reason}");
            OnCardRejected?.Invoke(reason);
        }

        private void HandleBattleStateChanged(BattleState state) => RefreshCardInteractables();

        private void HandleEnergyChanged(int current, int max) => RefreshCardInteractables();

        private void SubscribePlayer()
        {
            subscribedPlayer = CurrentPlayer;

            if (subscribedPlayer != null)
                subscribedPlayer.OnEnergyChanged += HandleEnergyChanged;
        }

        private void UnsubscribePlayer()
        {
            if (subscribedPlayer != null)
                subscribedPlayer.OnEnergyChanged -= HandleEnergyChanged;

            subscribedPlayer = null;
        }
    }
}
