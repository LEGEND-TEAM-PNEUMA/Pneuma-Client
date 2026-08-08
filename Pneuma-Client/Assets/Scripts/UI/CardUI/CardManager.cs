using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pneuma.UI.Card
{
    public enum CardListType
    {
        DRAW = 0,
        HAND,
        DISCARD,
        EXHAUST,
        PERMANENT
    }

    // 카드 더미와 드로우를 관리하는 모델입니다.
    // 화면(뷰)을 직접 알지 못하며, 카드 이동을 OnCardMoved 이벤트로만 알립니다.
    //
    // 기획서 3.1.2 / 3.2 기준:
    //  - 입장 시 덱을 셔플해 "뽑을 카드 더미"를 만든다.
    //  - 드로우는 무작위 추출이 아니라 더미 맨 위에서 순차로 뽑는다(셔플로 무작위성은 확보).
    //  - 손패는 최대 10장. 초과 드로우된 카드는 손패에 못 들어오고 곧바로 버림 더미로 간다.
    //  - 뽑을 더미가 비면 버림 더미를 셔플해 다시 채운 뒤 남은 드로우를 이어서 실행한다.
    public class CardManager : MonoBehaviour
    {
        // 시작 덱 구성 (에디터에서 CardData 에셋을 등록)
        [SerializeField] private List<CardData> startingDeck = new List<CardData>();

        [Header("드로우 규칙")]
        // 전투 입장·턴 시작 시 뽑는 기본 장수 (기획서 3.2.1 / 3.2.3)
        [SerializeField] private int handDrawCount = 5;

        // 손패 최대 보유 장수 (기획서 3.2.4)
        [SerializeField] private int maxHandSize = 10;

        // 씬을 단독 실행했을 때 Start에서 전투를 자동으로 시작할지 여부.
        // 실제 전투에서는 배틀 흐름이 StartBattle()을 직접 호출하므로 끕니다.
        [SerializeField] private bool autoStartOnPlay = true;

        // 카드들의 현재 상태 및 위치를 확인할 수 있는 딕셔너리 상태의 변수
        private Dictionary<CardListType, List<RuntimeCard>> cardListDictionary;

        /// <summary>
        /// 카드가 더미 사이를 이동할 때 발행됩니다. (카드, 출발 더미, 도착 더미)
        /// 뷰는 이 이벤트를 구독해 카드 오브젝트를 생성·제거합니다.
        /// </summary>
        public event Action<RuntimeCard, CardListType, CardListType> OnCardMoved;

        private void Awake()
        {
            InitializeDictionary();
        }

        private void Start()
        {
            if (autoStartOnPlay)
                StartBattle();
        }

        private void InitializeDictionary()
        {
            cardListDictionary = new Dictionary<CardListType, List<RuntimeCard>>();

            // CardListType의 모든 값에 대해 빈 리스트를 만든다.
            foreach (CardListType type in Enum.GetValues(typeof(CardListType)))
                cardListDictionary.Add(type, new List<RuntimeCard>());
        }

        /// <summary>
        /// 전투를 시작합니다. 덱을 구성·셔플하고 초기 손패를 뽑습니다. (기획서 3.1.2 / 3.2.1)
        /// </summary>
        [ContextMenu("테스트/전투 시작 (덱 구성 + 초기 드로우)")]
        public void StartBattle()
        {
            InitializeDeck();
            DrawCards(handDrawCount);
        }

        /// <summary>
        /// 플레이어 턴을 시작합니다. 남은 손패를 모두 버리고 새로 뽑습니다. (기획서 3.2.3)
        /// 턴 흐름(BattleManager)이 호출합니다.
        /// </summary>
        [ContextMenu("테스트/플레이어 턴 시작 (손패 버리고 재드로우)")]
        public void StartPlayerTurn()
        {
            DiscardHand();
            DrawCards(handDrawCount);
        }

        /// <summary>
        /// 시작 덱의 CardData를 런타임 카드로 만들어 뽑을 더미에 채우고 셔플합니다.
        /// 모든 더미를 비우므로 전투 시작 시 한 번만 호출합니다.
        /// </summary>
        public void InitializeDeck()
        {
            foreach (List<RuntimeCard> pile in cardListDictionary.Values)
                pile.Clear();

            List<RuntimeCard> drawPile = cardListDictionary[CardListType.DRAW];

            foreach (CardData data in startingDeck)
            {
                if (data == null) continue;
                drawPile.Add(new RuntimeCard(data));
            }

            Shuffle(drawPile);

            Debug.Log($"[CardManager] 덱 구성·셔플 완료 - {drawPile.Count}장");
        }

        /// <summary>
        /// 지정한 수만큼 카드를 뽑습니다. 뽑을 카드가 완전히 고갈되면 조기 종료합니다.
        /// </summary>
        /// <param name="count">뽑을 카드 수</param>
        public void DrawCards(int count)
        {
            Debug.Log($"[CardManager] 드로우 시작 - 덱 {cardListDictionary[CardListType.DRAW].Count}장 중 {count}장 뽑기");

            for (int i = 0; i < count; i++)
            {
                if (!DrawCard()) break;
            }

            Debug.Log($"[CardManager] 드로우 완료 - 손패 {cardListDictionary[CardListType.HAND].Count}장");
        }

        /// <summary>
        /// 뽑을 더미 맨 위 카드를 손패로 옮깁니다.
        /// 더미가 비었으면 버림 더미를 셔플해 다시 채웁니다. (기획서 3.2.5)
        /// 손패가 가득 찼으면 카드는 손패 대신 버림 더미로 갑니다. (기획서 3.2.4)
        /// </summary>
        /// <returns>뽑을 카드가 있어 실제로 이동했으면 true</returns>
        public bool DrawCard()
        {
            List<RuntimeCard> drawPile = cardListDictionary[CardListType.DRAW];

            // 뽑을 더미가 비면 버림 더미를 셔플해 다시 채운다. 둘 다 비면 더 뽑을 수 없다.
            if (drawPile.Count == 0 && !ReshuffleDiscardIntoDraw())
            {
                Debug.LogWarning("[CardManager] 뽑을 카드가 없습니다. (뽑을 더미·버림 더미 모두 비어있음)");
                return false;
            }

            // 셔플된 더미이므로 맨 위(마지막)에서 뽑는다. 앞에서 빼는 것보다 이동 비용이 적다.
            RuntimeCard card = drawPile[drawPile.Count - 1];

            // 손패가 가득 차면 손패로 못 들어오고 즉시 버림 더미로 간다(증발).
            List<RuntimeCard> handPile = cardListDictionary[CardListType.HAND];
            CardListType destination =
                handPile.Count >= maxHandSize ? CardListType.DISCARD : CardListType.HAND;

            if (destination == CardListType.DISCARD)
                Debug.Log($"[CardManager] 손패가 가득 차 카드가 증발합니다: {card.CardName}");

            MoveCard(CardListType.DRAW, destination, card);
            return true;
        }

        /// <summary>
        /// 손패의 카드를 버림 더미로 보냅니다.
        /// </summary>
        /// <param name="card">버릴 카드</param>
        public void DiscardCard(RuntimeCard card)
        {
            MoveCard(CardListType.HAND, CardListType.DISCARD, card);
        }

        /// <summary>
        /// 손패 전체를 버림 더미로 보냅니다. (턴 종료·시작 시)
        /// </summary>
        [ContextMenu("테스트/손패 전체 버리기")]
        public void DiscardHand()
        {
            List<RuntimeCard> handPile = cardListDictionary[CardListType.HAND];

            // 순회 도중 리스트가 줄어드니 뒤에서부터 옮긴다.
            for (int i = handPile.Count - 1; i >= 0; i--)
                MoveCard(CardListType.HAND, CardListType.DISCARD, handPile[i]);
        }

        /// <summary>
        /// 특정 더미의 카드 목록을 읽기 전용으로 반환합니다.
        /// </summary>
        /// <param name="type">조회할 더미 타입</param>
        public IReadOnlyList<RuntimeCard> GetCards(CardListType type) => cardListDictionary[type];

        // 버림 더미를 통째로 뽑을 더미로 옮겨 셔플합니다. 버림 더미가 비면 아무것도 하지 않습니다.
        private bool ReshuffleDiscardIntoDraw()
        {
            List<RuntimeCard> discardPile = cardListDictionary[CardListType.DISCARD];
            if (discardPile.Count == 0) return false;

            Debug.Log($"[CardManager] 재셔플 - 버림 더미 {discardPile.Count}장을 뽑을 더미로");

            // 순회 도중 리스트가 줄어드니 뒤에서부터 옮긴다.
            for (int i = discardPile.Count - 1; i >= 0; i--)
                MoveCard(CardListType.DISCARD, CardListType.DRAW, discardPile[i]);

            Shuffle(cardListDictionary[CardListType.DRAW]);
            return true;
        }

        // Fisher-Yates 셔플. 뒤에서부터 임의의 앞 카드와 교환한다.
        private void Shuffle(List<RuntimeCard> pile)
        {
            for (int i = pile.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (pile[i], pile[j]) = (pile[j], pile[i]);
            }
        }

        /// <summary>
        /// 카드 요소를 다른 리스트로 이동하는 함수
        /// </summary>
        /// <param name="from">시작 리스트</param>
        /// <param name="to">목적지 리스트</param>
        /// <param name="card">카드 요소</param>
        private void MoveCard(CardListType from, CardListType to, RuntimeCard card)
        {
            if (card == null) return;
            if (!cardListDictionary[from].Remove(card)) return;
            cardListDictionary[to].Add(card);

            OnCardMoved?.Invoke(card, from, to);
        }

#if UNITY_EDITOR
        // 씬에 테스트 버튼이 없어도 인스펙터에서 카드 흐름을 수동으로 돌려보기 위한 훅입니다.
        // 컴포넌트 헤더 우클릭 → "테스트/..." 메뉴. 플레이 중에도 동작합니다.
        // 빌드에는 포함되지 않습니다.

        [ContextMenu("테스트/카드 1장 뽑기")]
        private void EditorDrawOneCard()
        {
            // DrawCard는 bool을 반환해 ContextMenu에 직접 걸 수 없으므로 감싸둡니다.
            DrawCard();
        }

        [ContextMenu("테스트/더미 상태 로그")]
        private void EditorLogPileState()
        {
            if (cardListDictionary == null)
            {
                Debug.Log("[CardManager] 아직 초기화 전입니다. (플레이 중에만 상태가 있습니다)");
                return;
            }

            Debug.Log(
                $"[CardManager] 뽑을 {cardListDictionary[CardListType.DRAW].Count}장 · " +
                $"손패 {cardListDictionary[CardListType.HAND].Count}장 · " +
                $"버림 {cardListDictionary[CardListType.DISCARD].Count}장");
        }
#endif
    }

}
