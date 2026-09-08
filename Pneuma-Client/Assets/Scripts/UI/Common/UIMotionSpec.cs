using DG.Tweening;
using UnityEngine;

namespace Pneuma.UI.Common
{
    // UI 기획서 2.3(공통 버튼/요소 상태) / 2.4(공통 트랜지션)의 수치를 한곳에 모읍니다.
    //
    // 화면마다 인스펙터에 값을 따로 적으면 기획이 바뀔 때 반드시 누락이 생깁니다.
    // 공통 규칙에 해당하는 연출은 인스펙터로 빼지 말고 여기 상수를 참조하세요.
    // (카드 호버처럼 화면 고유 연출은 기획서 3장 소관이므로 각 컴포넌트가 자기 값을 가집니다.)
    public static class UIMotionSpec
    {
        // ───── 2.3 공통 버튼/요소 상태 ─────

        /// <summary>호버 시 확대 배율. (기획서 2.3 — 106%)</summary>
        public const float ButtonHoverScale = 1.06f;

        /// <summary>호버 시 밝기 배율. (기획서 2.3 — +10%)</summary>
        public const float ButtonHoverBrightness = 1.1f;

        /// <summary>호버 전환 시간. (기획서 2.3 — 0.15초 ease-out)</summary>
        public const float ButtonHoverDuration = 0.15f;

        public const Ease ButtonHoverEase = Ease.OutQuad;

        /// <summary>클릭 시 축소 배율. (기획서 2.3 — 96%)</summary>
        public const float ButtonPressedScale = 0.96f;

        /// <summary>클릭 시 밝기 배율. (기획서 2.3 — -10%)</summary>
        public const float ButtonPressedBrightness = 0.9f;

        /// <summary>클릭 전환 시간. (기획서 2.3 — 0.08초 ease-in)</summary>
        public const float ButtonPressedDuration = 0.08f;

        public const Ease ButtonPressedEase = Ease.InQuad;

        /// <summary>비활성 시 채도 감소량. (기획서 2.3 — 채도 -60%)</summary>
        public const float ButtonDisabledDesaturation = 0.6f;

        /// <summary>비활성 시 불투명도. (기획서 2.3 — 50%)</summary>
        public const float ButtonDisabledAlpha = 0.5f;

        // ───── 2.4 공통 트랜지션/애니메이션 ─────

        /// <summary>화면 전환 크로스 페이드 시간. (기획서 2.4 — 0.3초 ease-in-out)</summary>
        public const float ScreenFadeDuration = 0.3f;

        public const Ease ScreenFadeEase = Ease.InOutQuad;

        /// <summary>전투 진입·카드 보상 전환의 편도 페이드 시간. (기획서 2.4 — 0.25초)</summary>
        public const float LoadFadeDuration = 0.25f;

        /// <summary>오버레이 등장 시 딤드 배경 페이드인 시간. (기획서 2.4 — 0.2초)</summary>
        public const float OverlayDimFadeInDuration = 0.2f;

        /// <summary>오버레이 등장 시 패널 슬라이드 시간. (기획서 2.4 — 0.25초 ease-out)</summary>
        public const float OverlayPanelSlideInDuration = 0.25f;

        public const Ease OverlayPanelSlideInEase = Ease.OutQuad;

        /// <summary>오버레이 퇴장 시 패널 슬라이드 시간. (기획서 2.4 — 0.2초 ease-in)</summary>
        public const float OverlayPanelSlideOutDuration = 0.2f;

        /// <summary>오버레이 퇴장 시 딤드 배경 페이드아웃 시간. (기획서 2.4 — 0.2초)</summary>
        public const float OverlayDimFadeOutDuration = 0.2f;

        public const Ease OverlayPanelSlideOutEase = Ease.InQuad;

        /// <summary>딤드 배경의 최종 불투명도.</summary>
        // TODO: 기획서에 딤드 농도 수치가 없어 임시값입니다. 아트팀 스타일 가이드 확정 시 갱신.
        public const float OverlayDimAlpha = 0.6f;

        // ───── 색 계산 헬퍼 ─────

        /// <summary>
        /// 색의 알파는 유지한 채 RGB에만 밝기 배율을 적용합니다. (기획서 2.3 밝기 ±10%)
        /// </summary>
        public static Color ApplyBrightness(Color color, float multiplier)
        {
            return new Color(
                Mathf.Clamp01(color.r * multiplier),
                Mathf.Clamp01(color.g * multiplier),
                Mathf.Clamp01(color.b * multiplier),
                color.a);
        }

        /// <summary>
        /// 색의 채도를 낮춥니다. (기획서 2.3 비활성 — 채도 -60%)
        /// </summary>
        /// <param name="amount">0이면 원본, 1이면 완전 무채색</param>
        public static Color ApplyDesaturation(Color color, float amount)
        {
            // 사람 눈의 색상별 밝기 감도를 반영한 표준 휘도 가중치입니다.
            // 단순 평균을 쓰면 초록이 어둡게, 파랑이 밝게 보여 회색으로 자연스럽게 빠지지 않습니다.
            float luminance = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;
            float t = Mathf.Clamp01(amount);

            return new Color(
                Mathf.Lerp(color.r, luminance, t),
                Mathf.Lerp(color.g, luminance, t),
                Mathf.Lerp(color.b, luminance, t),
                color.a);
        }
    }
}
