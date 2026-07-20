using System.Collections.Generic;
using Battle;
using Pneuma.Unit;
using UnityEngine;

public class _TestCardUseController : MonoBehaviour
{
    [Header("Card")]
    [SerializeField] private CardData testCardData;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private Enemy testEnemy;

    private readonly CardExecutor executor = new CardExecutor();
    private readonly List<CardInstance> displayedCards = new List<CardInstance>();
    private readonly List<CardView> displayedViews = new List<CardView>();

    private void Start()
    {
        DisplayCard();
    }

    public void DisplayCard()
    {
        if (testCardData == null)
        {
            return;
        }

        CardInstance card = new CardInstance(testCardData);
        CardView cardView = CreateCardView();

        if (cardView != null)
        {
            cardView.Bind(card);
            displayedViews.Add(cardView);
        }

        displayedCards.Add(card);
        Debug.Log($"카드 표시: {card.CardName}");
    }

    public void UseCurrentCard()
    {
        if (displayedCards.Count == 0)
        {
            DisplayCard();
        }

        if (displayedCards.Count == 0)
        {
            return;
        }

        int lastIndex = displayedCards.Count - 1;
        CardInstance card = displayedCards[lastIndex];

        Debug.Log($"카드 사용: {card.CardName}");
        executor.Execute(card, testEnemy);
        displayedCards.RemoveAt(lastIndex);

        if (lastIndex < displayedViews.Count)
        {
            CardView cardView = displayedViews[lastIndex];
            displayedViews.RemoveAt(lastIndex);

            if (cardView != null)
            {
                Destroy(cardView.gameObject);
            }
        }

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ChangeState(BattleState.EnemyTurn);
        }
    }

    private CardView CreateCardView()
    {
        if (cardViewPrefab != null)
        {
            Transform parent = cardParent != null ? cardParent : transform;
            return Instantiate(cardViewPrefab, parent);
        }

        return null;
    }
}
