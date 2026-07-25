using System.Collections.Generic;

public class HandManager
{
    public const int MaxHandSize = 10;

    private readonly List<CardInstance> cards = new List<CardInstance>();

    public IReadOnlyList<CardInstance> Cards => cards;
    public int Count => cards.Count;
    public int AvailableCapacity => MaxHandSize - cards.Count;

    public bool TryAdd(CardInstance card)
    {
        if (card == null || cards.Count >= MaxHandSize || cards.Contains(card))
        {
            return false;
        }

        cards.Add(card);
        return true;
    }

    public bool Remove(CardInstance card)
    {
        return card != null && cards.Remove(card);
    }

    public List<CardInstance> TakeAll()
    {
        List<CardInstance> removedCards = new List<CardInstance>(cards);
        cards.Clear();
        return removedCards;
    }

    public void Clear()
    {
        cards.Clear();
    }
}
