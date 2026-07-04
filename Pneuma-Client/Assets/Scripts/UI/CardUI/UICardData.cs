using UnityEngine;

namespace Pneuma.UI.Card
{
    // 현재 UI 카드 오브젝트가 가지고 있는 카드 데이터와 애니메이션 코드를 관리하는 컴포넌트입니다.
    // 표기(UICardVisual)·연출(UICardAnimator)은 자식 Visual 오브젝트에 위치합니다.
    public class UICardData : MonoBehaviour
    {
        [SerializeField] private CardData cardData;

        public CardData CardData => cardData;

        private UICardVisual cardVisual;
        private UICardAnimator cardAnimator;

        private void Start()
        {
            cardVisual = GetComponentInChildren<UICardVisual>();
            cardAnimator = GetComponentInChildren<UICardAnimator>();

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