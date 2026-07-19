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
        EXHAUSTE,
        PREMANENT
    }

    // 카드 더미와 드로우를 관리하는 모델입니다.
    // 화면(뷰)을 직접 알지 못하며, 카드 이동을 OnCardMoved 이벤트로만 알립니다.
    public class CardManager : MonoBehaviour
    {
        // 시작 덱 구성 (에디터에서 CardData 에셋을 등록)
        [SerializeField] private List<CardData> startingDeck = new List<CardData>();

        // 시작 시 테스트로 뽑을 카드 수
        [SerializeField] private int testDrawCount = 3;

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
            // 테스트: 시작 시 덱을 구성하고 지정한 수만큼 드로우합니다.
            InitializeDeck();
            DrawCards(testDrawCount);
        }

        private void InitializeDictionary()
        {
            cardListDictionary = new Dictionary<CardListType, List<RuntimeCard>>();

            // 총 5개의 형태의 리스트 생성(드로우, 손패, 버림, 소멸, 영구)
            for (int i = 0; i < 5; i++)
                cardListDictionary.Add((CardListType)i, new List<RuntimeCard>());
        }

        /// <summary>
        /// 시작 덱의 CardData를 런타임 카드로 만들어 드로우 더미에 채웁니다.
        /// </summary>
        public void InitializeDeck()
        {
            List<RuntimeCard> drawPile = cardListDictionary[CardListType.DRAW];
            drawPile.Clear();

            foreach (CardData data in startingDeck)
            {
                if (data == null) continue;
                drawPile.Add(new RuntimeCard(data));
            }

            Debug.Log($"[CardManager] 덱 구성 완료 - {drawPile.Count}장");
        }

        /// <summary>
        /// 지정한 수만큼 카드를 뽑습니다.
        /// </summary>
        /// <param name="count">뽑을 카드 수</param>
        public void DrawCards(int count)
        {
            Debug.Log($"[CardManager] 드로우 시작 - 덱 {cardListDictionary[CardListType.DRAW].Count}장 중 {count}장 뽑기");

            for (int i = 0; i < count; i++)
                DrawCard();

            Debug.Log($"[CardManager] 드로우 완료 - 손패 {cardListDictionary[CardListType.HAND].Count}장");
        }

        // 랜덤한 드로우 더미의 카드를 손패로 이동합니다.
        public void DrawCard()
        {
            List<RuntimeCard> drawPile = cardListDictionary[CardListType.DRAW];
            if (drawPile.Count == 0)
            {
                Debug.LogWarning("[CardManager] 뽑을 카드가 없습니다. (DRAW 더미 비어있음)");
                return;
            }

            // 드로우 더미 중 랜덤하게 카드를 뽑음
            int rand = UnityEngine.Random.Range(0, drawPile.Count);
            MoveCard(CardListType.DRAW, CardListType.HAND, drawPile[rand]);
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
        /// 특정 더미의 카드 목록을 읽기 전용으로 반환합니다.
        /// </summary>
        /// <param name="type">조회할 더미 타입</param>
        public IReadOnlyList<RuntimeCard> GetCards(CardListType type) => cardListDictionary[type];

        public void SetCurrentCard()
        {
            // 현재 선택한 캐릭터의 기본 스킬을 적용함
            // 추후 캐릭터 타입에 맞춰 적용할 것
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

            Debug.Log($"[CardManager] 카드 이동: {card.CardName} ({from} → {to})");

            OnCardMoved?.Invoke(card, from, to);
        }
    }

}
