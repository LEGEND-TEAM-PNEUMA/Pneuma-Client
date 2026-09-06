using System.Collections.Generic;
using System.Text;
using Pneuma.Unit;
using TMPro;
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

        [Header("State View")]
        [SerializeField] private TMP_Text deckStateText;
        [SerializeField] private TMP_Text handStateText;
        [SerializeField] private TMP_Text discardStateText;

        private readonly CardCycleManager cardCycleManager = new CardCycleManager();
        private readonly CardExecutor executor = new CardExecutor();
        private readonly List<CardView> displayedViews = new List<CardView>();

        public IReadOnlyList<CardInstance> DrawPile => cardCycleManager.DrawPile;
        public IReadOnlyList<CardInstance> Hand => cardCycleManager.Hand;
        public IReadOnlyList<CardInstance> DiscardPile => cardCycleManager.DiscardPile;
        public int DrawPileCount => cardCycleManager.DrawPileCount;
        public int HandCount => cardCycleManager.HandCount;
        public int DiscardPileCount => cardCycleManager.DiscardPileCount;

        public void InitializeDeck()
        {
            InitializeBattleDeck();
        }

        public void InitializeBattleDeck()
        {
            ClearDisplayedViews();
            cardCycleManager.InitializeDeck(startingDeck);
            RefreshStateView();
            RefreshCardInteractables();

            Debug.Log($"[BattleCardController] 전투 덱 초기화 완료. 덱: {DrawPileCount}");
        }

        public void DrawCards()
        {
            DrawForPlayerTurn();
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

            RefreshStateView();
            RefreshCardInteractables();

            Debug.Log(
                $"[BattleCardController] 플레이어 턴 드로우 완료. " +
                $"드로우: {drawnCards.Count}, 덱: {DrawPileCount}, 손패: {HandCount}, 버린 더미: {DiscardPileCount}");
        }

        public void DiscardHand()
        {
            int discardedCount = cardCycleManager.DiscardHand();
            ClearDisplayedViews();
            RefreshStateView();

            Debug.Log(
                $"[BattleCardController] 손패 폐기 완료. " +
                $"폐기: {discardedCount}, 덱: {DrawPileCount}, 손패: {HandCount}, 버린 더미: {DiscardPileCount}");
        }

        private void UseCard(CardView cardView)
        {
            if (cardView == null || cardView.BoundCard == null)
            {
                return;
            }

            CardInstance card = cardView.BoundCard;
            Player currentPlayer = BattleManager.Instance != null ? BattleManager.Instance.CurrentPlayer : null;

            if (currentPlayer != null && !currentPlayer.TryUseEnergy(card.CurrentCost))
            {
                Debug.LogWarning($"[BattleCardController] 에너지가 부족해 카드를 사용할 수 없습니다: {card.CardName}");
                RefreshCardInteractables();
                return;
            }

            if (!cardCycleManager.UseCard(card))
            {
                return;
            }

            executor.Execute(card, targetEnemy);

            cardView.Clicked -= UseCard;
            displayedViews.Remove(cardView);
            Destroy(cardView.gameObject);
            RefreshStateView();
            RefreshCardInteractables();

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

        private void RefreshCardInteractables()
        {
            Player currentPlayer = BattleManager.Instance != null ? BattleManager.Instance.CurrentPlayer : null;

            for (int i = 0; i < displayedViews.Count; i++)
            {
                CardView cardView = displayedViews[i];

                if (cardView == null || cardView.BoundCard == null)
                {
                    continue;
                }

                bool canUse = currentPlayer == null || currentPlayer.CanUseEnergy(cardView.BoundCard.CurrentCost);
                cardView.SetInteractable(canUse);
            }
        }

        private void RefreshStateView()
        {
            SetStateText(
                deckStateText,
                "DECK",
                cardCycleManager.DrawPile,
                $"{DrawPileCount} CARDS");

            SetStateText(
                handStateText,
                "HAND",
                cardCycleManager.Hand,
                $"{HandCount} / {HandManager.MaxHandSize}");

            SetStateText(
                discardStateText,
                "DISCARD",
                cardCycleManager.DiscardPile,
                $"{DiscardPileCount} CARDS");
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
                builder.Append("  x");
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

        private void OnDestroy()
        {
            ClearDisplayedViews();
        }
    }
}
