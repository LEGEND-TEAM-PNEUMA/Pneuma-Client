using System.Collections.Generic;

namespace Pneuma.Card.Management
{
    /// <summary>
    /// 플레이어 손패와 최대 손패 수를 관리합니다.
    /// </summary>
    public class HandManager
    {
        public const int MaxHandSize = 10;

        private readonly List<CardInstance> cards = new List<CardInstance>();

        public IReadOnlyList<CardInstance> Cards => cards;
        public int Count => cards.Count;
        public int AvailableCapacity => MaxHandSize - cards.Count;

        /// <summary>
        /// 손패에 카드를 추가합니다. 손패가 가득 찼거나 카드가 유효하지 않으면 추가하지 않습니다.
        /// </summary>
        public bool TryAdd(CardInstance card)
        {
            if (card == null || cards.Count >= MaxHandSize || cards.Contains(card))
            {
                return false;
            }

            cards.Add(card);
            return true;
        }

        /// <summary>
        /// 손패에서 지정한 카드를 제거합니다.
        /// </summary>
        public bool Remove(CardInstance card)
        {
            return card != null && cards.Remove(card);
        }

        /// <summary>
        /// 손패 전체를 반환하고 손패를 비웁니다.
        /// 반환된 카드는 호출자가 버린 카드 더미 등으로 이동시킵니다.
        /// </summary>
        public List<CardInstance> TakeAll()
        {
            List<CardInstance> removedCards = new List<CardInstance>(cards);
            cards.Clear();
            return removedCards;
        }

        /// <summary>
        /// 손패를 비웁니다.
        /// </summary>
        public void Clear()
        {
            cards.Clear();
        }
    }
}
