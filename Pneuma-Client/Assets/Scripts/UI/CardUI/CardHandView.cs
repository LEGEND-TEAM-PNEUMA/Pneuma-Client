using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pneuma.UI.Card
{
    // CardManager의 손패 변경을 받아 카드 오브젝트를 생성·제거하는 뷰입니다.
    // 생성·제거만 담당하고, 배치는 CardHandLayout에 위임합니다.
    public class CardHandView : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] private CardManager cardManager;

        // 부채꼴 정렬 담당. 없어도 카드 생성 자체는 동작합니다.
        [SerializeField] private CardHandLayout handLayout;

        // 스폰할 카드 프리팹 (UICardData가 붙어 있어야 함)
        [SerializeField] private UICardData cardPrefab;

        // 생성된 카드가 붙을 부모. 비워두면 자기 자신에 붙습니다.
        [SerializeField] private RectTransform handRoot;

        // 런타임 카드 → 화면에 생성된 카드 오브젝트 (조회용)
        private readonly Dictionary<RuntimeCard, UICardData> spawnedCards =
            new Dictionary<RuntimeCard, UICardData>();

        // 손패에 놓인 순서. Dictionary는 순서를 보장하지 않으므로 정렬용으로 따로 유지한다.
        private readonly List<UICardData> handCards = new List<UICardData>();

        // 지금 사용 처리 중인 카드. 사용으로 손패에서 빠질 때만 다른 퇴장 연출을 쓰기 위해 기억한다.
        private RuntimeCard usingCard;

        /// <summary>
        /// 손패 카드가 사용 영역에 드롭되었을 때 발행됩니다.
        /// 사용 가능 여부(턴·에너지) 판단은 구독자인 배틀 쪽이 맡습니다. 뷰는 판단하지 않습니다.
        /// </summary>
        public event Action<RuntimeCard> OnCardUseRequested;

        /// <summary>
        /// 손패 구성이 바뀌었을 때(카드 생성·제거) 발행됩니다.
        /// </summary>
        public event Action OnHandChanged;

        /// <summary>
        /// 현재 손패에 놓인 카드 오브젝트 목록입니다. (손패 순서)
        /// </summary>
        public IReadOnlyList<UICardData> HandCards => handCards;

        // CardManager가 Start()에서 드로우하므로, 그보다 먼저 실행되는 OnEnable에서 구독한다.
        private void OnEnable()
        {
            if (cardManager == null)
            {
                Debug.LogError("[CardHandView] CardManager가 연결되지 않았습니다.");
                return;
            }

            cardManager.OnCardMoved += HandleCardMoved;
        }

        private void OnDisable()
        {
            if (cardManager != null)
                cardManager.OnCardMoved -= HandleCardMoved;
        }

        // 손패로 들어오면 생성하고, 손패에서 빠지면 제거한다.
        private void HandleCardMoved(RuntimeCard card, CardListType from, CardListType to)
        {
            if (to == CardListType.HAND)
                SpawnCard(card);
            else if (from == CardListType.HAND)
                DespawnCard(card);
        }

        private void SpawnCard(RuntimeCard card)
        {
            if (cardPrefab == null)
            {
                Debug.LogError("[CardHandView] 카드 프리팹이 지정되지 않았습니다.");
                return;
            }

            if (spawnedCards.ContainsKey(card)) return;

            // worldPositionStays: false — UI는 부모 기준 로컬 좌표를 유지해야 위치·스케일이 틀어지지 않는다.
            Transform parent = handRoot != null ? handRoot : transform;
            UICardData cardObject = Instantiate(cardPrefab, parent, false);
            cardObject.Bind(card);
            cardObject.UseRequested += HandleUseRequested;
            cardObject.HoverChanged += HandleHoverChanged;
            spawnedCards.Add(card, cardObject);
            handCards.Add(cardObject);

            Debug.Log($"[CardHandView] 카드 생성: {card.CardName} (손패 {spawnedCards.Count}장)");

            RefreshLayout();
            OnHandChanged?.Invoke();
        }

        private void DespawnCard(RuntimeCard card)
        {
            if (!spawnedCards.TryGetValue(card, out UICardData cardObject)) return;

            // 연출이 끝나기 전에 목록에서 빼둔다. 그래야 남은 카드가 빈자리를 곧바로 메운다.
            spawnedCards.Remove(card);
            handCards.Remove(cardObject);

            // 퇴장 연출이 끝난 뒤에 파괴한다. 파괴는 여기서만 하고, 연출은 카드가 스스로 맡는다.
            if (cardObject != null)
            {
                cardObject.UseRequested -= HandleUseRequested;
                cardObject.HoverChanged -= HandleHoverChanged;

                // 호버 중이던 카드가 빠지면 이웃 카드의 벌어짐도 푼다.
                if (handLayout != null) handLayout.ClearHoveredCard(cardObject);

                // 사용으로 빠지는 카드는 그 자리에서 사라지고, 버려지는 카드는 아래로 내려간다.
                bool isUsed = card == usingCard;
                cardObject.PlayDisappear(() => Destroy(cardObject.gameObject), isUsed);
            }

            Debug.Log($"[CardHandView] 카드 제거: {card.CardName} (손패 {spawnedCards.Count}장)");

            // 제거로 생긴 빈자리를 남은 카드들이 메우도록 다시 정렬한다.
            RefreshLayout();
            OnHandChanged?.Invoke();
        }

        // 카드 호버 상태를 손패 정렬에 전달한다. 뷰는 전달만 하고, 얼마나 벌릴지는 정렬이 정한다.
        private void HandleHoverChanged(UICardData cardObject, bool isHovered)
        {
            if (handLayout == null) return;

            if (isHovered)
                handLayout.SetHoveredCard(cardObject);
            else
                handLayout.ClearHoveredCard(cardObject);
        }

        // 카드가 사용 영역에 드롭되면 런타임 카드로 바꿔 배틀 쪽에 전달한다.
        // 구독자가 사용을 확정하면 같은 호출 안에서 HandleCardMoved → DespawnCard가 실행되므로,
        // 그동안만 usingCard를 기억해 두면 사용 연출과 버림 연출을 구분할 수 있다.
        private void HandleUseRequested(UICardData cardObject)
        {
            if (cardObject == null || cardObject.RuntimeCard == null) return;

            usingCard = cardObject.RuntimeCard;
            OnCardUseRequested?.Invoke(usingCard);
            usingCard = null;
        }

        // 손패 구성이 바뀔 때마다 정렬을 갱신한다.
        private void RefreshLayout()
        {
            if (handLayout == null) return;

            handLayout.UpdateLayout(handCards);
        }
    }

}
