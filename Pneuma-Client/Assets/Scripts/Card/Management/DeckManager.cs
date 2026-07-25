using System;
using System.Collections.Generic;

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

    public void Shuffle()
    {
        Shuffle(drawPile);
    }

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
