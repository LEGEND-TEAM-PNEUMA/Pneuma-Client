using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pneuma.UI.Card
{
    // 손패 카드의 포인터 입력(호버)과 사용 가능 여부 표시를 담당합니다.
    // 기획서 3.3.2 [1] 호버 인터랙션 기준입니다.
    //
    // 확대·상승 연출은 자식 Visual의 UICardAnimator에 위임하고,
    // 여기서는 "언제" 연출할지와 테두리·반투명 같은 상태 표시만 결정합니다.
    // 카드 사용은 클릭 선택이 아닌 드래그 앤 드롭이므로(3.3.2 [2]) 선택 상태는 두지 않습니다.
    [RequireComponent(typeof(UICardData))]
    public class UICardInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler
    {
        [Header("연결")]
        // 카드보다 조금 크게 깔려, 가장자리만 테두리처럼 드러나는 이미지.
        // 색상·두께는 기획 확정 전 임시값입니다. (두께는 이 오브젝트의 Width/Height로 조절)
        [SerializeField] private Image borderImage;

        [Header("테두리 색")]
        [SerializeField] private Color hoverColor = new Color(1f, 0.85f, 0.4f, 1f);

        // 코스트가 부족한 카드는 회색 계열 테두리로 구분합니다.
        [SerializeField] private Color disabledColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        [Header("사용 불가 표시")]
        // 코스트 부족 카드는 호버 여부와 무관하게 반투명을 유지합니다.
        [SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.5f;

        private UICardData cardData;
        private CanvasGroup canvasGroup;

        // 코스트가 모자라 사용할 수 없는 상태인지.
        // 코스트 시스템이 붙기 전까지는 아무도 false로 만들지 않습니다.
        private bool interactable = true;

        private bool isHovered;

        // 드래그 중 커서가 카드 밖으로 나간 상태. 드래그가 끝날 때 호버를 풀지 판단하는 데 씁니다.
        private bool unhoverOnDragEnd;

        // 호버로 맨 앞에 올리기 전의 렌더 순서. 정렬이 알려준 순서가 없을 때만 씁니다.
        private int siblingIndexBeforeHover = -1;

        private void Awake()
        {
            cardData = GetComponent<UICardData>();
            canvasGroup = GetComponent<CanvasGroup>();

            // 테두리는 호버할 때만 보인다.
            if (borderImage != null) borderImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// 카드를 사용할 수 있는지 지정합니다. 코스트가 부족하면 false를 넘깁니다.
        /// </summary>
        /// <param name="value">사용 가능 여부</param>
        public void SetInteractable(bool value)
        {
            interactable = value;

            // 반투명은 호버와 무관하게 유지되어야 사용 불가 카드를 한눈에 구분할 수 있다.
            if (canvasGroup != null)
                canvasGroup.alpha = value ? 1f : disabledAlpha;

            // 드래그로 사용하는 방식이므로, 사용 불가 카드는 집히지 않게 막는다.
            if (cardData.CardDrag != null)
                cardData.CardDrag.Interactable = value;

            // 이미 호버 중이라면 테두리 색을 즉시 새 상태에 맞춘다.
            if (isHovered) ApplyBorderColor();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 드래그 중 커서가 카드로 되돌아온 경우. 호버는 이미 켜져 있으니 해제 예약만 취소한다.
            unhoverOnDragEnd = false;

            if (isHovered) return;

            UICardAnimator animator = cardData.CardAnimator;

            // 등장 중에는 두 연출이 같은 값을 두고 다퉈 카드가 튀므로 호버를 받지 않는다.
            if (animator != null && animator.IsAppearing) return;

            isHovered = true;

            // 인접 카드에 가리지 않도록 맨 앞으로 올린다.
            // 정렬은 손패 리스트 순서를 쓰므로 렌더 순서를 바꿔도 배치는 흐트러지지 않는다.
            siblingIndexBeforeHover = transform.GetSiblingIndex();
            transform.SetAsLastSibling();

            ApplyBorderColor();
            if (borderImage != null) borderImage.gameObject.SetActive(true);

            if (animator != null) animator.PlayHover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isHovered) return;

            // 드래그 중에는 카드가 커서를 늦게 따라오느라 커서가 카드 밖으로 빠져나간다.
            // 이때 호버를 풀면 잡고 있는 카드가 갑자기 작아지므로, 판단을 드래그가 끝날 때까지 미룬다.
            if (cardData.CardDrag != null && cardData.CardDrag.IsDragging)
            {
                unhoverOnDragEnd = true;
                return;
            }

            Unhover();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 드래그를 놓은 시점에 커서가 카드 밖에 있었다면 그제서야 호버를 푼다.
            if (!unhoverOnDragEnd) return;

            unhoverOnDragEnd = false;
            Unhover();
        }

        private void Unhover()
        {
            if (!isHovered) return;

            isHovered = false;

            // 정렬이 알려준 손패 순서로 되돌린다.
            // 호버 중에 카드가 생기거나 사라져도 이 값이 최신이라 순서가 어긋나지 않는다.
            int restoreIndex = cardData.HandIndex >= 0 ? cardData.HandIndex : siblingIndexBeforeHover;
            if (restoreIndex >= 0) transform.SetSiblingIndex(restoreIndex);

            if (borderImage != null) borderImage.gameObject.SetActive(false);

            UICardAnimator animator = cardData.CardAnimator;
            if (animator != null) animator.PlayUnhover();
        }

        private void ApplyBorderColor()
        {
            if (borderImage == null) return;

            borderImage.color = interactable ? hoverColor : disabledColor;
        }
    }

}
