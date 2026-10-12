using System;
using DG.Tweening;
using UnityEngine;

namespace Pneuma.UI.Card
{
    // 카드 등장, 사용, 드로우 애니메이션을 관리합니다.
    // 다른 데이터는 저장하지 않고, 오로지 애니메이션을 위한 컴포넌트입니다.
    //
    // 이 컴포넌트는 카드 루트가 아닌 자식 Visual에 붙어 자기 자신만 움직입니다.
    // 루트의 위치·회전은 UICardDrag가 매 프레임 소유하므로, 루트를 트윈하면 서로 값을 덮어써
    // 연출이 사라집니다. 손패 안에서의 이동은 정렬·드래그가, 카드 한 장의 연출은 여기가 맡습니다.
    [RequireComponent(typeof(RectTransform))]
    public class UICardAnimator : MonoBehaviour
    {
        [Header("등장")]
        // 아래에서 올라오는 거리. 화면 크기가 아니라 카드 기준 로컬 거리다.
        [SerializeField] private float appearOffsetY = -400f;
        [SerializeField] private float appearScale = 0.8f;
        [SerializeField] private float appearDuration = 0.35f;
        [SerializeField] private Ease appearEase = Ease.OutQuad;

        [Header("호버")]
        // 기획서 3.3.2 [1] 기준: 기본 카드 대비 약 120%, 0.1초 이내 확대.
        [SerializeField] private float hoverScale = 1.2f;

        [Tooltip("켜면 호버 시 카드 아래 끝을 화면 아래 끝에 맞춰 설명까지 전부 보이게 올립니다.")]
        [SerializeField] private bool alignBottomToScreen = true;

        [Tooltip("화면 아래 끝과 카드 아래 끝 사이 여백(캔버스 단위)")]
        [SerializeField] private float screenBottomMargin = 10f;

        [Tooltip("켜면 호버 중 부채꼴 기울기를 풀어 카드를 똑바로 세웁니다.")]
        [SerializeField] private bool straightenOnHover = true;

        // alignBottomToScreen을 끄거나 캔버스를 찾지 못했을 때만 쓰는 고정 상승 거리.
        [SerializeField] private float hoverOffsetY = 40f;

        // 확대와 복귀는 같은 속도를 씁니다.
        [SerializeField] private float hoverDuration = 0.1f;
        [SerializeField] private Ease hoverEase = Ease.OutQuad;

        [Header("퇴장")]
        [SerializeField] private float disappearOffsetY = -400f;
        [SerializeField] private float disappearScale = 0.8f;
        [SerializeField] private float disappearDuration = 0.25f;
        [SerializeField] private Ease disappearEase = Ease.InQuad;

        [Header("사용")]
        // 놓은 자리에서 살짝 커졌다가 작아지며 사라집니다.
        [SerializeField] private float usePunchScale = 1.15f;
        [SerializeField] private float useDuration = 0.3f;

        private RectTransform rectTransform;

        // Visual의 제자리. 인스펙터에서 잡아둔 값을 기준으로 삼는다.
        private Vector2 restPosition;
        private Vector3 restLocalPosition;
        private Quaternion restRotation;
        private Vector3 restScale;

        // 화면 아래 끝을 구할 기준. 처음 호버할 때 찾아 둔다.
        private RectTransform canvasRect;

        /// <summary>
        /// 등장 연출이 재생 중인지 여부입니다. 재생 중에는 호버를 받지 않습니다.
        /// </summary>
        public bool IsAppearing { get; private set; }

        /// <summary>
        /// 사용·퇴장 연출이 시작되었는지 여부입니다.
        /// 한 번 켜지면 호버·호버 해제 연출을 받지 않습니다.
        /// 드래그를 놓는 순간 UICardInteraction.OnEndDrag가 호버 해제(PlayUnhover)를 요청하는데,
        /// 이를 받으면 KillTweens가 사용 연출을 끊고 크기를 되돌려 카드가 화면에 남기 때문입니다.
        /// </summary>
        public bool IsLeaving { get; private set; }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            restPosition = rectTransform.anchoredPosition;
            restLocalPosition = rectTransform.localPosition;
            restRotation = rectTransform.localRotation;
            restScale = rectTransform.localScale;
        }

