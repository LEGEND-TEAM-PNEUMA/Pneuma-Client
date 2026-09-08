using System;
using DG.Tweening;
using UnityEngine;

namespace Pneuma.UI.Common
{
    // 딤드 오버레이의 공통 동작입니다. (UI 기획서 2.2 / 2.4 / 2.1)
    //
    // 기획서 2.2에서 오버레이는 "화면 전환 없이 딤드 처리 후 그 위에 표시"로 정의됩니다.
    // 설정 오버레이, 카드 목록 오버레이, 일시정지 메뉴, 확인창이 전부 이 규칙을 공유하므로
    // 등장·퇴장 연출과 ESC 처리를 여기에 모으고, 각 오버레이는 내용만 채웁니다.
    //
    // 연출 (기획서 2.4)
    //   등장: 딤드 페이드인 0.2초 + 패널 아래→위 슬라이드 0.25초 ease-out
    //   퇴장: 패널 위→아래 슬라이드 0.2초 + 딤드 페이드아웃 0.2초 ease-in (동시 진행)
    public class UIOverlay : MonoBehaviour, IEscapeHandler
    {
        [Header("연결")]
        [Tooltip("뒤 화면을 덮는 딤드 배경. Image의 Raycast Target을 켜야 뒤쪽 클릭이 막힙니다.")]
        [SerializeField] private CanvasGroup dimmed;

        [Tooltip("아래에서 올라오는 내용 패널.")]
        [SerializeField] private RectTransform panel;

        [Header("동작")]
        [Tooltip("패널이 아래에서 올라오는 거리(px).")]
        [SerializeField] private float slideDistance = 80f;

        [Tooltip("끄면 ESC로 닫히지 않습니다. 카드 보상(UI_006)처럼 ESC를 막아야 하는 화면에 사용합니다.")]
        [SerializeField] private bool closeOnEscape = true;

        [Tooltip("씬 시작 시 자동으로 숨깁니다. 편집 중에는 켜둔 채 작업할 수 있습니다.")]
        [SerializeField] private bool hiddenOnAwake = true;

        // 인스펙터에서 잡아둔 패널의 제자리. 슬라이드의 도착점이자 출발점 기준입니다.
        private Vector2 panelHomePosition;

        /// <summary>오버레이가 열려 있는지 여부입니다. 퇴장 연출 중에는 이미 false입니다.</summary>
        public bool IsOpen { get; private set; }

        /// <summary>오버레이가 열릴 때 발생합니다.</summary>
        public event Action Opened;

        /// <summary>오버레이가 닫힐 때 발생합니다. 퇴장 연출 시작 시점입니다.</summary>
        public event Action Closed;

        protected virtual void Awake()
        {
            if (panel != null)
                panelHomePosition = panel.anchoredPosition;

            // IsOpen 확인이 반드시 필요합니다.
            // 오버레이가 씬에서 비활성으로 시작하면 Awake는 첫 Show()의 SetActive(true) 시점에야
            // 처음 실행됩니다. 그때 조건 없이 숨겨 버리면 방금 연 오버레이가 곧바로 다시 꺼집니다.
            // Show()는 SetActive(true)보다 먼저 IsOpen을 올리므로 이 검사로 걸러집니다.
            if (hiddenOnAwake && !IsOpen)
                gameObject.SetActive(false);
        }

