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
            spawnedCards.Add(card, cardObject);
            handCards.Add(cardObject);

            Debug.Log($"[CardHandView] 카드 생성: {card.CardName} (손패 {spawnedCards.Count}장)");

            RefreshLayout();
        }

        private void DespawnCard(RuntimeCard card)
        {
            if (!spawnedCards.TryGetValue(card, out UICardData cardObject)) return;

            spawnedCards.Remove(card);
            handCards.Remove(cardObject);

            // TODO(#25): 퇴장 애니메이션 후 제거하도록 변경 (현재는 즉시 파괴)
            if (cardObject != null)
                Destroy(cardObject.gameObject);

            Debug.Log($"[CardHandView] 카드 제거: {card.CardName} (손패 {spawnedCards.Count}장)");

            // 제거로 생긴 빈자리를 남은 카드들이 메우도록 다시 정렬한다.
            RefreshLayout();
        }

        // 손패 구성이 바뀔 때마다 정렬을 갱신한다.
        private void RefreshLayout()
        {
            if (handLayout == null) return;

            handLayout.UpdateLayout(handCards);
        }
    }

}
