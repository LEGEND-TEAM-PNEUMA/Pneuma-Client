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

    // 카드 드로우, 더미 및 카드 정렬을 관리합니다.
    public class CardManager : MonoBehaviour
    {
        // 현재 보유 중인 카드 목록
        private List<UICardData> currentCardList = new List<UICardData>();

        // 카드들의 현재 상태 및 위치를 확인할 수 있는 딕셔너리 상태의 변수
        private Dictionary<CardListType, List<UICardData>> cardListDictionary;

        // public event System.Action<UICardData> OnCardMoved;

        private void Awake()
        {
            InitializeDictionary();
        }

        /// <summary>
        /// 첫 게임 시작 시 손패 카드를 뽑는 함수입니다.
        /// </summary>
        /// <param name="count">처음 뽑는 카드 수</param>
        public void DrawCards(int count)
        {
            cardListDictionary[CardListType.DRAW] = new List<UICardData>(currentCardList);

            // 카드를 뽑으면 해당 카드는 handCardList로 이동
            // drawCardList에서는 제거
            for(int i = 0; i < count; i++)
            {
                DrawCard();
            }
        }

        private void InitializeDictionary()
        {
            cardListDictionary.Clear();

            // 총 4개의 형태의 리스트 생성(드로우, 손패, 더미, 소멸)
            for(int i = 0; i < 5; i++)
                cardListDictionary.Add((CardListType)i, new List<UICardData>());
        }

        // 랜덤한 보유 카드를 드로우 카드로 이동합니다.
        public void DrawCard()
        {
            // 보유중인 카드 중 랜덤하게 카드를 뽑음
            int rand = Random.Range(0, cardListDictionary[CardListType.DRAW].Count);
            UICardData card = cardListDictionary[CardListType.DRAW][rand];
            MoveCard(CardListType.DRAW, CardListType.HAND, card);
        }
        
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
        private void MoveCard(CardListType from, CardListType to, UICardData card)
        {
            if (!cardListDictionary[from].Remove(card)) return;
            cardListDictionary[to].Add(card);
        }

        /// <summary>
        /// 카드 요소가 가고자 하는 리스트에 존재하는지 확인(중복 방지)
        /// </summary>
        /// <param name="type">검사하고자 하는 카드 리스트의 타입</param>
        /// <param name="card">카드 요소</param>
        /// <returns></returns>
        //private bool IsContain(CardListType type, UICardData card) => cardListDictionary[type].Contains(card);

    }

}