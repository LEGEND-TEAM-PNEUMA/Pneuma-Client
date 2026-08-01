using System;
using System.Collections.Generic;

namespace Pneuma.Card.Management
{
    /// <summary>
    /// 덱, 손패, 버린 카드 더미 사이의 카드 순환을 조정합니다.
    /// 카드 효과 실행은 이 클래스의 책임이 아니며 호출자가 처리합니다.
    /// </summary>
    public class CardCycleManager
    {
        private readonly DeckManager deckManager;
        private readonly HandManager handManager;

        public IReadOnlyList<CardInstance> DrawPile => deckManager.DrawPile;
        public IReadOnlyList<CardInstance> Hand => handManager.Cards;
        public IReadOnlyList<CardInstance> DiscardPile => deckManager.DiscardPile;
        public int DrawPileCount => deckManager.DrawPileCount;
        public int HandCount => handManager.Count;
        public int DiscardPileCount => deckManager.DiscardPileCount;

        public CardCycleManager(Random random = null)
        {
            deckManager = new DeckManager(random);
            handManager = new HandManager();
        }

        /// <summary>
        /// 손패를 비우고 전달받은 카드 데이터로 덱을 초기화합니다.
        /// </summary>
        public void InitializeDeck(IEnumerable<CardData> cards, bool shuffle = true)
        {
            handManager.Clear();
            deckManager.Initialize(cards);

            if (shuffle)
            {
                deckManager.Shuffle();
            }
        }

        /// <summary>
        /// 현재 드로우 더미를 섞습니다.
        /// </summary>
        public void ShuffleDeck()
        {
            deckManager.Shuffle();
        }

        /// <summary>
        /// 손패 여유 범위 안에서 지정한 수만큼 카드를 드로우하고, 드로우한 목록을 반환합니다.
        /// </summary>
        public IReadOnlyList<CardInstance> DrawCards(int count)
        {
            List<CardInstance> drawnCards = new List<CardInstance>();
            int drawCount = Math.Min(count, handManager.AvailableCapacity);

            for (int i = 0; i < drawCount; i++)
            {
                if (!deckManager.TryDraw(out CardInstance card))
                {
                    break;
                }

                if (!handManager.TryAdd(card))
                {
                    deckManager.Discard(card);
                    break;
                }

                drawnCards.Add(card);
            }

            return drawnCards;
        }

        /// <summary>
        /// 사용한 카드를 손패에서 제거하고 버린 카드 더미로 이동합니다.
        /// 카드 효과 실행은 호출자가 별도로 처리합니다.
        /// </summary>
        public bool UseCard(CardInstance card)
        {
            if (!handManager.Remove(card))
            {
                return false;
            }

            deckManager.Discard(card);
            return true;
        }

        /// <summary>
        /// 손패 전체를 버린 카드 더미로 이동하고 폐기한 카드 수를 반환합니다.
        /// </summary>
        public int DiscardHand()
        {
            List<CardInstance> discardedCards = handManager.TakeAll();

            foreach (CardInstance card in discardedCards)
            {
                deckManager.Discard(card);
            }

            return discardedCards.Count;
        }
    }
}
