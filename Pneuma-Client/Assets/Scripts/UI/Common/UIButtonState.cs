using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pneuma.UI.Common
{
    // 공통 버튼/요소의 4가지 상태 연출을 담당합니다. (UI 기획서 2.3)
    //
    //   기본   100% / 기본 색상·채도
    //   호버   106% + 밝기 +10%, 0.15초 ease-out
    //   클릭    96% + 밝기 -10%, 0.08초 ease-in
    //   비활성 채도 -60%, 불투명도 50%, 클릭 이벤트 미발생
    //
    // 수치는 인스펙터로 빼지 않고 UIMotionSpec을 참조합니다.
    // 공통 규칙이라 화면마다 값이 달라지면 안 되기 때문입니다.
    //
    // 비활성 판정은 같은 오브젝트의 Selectable(Button 등) interactable을 그대로 따릅니다.
    // 이미 붙어 있는 버튼에 이 컴포넌트만 얹으면 동작하도록 하기 위함입니다.
    [DisallowMultipleComponent]
    public class UIButtonState : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private enum VisualState
        {
            Default,
            Hover,
            Pressed,
            Disabled
        }

        [Header("연결")]
        [Tooltip("확대·축소할 대상. 비워두면 자기 자신을 사용합니다.")]
        [SerializeField] private RectTransform scaleTarget;

        [Tooltip("밝기·채도를 적용할 그래픽. 비워두면 자기 자신에서 찾습니다.")]
        [SerializeField] private Graphic tintTarget;

        [Tooltip("비활성 불투명도를 적용할 그룹. 비워두면 자기 자신에서 찾습니다. " +
                 "없으면 그래픽 알파에만 적용되어 라벨 텍스트는 흐려지지 않습니다.")]
        [SerializeField] private CanvasGroup fadeTarget;

        private Selectable selectable;

        private Vector3 baseScale;
        private Color baseColor;
        private float baseAlpha = 1f;

        private VisualState currentState = VisualState.Default;

        private bool isPointerInside;
        private bool isPointerDown;

        // Selectable.interactable은 코드에서 바뀌어도 알림이 없어 매 프레임 확인합니다.
        private bool lastInteractable = true;

        /// <summary>
        /// 버튼을 사용할 수 있는지 여부입니다. Selectable이 있으면 그쪽 값을 함께 갱신합니다.
        /// </summary>
        public bool Interactable
        {
            get => selectable == null || selectable.interactable;
            set
            {
                if (selectable != null)
                    selectable.interactable = value;

                lastInteractable = value;
                RefreshState(instant: false);
            }
        }

        private void Awake()
        {
            if (scaleTarget == null)
                scaleTarget = transform as RectTransform;

            if (tintTarget == null)
                tintTarget = GetComponent<Graphic>();

            if (fadeTarget == null)
                fadeTarget = GetComponent<CanvasGroup>();

            selectable = GetComponent<Selectable>();

            baseScale = scaleTarget != null ? scaleTarget.localScale : Vector3.one;

            if (tintTarget != null)
                baseColor = tintTarget.color;

            if (fadeTarget != null)
                baseAlpha = fadeTarget.alpha;

            lastInteractable = Interactable;
        }

        private void OnEnable()
        {
            // 비활성 상태로 켜지는 버튼이 기본 모습으로 한 프레임 보이지 않도록 즉시 반영합니다.
            isPointerInside = false;
            isPointerDown = false;
            RefreshState(instant: true);
        }

        private void Update()
        {
            bool interactable = Interactable;

            if (interactable == lastInteractable)
                return;

            lastInteractable = interactable;

            // 비활성으로 바뀐 순간 커서가 위에 있었다면 호버 상태가 남지 않도록 함께 정리합니다.
            if (!interactable)
            {
                isPointerDown = false;
                isPointerInside = false;
            }

            RefreshState(instant: false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
            RefreshState(instant: false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;

            // 버튼 밖에서 손을 뗐을 때 눌린 상태로 남지 않게 같이 내립니다.
            isPointerDown = false;
            RefreshState(instant: false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
            RefreshState(instant: false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointerDown = false;
            RefreshState(instant: false);
        }

        private void RefreshState(bool instant)
        {
            VisualState nextState = ResolveState();

            // 같은 상태로의 재진입은 트윈만 다시 깔아 연출이 끊기므로 걸러냅니다.
            if (nextState == currentState && !instant)
                return;

            currentState = nextState;
            ApplyState(currentState, instant);
        }

        private VisualState ResolveState()
        {
            if (!Interactable)
                return VisualState.Disabled;

            if (isPointerDown && isPointerInside)
                return VisualState.Pressed;

            return isPointerInside ? VisualState.Hover : VisualState.Default;
        }

        private void ApplyState(VisualState state, bool instant)
        {
            float scale;
            float duration;
            Ease ease;
            Color color;
            float alpha = baseAlpha;

            switch (state)
            {
                case VisualState.Hover:
                    scale = UIMotionSpec.ButtonHoverScale;
                    duration = UIMotionSpec.ButtonHoverDuration;
                    ease = UIMotionSpec.ButtonHoverEase;
                    color = UIMotionSpec.ApplyBrightness(baseColor, UIMotionSpec.ButtonHoverBrightness);
                    break;

                case VisualState.Pressed:
                    scale = UIMotionSpec.ButtonPressedScale;
                    duration = UIMotionSpec.ButtonPressedDuration;
                    ease = UIMotionSpec.ButtonPressedEase;
                    color = UIMotionSpec.ApplyBrightness(baseColor, UIMotionSpec.ButtonPressedBrightness);
                    break;

                case VisualState.Disabled:
                    // 기획서 2.3 비활성은 크기 변화가 없습니다. 채도와 불투명도만 낮춥니다.
                    scale = 1f;
                    duration = UIMotionSpec.ButtonHoverDuration;
                    ease = UIMotionSpec.ButtonHoverEase;
                    color = UIMotionSpec.ApplyDesaturation(baseColor, UIMotionSpec.ButtonDisabledDesaturation);
                    alpha = baseAlpha * UIMotionSpec.ButtonDisabledAlpha;
                    break;

                default:
                    scale = 1f;
                    duration = UIMotionSpec.ButtonHoverDuration;
                    ease = UIMotionSpec.ButtonHoverEase;
                    color = baseColor;
                    break;
            }

            ApplyScale(baseScale * scale, duration, ease, instant);
            ApplyColor(color, duration, ease, instant);
            ApplyAlpha(alpha, duration, ease, instant);
        }

        private void ApplyScale(Vector3 target, float duration, Ease ease, bool instant)
        {
            if (scaleTarget == null)
                return;

            scaleTarget.DOKill();

            if (instant)
            {
                scaleTarget.localScale = target;
                return;
            }

            scaleTarget.DOScale(target, duration).SetEase(ease);
        }

        private void ApplyColor(Color target, float duration, Ease ease, bool instant)
        {
            if (tintTarget == null)
                return;

            tintTarget.DOKill();

            if (instant)
            {
                tintTarget.color = target;
                return;
            }

            tintTarget.DOColor(target, duration).SetEase(ease);
        }

        private void ApplyAlpha(float target, float duration, Ease ease, bool instant)
        {
            if (fadeTarget == null)
                return;

            fadeTarget.DOKill();

            if (instant)
            {
                fadeTarget.alpha = target;
                return;
            }

            fadeTarget.DOFade(target, duration).SetEase(ease);
        }

        private void OnDestroy()
        {
            if (scaleTarget != null) scaleTarget.DOKill();
            if (tintTarget != null) tintTarget.DOKill();
            if (fadeTarget != null) fadeTarget.DOKill();
        }

#if UNITY_EDITOR
        // 컴포넌트를 처음 붙일 때만 실행됩니다.
        // Unity 기본 Button의 ColorTint가 켜져 있으면 이 컴포넌트의 밝기 연출과 서로 색을 덮어씁니다.
        private void Reset()
        {
            Selectable target = GetComponent<Selectable>();

            if (target != null)
                target.transition = Selectable.Transition.None;
        }
#endif
    }
}