        /// <summary>
        /// 손패에 등장하는 연출을 재생합니다. 아래에서 제자리로 올라옵니다.
        /// </summary>
        public void PlayAppear()
        {
            KillTweens();
            IsAppearing = true;

            // 시작점을 직접 잡아둔다. 이전 연출이 남긴 위치에서 이어지면 이동 거리가 들쭉날쭉해진다.
            rectTransform.anchoredPosition = restPosition + Vector2.up * appearOffsetY;
            rectTransform.localScale = restScale * appearScale;
            rectTransform.localRotation = restRotation;

            rectTransform.DOAnchorPos(restPosition, appearDuration)
                .SetEase(appearEase)
                .OnComplete(() => IsAppearing = false);

            rectTransform.DOScale(restScale, appearDuration).SetEase(appearEase);
        }

        /// <summary>
        /// 호버 연출을 재생합니다.
        /// 확대한 카드의 아래 끝이 화면 아래 끝에 닿도록 올려, 화면 밖에 걸쳐 있던 설명까지 보이게 합니다.
        /// </summary>
        public void PlayHover()
        {
            if (IsLeaving) return;

            KillTweens();

            Vector3 targetScale = restScale * hoverScale;

            rectTransform
                .DOLocalMove(CalculateHoverLocalPosition(targetScale), hoverDuration)
                .SetEase(hoverEase);

            rectTransform.DOScale(targetScale, hoverDuration).SetEase(hoverEase);

            rectTransform
                .DOLocalRotateQuaternion(CalculateHoverLocalRotation(), hoverDuration)
                .SetEase(hoverEase);
        }

        /// <summary>
        /// 드래그를 시작할 때 호출합니다. 확대는 유지하고 위치·기울기만 제자리로 돌려
        /// 카드가 커서 밑에 오도록 합니다. (올라간 채로 끌면 커서와 카드가 멀어집니다)
        /// </summary>
        public void PlayDragHold()
        {
            if (IsLeaving) return;

            KillTweens();

            rectTransform.DOAnchorPos(restPosition, hoverDuration).SetEase(hoverEase);
            rectTransform.DOLocalRotateQuaternion(restRotation, hoverDuration).SetEase(hoverEase);
            rectTransform.DOScale(restScale * hoverScale, hoverDuration).SetEase(hoverEase);
        }

        // 확대 후 카드 아래 끝이 화면(루트 캔버스) 아래 끝 + 여백에 오도록 하는 Visual의 로컬 위치를 구한다.
        // 계산은 캔버스 좌표에서 하고, 마지막에 부모(카드 루트) 로컬 좌표로 되돌린다.
        // 이렇게 하면 루트가 부채꼴로 기울어 있거나 손패 위치가 바뀌어도 같은 식으로 맞출 수 있다.
        private Vector3 CalculateHoverLocalPosition(Vector3 targetScale)
        {
            Vector3 fallback = restLocalPosition + Vector3.up * hoverOffsetY;

            RectTransform parent = rectTransform.parent as RectTransform;
            RectTransform canvas = GetCanvasRect();

            if (!alignBottomToScreen || parent == null || canvas == null)
                return fallback;

            // 1) 제자리에 있을 때 Visual 기준점(pivot)의 캔버스 좌표
            Vector3 pivotInCanvas = canvas.InverseTransformPoint(parent.TransformPoint(restLocalPosition));

            // 2) 확대된 카드 크기를 캔버스 단위로 환산 (부모들의 스케일 포함)
            float toCanvas = parent.lossyScale.y / canvas.lossyScale.y;
            float height = rectTransform.rect.height * targetScale.y * toCanvas;
            float width = rectTransform.rect.width * targetScale.x * toCanvas;
            Vector2 pivot = rectTransform.pivot;
            Rect canvasArea = canvas.rect;

            // 3) 아래 끝 = 화면 아래 + 여백 → 기준점 y = 아래 끝 + (pivot.y × 높이)
            float alignedY = canvasArea.yMin + screenBottomMargin + pivot.y * height;

            // 이미 화면 안에 다 보이는 카드라면 아래로 끌어내리지 않는다.
            pivotInCanvas.y = Mathf.Max(pivotInCanvas.y, alignedY);

            // 4) 확대로 좌우 끝 카드가 화면 밖으로 나가지 않게 가로를 화면 안으로 제한한다.
            float minX = canvasArea.xMin + pivot.x * width;
            float maxX = canvasArea.xMax - (1f - pivot.x) * width;
            if (minX <= maxX)
                pivotInCanvas.x = Mathf.Clamp(pivotInCanvas.x, minX, maxX);

            return parent.InverseTransformPoint(canvas.TransformPoint(pivotInCanvas));
        }

