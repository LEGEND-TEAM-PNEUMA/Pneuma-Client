using System.Collections.Generic;
using System.Text;
using Battle;
using Pneuma.Card.Management;
using Pneuma.Unit;
using TMPro;
using UnityEngine;

public class _TestCardUseController : MonoBehaviour
{
    [Header("Deck")]
    [SerializeField] private List<CardData> testDeck = new List<CardData>();
    [SerializeField, Min(1)] private int drawCount = 5;
    [SerializeField] private bool initializeOnStart = true;

    [Header("Card View")]
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private Enemy testEnemy;

    [Header("State View")]
    [SerializeField] private TMP_Text deckStateText;
    [SerializeField] private TMP_Text handStateText;
    [SerializeField] private TMP_Text discardStateText;

    private readonly CardCycleManager cardCycleManager = new CardCycleManager();
    private readonly CardExecutor executor = new CardExecutor();
    private readonly List<CardView> displayedViews = new List<CardView>();

    private void Start()
    {
        if (initializeOnStart)
        {
            InitializeDeck();
        }
    }

    public void InitializeDeck()
    {
        ClearDisplayedViews();
        cardCycleManager.InitializeDeck(testDeck);
        LogState("덱 초기화 및 셔플");
    }

    public void ShuffleDeck()
    {
        cardCycleManager.ShuffleDeck();
        LogState("덱 셔플");
    }

    public void DrawCards()
    {
        IReadOnlyList<CardInstance> drawnCards = cardCycleManager.DrawCards(drawCount);

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

        if (drawnCards.Count == 0)
        {
            Debug.Log("[CardCycleTest] 드로우할 수 있는 카드가 없습니다.");
        }

        LogState($"{drawnCards.Count}장 드로우");
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

        Debug.Log($"카드 사용: {card.CardName}");

        // 카드 순환 상태 변경은 CardCycleManager가 담당하고,
        // 실제 카드 효과 실행은 테스트 컨트롤러가 담당합니다.
        executor.Execute(card, testEnemy);

        cardView.Clicked -= UseCard;
        displayedViews.Remove(cardView);
        Destroy(cardView.gameObject);

        LogState("카드 사용");

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ChangeState(BattleState.EnemyTurn);
        }
    }

    public void DiscardHand()
    {
        int discardedCount = cardCycleManager.DiscardHand();
        ClearDisplayedViews();
        LogState($"손패 {discardedCount}장 폐기");
    }

    public void LogState()
    {
        LogState("상태 조회");
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

    private void ClearDisplayedViews()
    {
        for (int i = 0; i < displayedViews.Count; i++)
        {
            if (displayedViews[i] != null)
            {
                displayedViews[i].Clicked -= UseCard;
                Destroy(displayedViews[i].gameObject);
            }
        }

        displayedViews.Clear();
    }

    private void LogState(string action)
    {
        RefreshStateView();

        Debug.Log(
            $"[CardCycleTest] {action} | 덱: {cardCycleManager.DrawPileCount}, " +
            $"손패: {cardCycleManager.HandCount}, 버린 더미: {cardCycleManager.DiscardPileCount}");
    }

    private void RefreshStateView()
    {
        SetStateText(
            deckStateText,
            "DECK",
            cardCycleManager.DrawPile,
            $"{cardCycleManager.DrawPileCount} CARDS");

        SetStateText(
            handStateText,
            "HAND",
            cardCycleManager.Hand,
            $"{cardCycleManager.HandCount} / {HandManager.MaxHandSize}");

        SetStateText(
            discardStateText,
            "DISCARD",
            cardCycleManager.DiscardPile,
            $"{cardCycleManager.DiscardPileCount} CARDS");
    }

    private static void SetStateText(
        TMP_Text target,
        string title,
        IReadOnlyList<CardInstance> cards,
        string countLabel)
    {
        if (target == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.Append("<b>");
        builder.Append(title);
        builder.Append("</b>\n<size=64><b>");
        builder.Append(countLabel);
        builder.Append("</b></size>");

        if (cards.Count == 0)
        {
            builder.Append("\n\n<size=36><color=#B8B8B8>EMPTY</color></size>");
            target.text = builder.ToString();
            return;
        }

        Dictionary<string, int> cardCounts = new Dictionary<string, int>();
        List<string> cardOrder = new List<string>();

        for (int i = 0; i < cards.Count; i++)
        {
            CardInstance card = cards[i];
            string cardName = card?.Data != null ? card.CardName : "Unknown";

            if (!cardCounts.ContainsKey(cardName))
            {
                cardCounts.Add(cardName, 0);
                cardOrder.Add(cardName);
            }

            cardCounts[cardName]++;
        }

        builder.Append("\n");

        for (int i = 0; i < cardOrder.Count; i++)
        {
            string cardName = cardOrder[i];
            builder.Append("\n<size=36>");
            builder.Append(EscapeRichText(cardName));
            builder.Append("  ×");
            builder.Append(cardCounts[cardName]);
            builder.Append("</size>");
        }

        target.text = builder.ToString();
    }

    private static string EscapeRichText(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
