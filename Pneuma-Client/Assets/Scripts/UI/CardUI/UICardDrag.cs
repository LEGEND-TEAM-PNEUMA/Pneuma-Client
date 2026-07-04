using UnityEngine;
using UnityEngine.EventSystems;

namespace Pneuma.UI.Card
{
    // 카드를 드래그하여 이동시킵니다.
    // 마우스를 즉시 따라가지 않고 약간 지연(관성)을 주며 따라갑니다.
    [RequireComponent(typeof(RectTransform))]
    public class UICardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Follow (지연 추적)")]
        [SerializeField] private float followTime = 0.18f;   // 클수록 더 천천히/늦게 따라옴

        [Header("Tilt (이동 시 기울기)")]
        [SerializeField] private float tiltAmount = 0.15f;   // 뒤처진 간격 → 각도 비율
        [SerializeField] private float maxTilt = 25f;        // 최대 기울기 각도
        [SerializeField] private float tiltSmoothTime = 0.1f; // 각도 보간 시간(클수록 부드럽게)

        private RectTransform rectTransform;
        private Canvas canvas;

        private Vector2 homePosition;      // 드래그 취소 시 복귀 위치
        private Vector2 targetPosition;    // 지연 추적 목표
        private Vector2 followVelocity;    // 위치 SmoothDamp 내부 속도
        private float tiltVelocity;        // 각도 SmoothDamp 내부 속도

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            targetPosition = rectTransform.anchoredPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            homePosition = rectTransform.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 즉시 이동하지 않고 "목표"만 갱신 → 실제 이동은 Update에서 지연 추적
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            targetPosition += eventData.delta / scale;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // TODO(#27): 드롭 위치에 따라 카드 사용/취소 판정. 지금은 원위치로 복귀.
            targetPosition = homePosition;
        }

        private void Update()
        {
            // 1) 위치: 목표를 부드럽게 추적 → 지연(관성) 발생
            rectTransform.anchoredPosition = Vector2.SmoothDamp(
                rectTransform.anchoredPosition, targetPosition, ref followVelocity, followTime);

            // 2) 회전: 목표와의 가로 간격(뒤처진 정도)에 비례해 기울임
            //    → 따라잡을수록 간격이 줄어 자연히 0으로 복귀
            float gap = targetPosition.x - rectTransform.anchoredPosition.x;
            float targetTilt = Mathf.Clamp(-gap * tiltAmount, -maxTilt, maxTilt);

            // 각도 자체도 부드럽게 보간해 스냅/떨림 제거 (SmoothDampAngle: 360도 wrap 처리)
            float newZ = Mathf.SmoothDampAngle(
                rectTransform.localEulerAngles.z, targetTilt, ref tiltVelocity, tiltSmoothTime);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, newZ);
        }
    }

}
