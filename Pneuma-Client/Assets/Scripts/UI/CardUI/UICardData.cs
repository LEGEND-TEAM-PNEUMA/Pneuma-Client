using UnityEngine;

namespace Pneuma.UI.Card
{
    // 현재 UI 카드 오브젝트가 가지고 있는 카드 데이터와 애니메이션 코드를 관리하는 컴포넌트입니다.
    [RequireComponent(typeof(UICardVisual))]
    [RequireComponent(typeof(UICardAnimator))]
    public class UICardData : MonoBehaviour
    {
        [SerializeField] private CardData cardData;

        public CardData CardData => cardData;

        private UICardVisual cardVisual;
        private UICardAnimator cardAnimator;

        private void Start()
        {
            cardVisual = GetComponent<UICardVisual>();
            cardAnimator = GetComponent<UICardAnimator>();

            if (cardData == null)
            {
                Debug.LogWarning("[Warning] 카드 데이터가 존재하지 않습니다!");
                return;
            }

            // 카드 데이터를 UI에 표기하고 등장 애니메이션을 재생
            cardVisual.SetCard(cardData);
            cardAnimator.StartAnimation();
        }
    }

}