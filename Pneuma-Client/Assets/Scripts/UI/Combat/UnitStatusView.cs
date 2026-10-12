using DG.Tweening;
using Pneuma.Unit;
using TMPro;
using UnityEngine;

namespace Pneuma.UI.Combat
{
    /// <summary>
    /// 캐릭터·몬스터 한 명의 이름, 체력 바, 방어도, 떠오르는 숫자를 표시합니다.
    /// CharacterBase의 이벤트만 구독하므로 Player와 Enemy 모두에 그대로 쓸 수 있습니다.
    ///
    /// 체력 바는 Image Fill 대신 RectTransform의 앵커(anchorMax.x)로 비율을 표현합니다.
    /// 스프라이트가 없어도 동작하고, 앵커 값을 트윈하면 그대로 줄어드는 연출이 됩니다.
    /// </summary>
    public class UnitStatusView : MonoBehaviour
    {
        [Header("대상")]
        [SerializeField] private CharacterBase target;

        [Header("표시")]
        [SerializeField] private TMP_Text nameText;

        [Tooltip("현재 HP 바. 앵커 Min (0,0) / Max (1,1)로 부모를 꽉 채운 상태에서 시작합니다.")]
        [SerializeField] private RectTransform hpFill;

        [Tooltip("피격 시 늦게 줄어드는 잔상 바 (선택)")]
        [SerializeField] private RectTransform hpDelayFill;

        [SerializeField] private TMP_Text hpText;

        [Tooltip("방어도가 0이면 숨길 오브젝트")]
        [SerializeField] private GameObject shieldRoot;
        [SerializeField] private TMP_Text shieldText;

        [Tooltip("사망·도주 시 흐리게 만들 때 사용")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("바 연출")]
        [SerializeField] private float fillDuration = 0.2f;
        [SerializeField] private float delayFillWait = 0.35f;
        [SerializeField] private float delayFillDuration = 0.4f;
        [SerializeField, Range(0f, 1f)] private float deadAlpha = 0.35f;

        [Header("떠오르는 숫자")]
        [Tooltip("비활성화된 템플릿. 변화가 생길 때마다 복제해서 띄웁니다.")]
        [SerializeField] private TMP_Text popupTemplate;
        [SerializeField] private Color damageColor = new Color(1f, 0.35f, 0.3f);
        [SerializeField] private Color healColor = new Color(0.45f, 1f, 0.5f);
        [SerializeField] private Color shieldColor = new Color(0.45f, 0.75f, 1f);
        [SerializeField] private float popupRise = 90f;
        [SerializeField] private float popupDuration = 0.8f;

        private CharacterBase subscribedTarget;
        private int lastHp = -1;
        private int lastShield;
        private int pendingBlocked;

