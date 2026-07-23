using UnityEngine;
using UnityEngine.EventSystems;

namespace Pneuma.UI.Card
{
    // 카드의 위치·회전을 소유하는 유일한 컴포넌트입니다.
    // 평소에는 정렬이 지정한 제자리(home)를, 드래그 중에는 마우스를 지연 추적합니다.
    // 정렬(CardHandLayout)은 위치를 직접 쓰지 않고 SetHome()으로 목표만 알려줍니다.
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

        private Vector2 homePosition;      // 정렬이 지정한 제자리
        private float homeRotation;        // 정렬이 지정한 부채꼴 각도
        private Vector2 dragTarget;        // 드래그 중 추적 목표
        private bool isDragging;

        private Vector2 followVelocity;    // 위치 SmoothDamp 내부 속도
        private float tiltVelocity;        // 각도 SmoothDamp 내부 속도

        /// <summary>
        /// 카드를 집을 수 있는지 여부입니다. 코스트가 부족하거나 퇴장 중인 카드는 false가 됩니다.
        /// 컴포넌트를 끄지 않고 이 값만 내리는 이유는, 꺼버리면 Update가 멈춰
        /// 정렬이 새 자리를 알려줘도 카드가 따라가지 못하기 때문입니다.
        /// </summary>
        public bool Interactable { get; set; } = true;

        /// <summary>
        /// 지금 드래그 중인지 여부입니다.
        /// 카드가 커서를 늦게 따라오는 탓에 드래그 중에도 커서가 카드 밖으로 나갈 수 있어,
        /// 호버 쪽에서 이 값을 보고 상태를 유지할지 판단합니다.
        /// </summary>
        public bool IsDragging => isDragging;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();

            // 정렬이 없는 경우(씬 배치 카드)에도 제자리를 유지하도록 현재 위치를 기본값으로 둔다.
            homePosition = rectTransform.anchoredPosition;
            homeRotation = rectTransform.localEulerAngles.z;
        }

        /// <summary>
        /// 정렬이 계산한 제자리를 지정합니다. 드래그 중이 아니면 카드가 이쪽으로 돌아갑니다.
        /// </summary>
        /// <param name="position">부모 기준 목표 위치(anchoredPosition)</param>
        /// <param name="rotation">부채꼴 각도(Z)</param>
        public void SetHome(Vector2 position, float rotation)
        {
            homePosition = position;
            homeRotation = rotation;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Interactable) return;

            // 현재 위치에서 시작해야 잡는 순간 카드가 튀지 않는다.
            dragTarget = rectTransform.anchoredPosition;
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // OnBeginDrag를 거절해도 OnDrag는 계속 들어오므로 여기서도 막는다.
            if (!isDragging) return;

            // 즉시 이동하지 않고 "목표"만 갱신 → 실제 이동은 Update에서 지연 추적
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            dragTarget += eventData.delta / scale;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // TODO(#27): 드롭 위치에 따라 카드 사용/취소 판정.
            // 지금은 드래그만 풀면 Update가 알아서 제자리(home)로 되돌린다.
            isDragging = false;
        }

        private void Update()
        {
            // 1) 위치: 드래그 중이면 마우스를, 아니면 정렬이 지정한 제자리를 추적
            Vector2 target = isDragging ? dragTarget : homePosition;

            rectTransform.anchoredPosition = Vector2.SmoothDamp(
                rectTransform.anchoredPosition, target, ref followVelocity, followTime);

            // 2) 회전: 목표와의 가로 간격(뒤처진 정도)에 비례해 기울임
            //    → 따라잡을수록 간격이 줄어 자연히 기준 각도로 복귀
            float gap = target.x - rectTransform.anchoredPosition.x;
            float lean = Mathf.Clamp(-gap * tiltAmount, -maxTilt, maxTilt);

            // 드래그 중에는 부채꼴 각도를 풀어 정면(0도)을 기준으로 삼는다.
            float baseRotation = isDragging ? 0f : homeRotation;

            // 각도 자체도 부드럽게 보간해 스냅/떨림 제거 (SmoothDampAngle: 360도 wrap 처리)
            float newZ = Mathf.SmoothDampAngle(
                rectTransform.localEulerAngles.z, baseRotation + lean, ref tiltVelocity, tiltSmoothTime);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, newZ);
        }
    }

}
