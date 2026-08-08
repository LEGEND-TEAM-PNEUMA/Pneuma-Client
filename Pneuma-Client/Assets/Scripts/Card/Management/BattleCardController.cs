using System.Collections.Generic;
using Pneuma.Unit;
using UnityEngine;

namespace Pneuma.Card.Management
{
    /// <summary>
    /// 실제 전투 흐름에서 카드 순환 상태와 손패 UI 생성을 관리합니다.
    /// </summary>
    public class BattleCardController : MonoBehaviour
    {
        [Header("Deck")]
        [SerializeField] private List<CardData> startingDeck = new List<CardData>();
        [SerializeField, Min(1)] private int drawCountPerTurn = 5;

        [Header("Card View")]
        [SerializeField] private CardView cardViewPrefab;
        [SerializeField] private Transform cardParent;
        [SerializeField] private Enemy targetEnemy;

        private readonly CardCycleManager cardCycleManager = new CardCycleManager();
        private readonly CardExecutor executor = new CardExecutor();
        private readonly List<CardView> displayedViews = new List<CardView>();

        public IReadOnlyList<CardInstance> DrawPile => cardCycleManager.DrawPile;
        public IReadOnlyList<CardInstance> Hand => cardCycleManager.Hand;
        public IReadOnlyList<CardInstance> DiscardPile => cardCycleManager.DiscardPile;
        public int DrawPileCount => cardCycleManager.DrawPileCount;
        public int HandCount => cardCycleManager.HandCount;
        public int DiscardPileCount => cardCycleManager.DiscardPileCount;

        public void InitializeBattleDeck()
        {
            ClearDisplayedViews();
            cardCycleManager.InitializeDeck(startingDeck);

            Debug.Log($"[BattleCardController] 전투 덱 초기화 완료. 덱: {DrawPileCount}");
        }

        public void DrawForPlayerTurn()
        {
            IReadOnlyList<CardInstance> drawnCards = cardCycleManager.DrawCards(drawCountPerTurn);

            for (int i = 0; i < drawnCards.Count; i++)
            {
                CardView cardView = CreateCardView();

                if (cardView == null)
                {
                    continue;
                }

                cardView.Bind(drawnCards[i]);
                cardView.Clicked += UseCard;
                displayedViews.Add(cardView);
            }

            Debug.Log(
                $"[BattleCardController] 플레이어 턴 드로우 완료. " +
                $"드로우: {drawnCards.Count}, 덱: {DrawPileCount}, 손패: {HandCount}, 버린 더미: {DiscardPileCount}");
        }

        private void UseCard(CardView cardView)
        {
            if (cardView == null || cardView.BoundCard == null)
            {
                return;
            }

            CardInstance card = cardView.BoundCard;

            if (!cardCycleManager.UseCard(card))
            {
                return;
            }

            executor.Execute(card, targetEnemy);

            cardView.Clicked -= UseCard;
            displayedViews.Remove(cardView);
            Destroy(cardView.gameObject);

            Debug.Log(
                $"[BattleCardController] 카드 사용: {card.CardName}. " +
                $"덱: {DrawPileCount}, 손패: {HandCount}, 버린 더미: {DiscardPileCount}");
        }

        private CardView CreateCardView()
        {
            if (cardViewPrefab == null)
            {
                Debug.LogError("[BattleCardController] CardView 프리팹이 연결되지 않았습니다.");
                return null;
            }

            Transform parent = cardParent != null ? cardParent : transform;
            return Instantiate(cardViewPrefab, parent);
        }

        private void ClearDisplayedViews()
        {
            for (int i = 0; i < displayedViews.Count; i++)
            {
                if (displayedViews[i] == null)
                {
                    continue;
                }

                displayedViews[i].Clicked -= UseCard;
                Destroy(displayedViews[i].gameObject);
            }

            displayedViews.Clear();
        }

        private void OnDestroy()
        {
            ClearDisplayedViews();
        }
    }
}
