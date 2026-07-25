using System;
using System.Collections.Generic;

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

    public void InitializeDeck(IEnumerable<CardData> cards, bool shuffle = true)
    {
        handManager.Clear();
        deckManager.Initialize(cards);

        if (shuffle)
        {
            deckManager.Shuffle();
        }
    }

    public void ShuffleDeck()
    {
        deckManager.Shuffle();
    }

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

    public bool UseCard(CardInstance card)
    {
        if (!handManager.Remove(card))
        {
            return false;
        }

        deckManager.Discard(card);
        return true;
    }

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