        private void OnEnable()
        {
            Subscribe(target);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        // 모든 Awake가 끝나 HP가 초기화된 뒤 현재 값을 연출 없이 그린다.
        private void Start()
        {
            RefreshImmediate();
        }

        /// <summary>
        /// 표시할 대상을 바꿉니다. (런타임 소환 등 코드로 연결할 때)
        /// </summary>
        public void Bind(CharacterBase newTarget)
        {
            target = newTarget;

            if (isActiveAndEnabled)
            {
                Subscribe(target);
                RefreshImmediate();
            }
        }

        private void Subscribe(CharacterBase unit)
        {
            Unsubscribe();

            if (unit == null) return;

            subscribedTarget = unit;
            subscribedTarget.OnHpChanged += HandleHpChanged;
            subscribedTarget.OnShieldChanged += HandleShieldChanged;
            subscribedTarget.OnDeath += HandleDeath;
            subscribedTarget.OnDamaged += HandleDamaged;

            if (subscribedTarget is Enemy enemy)
                enemy.OnEscaped += HandleEscaped;
        }

        private void Unsubscribe()
        {
            if (subscribedTarget == null) return;

            subscribedTarget.OnHpChanged -= HandleHpChanged;
            subscribedTarget.OnShieldChanged -= HandleShieldChanged;
            subscribedTarget.OnDeath -= HandleDeath;
            subscribedTarget.OnDamaged -= HandleDamaged;

            if (subscribedTarget is Enemy enemy)
                enemy.OnEscaped -= HandleEscaped;

            subscribedTarget = null;
        }

        private void RefreshImmediate()
        {
            if (target == null) return;

            if (nameText != null) nameText.text = GetDisplayName(target);

            lastHp = target.CurrentHp;
            lastShield = target.CurrentShield;

            float ratio = GetRatio(target.CurrentHp, target.MaxHp);
            SetFillImmediate(hpFill, ratio);
            SetFillImmediate(hpDelayFill, ratio);
            SetHpText(target.CurrentHp, target.MaxHp);
            SetShield(target.CurrentShield);

            if (canvasGroup != null) canvasGroup.alpha = target.IsDead ? deadAlpha : 1f;
        }

        private void HandleHpChanged(int current, int max)
        {
            float ratio = GetRatio(current, max);
            bool isDamage = lastHp >= 0 && current < lastHp;

            TweenFill(hpFill, ratio, fillDuration, 0f);

            // 잔상 바는 피격일 때만 잠깐 머물렀다가 따라 내려온다. 회복이면 즉시 맞춘다.
            TweenFill(hpDelayFill, ratio, delayFillDuration, isDamage ? delayFillWait : 0f);

            SetHpText(current, max);

            if (lastHp >= 0 && current != lastHp)
            {
                int delta = current - lastHp;
                SpawnPopup(delta > 0 ? $"+{delta}" : delta.ToString(), delta > 0 ? healColor : damageColor);
            }

            lastHp = current;
        }

        private void HandleShieldChanged(int shield)
        {
            int delta = shield - lastShield;
            lastShield = shield;

            // 이전에 남아 있던 값은 버린다. (턴 시작 ClearShield처럼 피격 없이 줄어든 경우)
            pendingBlocked = 0;

            SetShield(shield);

            if (delta > 0)
            {
                SpawnPopup($"+{delta} 방어", shieldColor);
            }
            else if (delta < 0)
            {
                // 방어도가 줄어든 이유가 피격인지 턴 시작 초기화인지는 아직 모른다.
                // CharacterBase.TakeDamage는 방어도 변경 → (HP 변경) → OnDamaged 순서로 알리므로,
                // 일단 기억해 두었다가 OnDamaged가 오면 그때 "막음"을 띄운다.
                pendingBlocked = -delta;
            }
        }

        private void HandleDamaged(CharacterBase unit)
        {
            if (pendingBlocked <= 0) return;

            SpawnPopup($"막음 {pendingBlocked}", shieldColor);
            pendingBlocked = 0;
        }

        private void HandleDeath(CharacterBase unit)
        {
            if (canvasGroup == null) return;

            canvasGroup.DOKill();
            canvasGroup.DOFade(deadAlpha, 0.4f);
        }

        private void HandleEscaped(Enemy enemy)
        {
            if (canvasGroup == null) return;

            canvasGroup.DOKill();
            canvasGroup.DOFade(0f, 0.3f);
        }

        private void SetHpText(int current, int max)
        {
            if (hpText != null) hpText.text = $"{current} / {max}";
        }

        private void SetShield(int shield)
        {
            if (shieldRoot != null) shieldRoot.SetActive(shield > 0);
            if (shieldText != null) shieldText.text = shield.ToString();
        }

        private void SpawnPopup(string message, Color color)
        {
            if (popupTemplate == null) return;

            TMP_Text popup = Instantiate(popupTemplate, popupTemplate.transform.parent, false);
            popup.gameObject.SetActive(true);
            popup.text = message;
            popup.color = color;
            popup.alpha = 1f;

            RectTransform rect = popup.rectTransform;
            rect.anchoredPosition = popupTemplate.rectTransform.anchoredPosition;

            // 연속으로 맞아도 숫자가 겹치지 않도록 좌우로 살짝 흩어 놓는다.
            rect.anchoredPosition += new Vector2(Random.Range(-25f, 25f), 0f);

            Sequence sequence = DOTween.Sequence();
            sequence.Join(rect.DOAnchorPosY(rect.anchoredPosition.y + popupRise, popupDuration).SetEase(Ease.OutCubic));
            sequence.Insert(popupDuration * 0.5f,
                DOTween.To(() => popup.alpha, value => popup.alpha = value, 0f, popupDuration * 0.5f));
            sequence.SetTarget(popup);
            sequence.OnComplete(() =>
            {
                if (popup != null) Destroy(popup.gameObject);
            });
        }

        private static void TweenFill(RectTransform fill, float ratio, float duration, float delay)
        {
            if (fill == null) return;

            fill.DOKill();
            fill.DOAnchorMax(new Vector2(ratio, fill.anchorMax.y), duration)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad);
        }

        private static void SetFillImmediate(RectTransform fill, float ratio)
        {
            if (fill == null) return;

            fill.DOKill();
            fill.anchorMax = new Vector2(ratio, fill.anchorMax.y);
        }

        private static float GetRatio(int current, int max)
        {
            return max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
        }

        private static string GetDisplayName(CharacterBase unit)
        {
            string displayName = null;

            if (unit is Player player) displayName = player.CharacterName;
            else if (unit is Enemy enemy) displayName = enemy.EnemyName;

            return string.IsNullOrEmpty(displayName) ? unit.name : displayName;
        }

        private void OnDestroy()
        {
            if (hpFill != null) hpFill.DOKill();
            if (hpDelayFill != null) hpDelayFill.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();
        }
    }
}