        // 부모(루트)의 부채꼴 기울기를 상쇄해 캔버스 기준으로 똑바로 서는 로컬 회전을 구한다.
        private Quaternion CalculateHoverLocalRotation()
        {
            Transform parent = rectTransform.parent;
            RectTransform canvas = GetCanvasRect();

            if (!straightenOnHover || parent == null || canvas == null)
                return restRotation;

            return Quaternion.Inverse(parent.rotation) * canvas.rotation;
        }

        private RectTransform GetCanvasRect()
        {
            if (canvasRect != null) return canvasRect;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null) canvasRect = canvas.rootCanvas.transform as RectTransform;

            return canvasRect;
        }

        /// <summary>
        /// 호버가 풀렸을 때 기본 크기·제자리로 되돌립니다.
        /// </summary>
        public void PlayUnhover()
        {
            if (IsLeaving) return;

            KillTweens();

            rectTransform.DOAnchorPos(restPosition, hoverDuration).SetEase(hoverEase);
            rectTransform.DOScale(restScale, hoverDuration).SetEase(hoverEase);
            rectTransform.DOLocalRotateQuaternion(restRotation, hoverDuration).SetEase(hoverEase);
        }

        /// <summary>
        /// 손패에서 빠지는 연출을 재생하고, 끝난 뒤 콜백을 호출합니다.
        /// 호출자는 이 콜백에서 카드 오브젝트를 파괴합니다.
        /// </summary>
        /// <param name="onComplete">연출이 끝난 뒤 실행할 동작</param>
        public void PlayDisappear(Action onComplete = null)
        {
            KillTweens();
            IsLeaving = true;

            rectTransform.DOScale(restScale * disappearScale, disappearDuration).SetEase(disappearEase);
            rectTransform.DOLocalRotateQuaternion(restRotation, disappearDuration).SetEase(disappearEase);

            // 콜백은 위치 트윈에만 건다. 두 트윈에 모두 걸면 콜백이 두 번 실행된다.
            rectTransform
                .DOAnchorPos(restPosition + Vector2.up * disappearOffsetY, disappearDuration)
                .SetEase(disappearEase)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 카드가 사용되었을 때의 연출을 재생하고, 끝난 뒤 콜백을 호출합니다.
        /// </summary>
        /// <param name="onComplete">연출이 끝난 뒤 실행할 동작</param>
        public void PlayUse(Action onComplete = null)
        {
            KillTweens();
            IsLeaving = true;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(rectTransform.DOScale(restScale * usePunchScale, useDuration * 0.35f).SetEase(Ease.OutQuad));
            sequence.Append(rectTransform.DOScale(Vector3.zero, useDuration * 0.65f).SetEase(Ease.InBack));

            // 타깃을 지정해야 KillTweens(DOKill)·OnDestroy에서 시퀀스도 함께 정리된다.
            sequence.SetTarget(rectTransform);
            sequence.OnComplete(() => onComplete?.Invoke());
        }

        // 이전 연출이 남아 있으면 새 연출과 같은 값을 두고 다투므로 먼저 끊는다.
        // 등장 트윈이 끊기면 OnComplete가 오지 않으므로 여기서 상태도 같이 내린다.
        private void KillTweens()
        {
            rectTransform.DOKill();
            IsAppearing = false;
        }

        // 파괴된 RectTransform을 남은 트윈이 건드리지 않도록 정리한다.
        private void OnDestroy() => rectTransform.DOKill();
    }

}
