using System;
using System.Collections.Generic;

namespace Pneuma.Card.Management
{
    /// <summary>
    /// 드로우 더미와 버린 카드 더미를 관리합니다.
    /// </summary>
    public class DeckManager
    {
        private readonly List<CardInstance> drawPile = new List<CardInstance>();
        private readonly List<CardInstance> discardPile = new List<CardInstance>();
        private readonly Random random;

        public IReadOnlyList<CardInstance> DrawPile => drawPile;
        public IReadOnlyList<CardInstance> DiscardPile => discardPile;
        public int DrawPileCount => drawPile.Count;
        public int DiscardPileCount => discardPile.Count;

        public DeckManager(Random random = null)
        {
            this.random = random ?? new Random();
        }

        /// <summary>
        /// 전달받은 카드 데이터로 드로우 더미를 초기화하고 버린 카드 더미를 비웁니다.
        /// 셔플은 호출자가 별도로 수행합니다.
        /// </summary>
        public void Initialize(IEnumerable<CardData> cards)
        {
            drawPile.Clear();
            discardPile.Clear();

            if (cards == null)
            {
                return;
            }

            foreach (CardData cardData in cards)
            {
                if (cardData == null)
                {
                    continue;
                }

                drawPile.Add(new CardInstance(cardData));
            }
        }

        /// <summary>
        /// 현재 드로우 더미의 카드 순서를 무작위로 섞습니다.
        /// </summary>
        public void Shuffle()
        {
            Shuffle(drawPile);
        }

        /// <summary>
        /// 드로우 더미에서 카드 한 장을 꺼냅니다.
        /// 드로우 더미가 비어 있으면 버린 카드 더미를 드로우 더미로 재구성하고 셔플합니다.
        /// </summary>
        public bool TryDraw(out CardInstance card)
        {
            if (drawPile.Count == 0)
            {
                RebuildDrawPile();
            }

            if (drawPile.Count == 0)
            {
                card = null;
                return false;
            }

            int lastIndex = drawPile.Count - 1;
            card = drawPile[lastIndex];
            drawPile.RemoveAt(lastIndex);
            return true;
        }

        /// <summary>
        /// 카드를 버린 카드 더미로 이동합니다.
        /// </summary>
        public void Discard(CardInstance card)
        {
            if (card == null || discardPile.Contains(card))
            {
                return;
            }

            discardPile.Add(card);
        }

        private void RebuildDrawPile()
        {
            if (discardPile.Count == 0)
            {
                return;
            }

            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle();
        }

        private void Shuffle(List<CardInstance> cards)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                CardInstance temporaryCard = cards[i];
                cards[i] = cards[swapIndex];
                cards[swapIndex] = temporaryCard;
            }
        }
    }
}
