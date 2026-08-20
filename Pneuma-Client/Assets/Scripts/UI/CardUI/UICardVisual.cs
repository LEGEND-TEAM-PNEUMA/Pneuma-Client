using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pneuma.UI.Card
{
    // 카드의 데이터를 UI로 표기하는 컴포넌트입니다.
    public class UICardVisual : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image illustImage;

        /// <summary>
        /// 카드 데이터를 받아 이름, 코스트, 설명, 일러스트 등 UI를 갱신합니다.
        /// </summary>
        /// <param name="cardData">표기할 카드 데이터</param>
        public void SetCard(CardData cardData)
        {
            if (nameText != null) nameText.text = cardData.CardName;
            if (costText != null) costText.text = cardData.CardCost.ToString();
            if (descriptionText != null) descriptionText.text = cardData.CardDescription;
            if (illustImage != null && cardData.CardIllust != null) illustImage.sprite = cardData.CardIllust;

            Debug.Log($"[UICardVisual] 카드 표기: {cardData.CardName} (코스트 {cardData.CardCost}) - {cardData.CardDescription}");
        }
    }

}
