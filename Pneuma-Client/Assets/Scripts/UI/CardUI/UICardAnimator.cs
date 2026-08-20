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

        // 카드가 다 보이도록 위로 올리는 거리.
        [SerializeField] private float hoverOffsetY = 40f;

        // 확대와 복귀는 같은 속도를 씁니다.
        [SerializeField] private float hoverDuration = 0.1f;
        [SerializeField] private Ease hoverEase = Ease.OutQuad;

        [Header("퇴장")]
        [SerializeField] private float disappearOffsetY = -400f;
        [SerializeField] private float disappearScale = 0.8f;
        [SerializeField] private float disappearDuration = 0.25f;
        [SerializeField] private Ease disappearEase = Ease.InQuad;

        private RectTransform rectTransform;

        // Visual의 제자리. 인스펙터에서 잡아둔 값을 기준으로 삼는다.
        private Vector2 restPosition;
        private Vector3 restScale;

        /// <summary>
        /// 등장 연출이 재생 중인지 여부입니다. 재생 중에는 호버를 받지 않습니다.
        /// </summary>
        public bool IsAppearing { get; private set; }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            restPosition = rectTransform.anchoredPosition;
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

            rectTransform.DOAnchorPos(restPosition, appearDuration)
                .SetEase(appearEase)
                .OnComplete(() => IsAppearing = false);

            rectTransform.DOScale(restScale, appearDuration).SetEase(appearEase);
        }

        /// <summary>
        /// 호버 연출을 재생합니다. 살짝 위로 올라오며 확대됩니다.
        /// </summary>
        public void PlayHover()
        {
            KillTweens();

            rectTransform
                .DOAnchorPos(restPosition + Vector2.up * hoverOffsetY, hoverDuration)
                .SetEase(hoverEase);

            rectTransform.DOScale(restScale * hoverScale, hoverDuration).SetEase(hoverEase);
        }

        /// <summary>
        /// 호버가 풀렸을 때 기본 크기·제자리로 되돌립니다.
        /// </summary>
        public void PlayUnhover()
        {
            KillTweens();

            rectTransform.DOAnchorPos(restPosition, hoverDuration).SetEase(hoverEase);
            rectTransform.DOScale(restScale, hoverDuration).SetEase(hoverEase);
        }

        /// <summary>
        /// 손패에서 빠지는 연출을 재생하고, 끝난 뒤 콜백을 호출합니다.
        /// 호출자는 이 콜백에서 카드 오브젝트를 파괴합니다.
        /// </summary>
        /// <param name="onComplete">연출이 끝난 뒤 실행할 동작</param>
        public void PlayDisappear(Action onComplete = null)
        {
            KillTweens();

            rectTransform.DOScale(restScale * disappearScale, disappearDuration).SetEase(disappearEase);

            // 콜백은 위치 트윈에만 건다. 두 트윈에 모두 걸면 콜백이 두 번 실행된다.
            rectTransform
                .DOAnchorPos(restPosition + Vector2.up * disappearOffsetY, disappearDuration)
                .SetEase(disappearEase)
                .OnComplete(() => onComplete?.Invoke());
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
