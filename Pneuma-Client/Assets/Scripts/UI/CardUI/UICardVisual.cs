using UnityEngine;

namespace Pneuma.UI.Card
{
    // 카드의 데이터를 UI로 표기하는 컴포넌트입니다.
    public class UICardVisual : MonoBehaviour
    {
        /// <summary>
        /// 카드 데이터를 받아 이름, 코스트, 설명, 일러스트 등 UI를 갱신합니다.
        /// </summary>
        /// <param name="cardData">표기할 카드 데이터</param>
        public void SetCard(CardData cardData)
        {
            // TODO: cardData.CardName, CardCost, CardDescription, CardIllust 등을 UI 요소에 반영
            Debug.Log($"[UICardVisual] 카드 표기: {cardData.CardName} (코스트 {cardData.CardCost}) - {cardData.CardDescription}");
        }
    }

}
