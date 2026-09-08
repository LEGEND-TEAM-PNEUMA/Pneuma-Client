using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pneuma.UI.Common
{
    // 공통 확인창 / 경고창입니다. (UI 기획서 2.2)
    //
    //   확인창(Confirm) — 예/아니오 선택형   예) '정말 종료하시겠습니까?'
    //   경고창(Warning) — 확인 버튼만 존재
    //
    // 인트로 영상의 SKIP 확인처럼 '다음 접속 시 자동 스킵' 체크박스가 붙는 경우가 있어
    // (기획서 3.1-다) 선택적으로 체크박스를 노출할 수 있게 했습니다.
    //
    // ESC는 취소(아니오)와 동일하게 처리합니다. (기획서 2.1 — 종료 확인창: ESC → 취소 처리)
    public class UIConfirmPopup : UIOverlay
    {
        [Header("확인창 연결")]
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [Tooltip("경고창(확인만)일 때 통째로 숨길 취소 버튼 루트. 비우면 취소 버튼 자체를 숨깁니다.")]
        [SerializeField] private GameObject cancelRoot;

        [Header("선택 체크박스")]
        [Tooltip("'다음 접속 시 자동 스킵' 같은 부가 선택지. 필요 없는 호출에서는 숨겨집니다.")]
        [SerializeField] private GameObject optionRoot;
        [SerializeField] private Toggle optionToggle;
        [SerializeField] private TMP_Text optionLabel;

        // 결과를 아직 호출자에게 넘기지 않았는지. ESC·바깥 클릭으로 닫힐 때 취소로 처리하는 데 씁니다.
        private bool resultDelivered = true;

        private Action<bool, bool> pendingCallback;

        protected override void Awake()
        {
            base.Awake();

            if (confirmButton != null)
                confirmButton.onClick.AddListener(HandleConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(HandleCancelClicked);
        }

        /// <summary>
        /// 예/아니오 확인창을 엽니다.
        /// </summary>
        /// <param name="message">본문 문구</param>
        /// <param name="onResult">예를 누르면 true, 아니오·ESC면 false</param>
        public void Open(string message, Action<bool> onResult)
        {
            Open(message, null, (confirmed, _) => onResult?.Invoke(confirmed));
        }

        /// <summary>
        /// 체크박스가 달린 확인창을 엽니다. (기획서 3.1-다 인트로 SKIP 확인 팝업)
        /// </summary>
        /// <param name="message">본문 문구</param>
        /// <param name="checkboxLabel">체크박스 라벨. null이면 체크박스를 숨깁니다.</param>
        /// <param name="onResult">(예를 눌렀는지, 체크박스가 켜져 있는지)</param>
        public void Open(string message, string checkboxLabel, Action<bool, bool> onResult)
        {
            pendingCallback = onResult;
            resultDelivered = false;

            if (messageText != null)
                messageText.text = message;

            SetCancelVisible(true);
            SetCheckbox(checkboxLabel);

            Show();
        }

        /// <summary>
        /// 확인 버튼만 있는 경고창을 엽니다.
        /// </summary>
        /// <param name="message">본문 문구</param>
        /// <param name="onConfirm">확인을 누르거나 ESC로 닫을 때 호출됩니다.</param>
        public void OpenWarning(string message, Action onConfirm = null)
        {
            pendingCallback = (_, __) => onConfirm?.Invoke();
            resultDelivered = false;

            if (messageText != null)
                messageText.text = message;

            SetCancelVisible(false);
            SetCheckbox(null);

            Show();
        }

        private void SetCancelVisible(bool visible)
        {
            if (cancelRoot != null)
            {
                cancelRoot.SetActive(visible);
                return;
            }

            if (cancelButton != null)
                cancelButton.gameObject.SetActive(visible);
        }

        private void SetCheckbox(string label)
        {
            bool useCheckbox = !string.IsNullOrEmpty(label);

            if (optionRoot != null)
                optionRoot.SetActive(useCheckbox);
            else if (optionToggle != null)
                optionToggle.gameObject.SetActive(useCheckbox);

            if (!useCheckbox)
                return;

            if (optionLabel != null)
                optionLabel.text = label;

            // 이전 호출에서 켜둔 값이 남지 않도록 항상 꺼진 상태로 시작합니다.
            if (optionToggle != null)
                optionToggle.isOn = false;
        }

        private void HandleConfirmClicked()
        {
            DeliverResult(true);
            Hide();
        }

        private void HandleCancelClicked()
        {
            DeliverResult(false);
            Hide();
        }

        // ESC나 외부의 Hide() 호출로 닫히는 경우에도 호출자가 결과를 반드시 받도록,
        // 아직 결과를 넘기지 않았다면 취소로 처리합니다. (기획서 2.1 — ESC는 아니오와 동일)
        protected override void OnHidden()
        {
            DeliverResult(false);
        }

        private void DeliverResult(bool confirmed)
        {
            if (resultDelivered)
                return;

            resultDelivered = true;

            Action<bool, bool> callback = pendingCallback;
            pendingCallback = null;

            bool optionChecked = optionToggle != null && optionToggle.isOn;

            callback?.Invoke(confirmed, optionChecked);
        }

        protected override void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(HandleConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(HandleCancelClicked);

            base.OnDestroy();
        }
    }
}