        /// <summary>
        /// 오버레이를 엽니다. 이미 열려 있으면 아무 일도 하지 않습니다.
        /// </summary>
        public void Show()
        {
            if (IsOpen)
                return;

            IsOpen = true;
            gameObject.SetActive(true);

            // 열려 있는 동안에만 ESC를 받습니다. 스택 맨 위에 올라가므로
            // 뒤 화면의 ESC 동작보다 이 오버레이가 먼저 처리합니다. (기획서 2.1)
            EscapeRouter.Push(this);

            KillTweens();

            if (dimmed != null)
            {
                dimmed.alpha = 0f;
                dimmed.blocksRaycasts = true;
                dimmed.DOFade(1f, UIMotionSpec.OverlayDimFadeInDuration);
            }

            if (panel != null)
            {
                panel.anchoredPosition = panelHomePosition + Vector2.down * slideDistance;

                panel
                    .DOAnchorPos(panelHomePosition, UIMotionSpec.OverlayPanelSlideInDuration)
                    .SetEase(UIMotionSpec.OverlayPanelSlideInEase);
            }

            OnShown();
            Opened?.Invoke();
        }

        /// <summary>
        /// 오버레이를 닫습니다. 퇴장 연출이 끝난 뒤 오브젝트가 비활성화됩니다.
        /// </summary>
        public void Hide()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            EscapeRouter.Pop(this);

            KillTweens();

            // 딤드는 클릭을 막는 역할도 하므로, 사라지기 시작하는 즉시 입력을 통과시킵니다.
            if (dimmed != null)
            {
                dimmed.blocksRaycasts = false;
                dimmed.DOFade(0f, UIMotionSpec.OverlayDimFadeOutDuration)
                    .SetEase(UIMotionSpec.OverlayPanelSlideOutEase);
            }

            OnHidden();
            Closed?.Invoke();

            if (panel == null)
            {
                // 슬라이드할 패널이 없으면 딤드 페이드가 끝나는 시점에 맞춰 내립니다.
                DOVirtual.DelayedCall(
                    UIMotionSpec.OverlayDimFadeOutDuration,
                    DeactivateIfStillClosed,
                    ignoreTimeScale: false);

                return;
            }

            panel
                .DOAnchorPos(
                    panelHomePosition + Vector2.down * slideDistance,
                    UIMotionSpec.OverlayPanelSlideOutDuration)
                .SetEase(UIMotionSpec.OverlayPanelSlideOutEase)
                .OnComplete(DeactivateIfStillClosed);
        }

        /// <summary>
        /// 열려 있으면 닫고, 닫혀 있으면 엽니다.
        /// </summary>
        public void Toggle()
        {
            if (IsOpen)
                Hide();
            else
                Show();
        }

        /// <summary>
        /// ESC 입력을 처리합니다. (기획서 2.1 — 오버레이는 ESC 시 오버레이만 닫히고 뒤 화면은 유지)
        /// </summary>
        public bool HandleEscape()
        {
            if (!IsOpen)
                return false;

            if (closeOnEscape)
                Hide();

            // closeOnEscape가 꺼져 있어도 true를 돌려줍니다.
            // 오버레이는 모달이므로 ESC가 뒤 화면으로 새어 나가면 안 됩니다.
            return true;
        }

        /// <summary>오버레이가 열린 직후 호출됩니다. 내용 갱신은 여기서 합니다.</summary>
        protected virtual void OnShown()
        {
        }

        /// <summary>오버레이가 닫히기 시작할 때 호출됩니다.</summary>
        protected virtual void OnHidden()
        {
        }

        // 퇴장 연출 도중 다시 Show()가 불리면 IsOpen이 true로 돌아옵니다.
        // 그때는 오브젝트를 내리면 안 되므로 완료 콜백에서 한 번 더 확인합니다.
        private void DeactivateIfStillClosed()
        {
            if (IsOpen)
                return;

            gameObject.SetActive(false);
        }

        private void KillTweens()
        {
            if (dimmed != null) dimmed.DOKill();
            if (panel != null) panel.DOKill();
        }

        protected virtual void OnDisable()
        {
            // 씬 전환이나 수동 SetActive(false)로 꺼질 때 스택에 남지 않도록 정리합니다.
            EscapeRouter.Pop(this);
            IsOpen = false;
        }

        protected virtual void OnDestroy()
        {
            KillTweens();
            EscapeRouter.Pop(this);
        }
    }
}
