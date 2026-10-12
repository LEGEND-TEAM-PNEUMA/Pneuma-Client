using DG.Tweening;
using Pneuma.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pneuma.UI.Combat
{
    /// <summary>
    /// 몬스터가 다음 턴에 할 행동(예견)을 머리 위에 표시합니다.
    /// EnemyActionController.OnActionPredicted를 구독하므로, 예견 규칙이 바뀌어도 이 뷰는 바뀌지 않습니다.
    /// </summary>
    public class EnemyIntentView : MonoBehaviour
    {
        [Header("대상")]
        [SerializeField] private Enemy enemy;

        [Tooltip("취약 상태를 예상 피해량에 반영하기 위해 사용합니다. (선택)")]
        [SerializeField] private Player player;

        [Header("표시")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image background;

        [Tooltip("행동 종류 + 수치. 예) 공격 4")]
        [SerializeField] private TMP_Text typeText;

        [Tooltip("행동 이름. 예) 째깍째깍 할퀴기")]
        [SerializeField] private TMP_Text nameText;

        [Header("행동 종류별 색")]
        [SerializeField] private Color attackColor = new Color(0.75f, 0.2f, 0.2f, 0.9f);
        [SerializeField] private Color defenseColor = new Color(0.2f, 0.45f, 0.8f, 0.9f);
        [SerializeField] private Color buffColor = new Color(0.85f, 0.6f, 0.15f, 0.9f);
        [SerializeField] private Color debuffColor = new Color(0.55f, 0.25f, 0.7f, 0.9f);
        [SerializeField] private Color uniqueColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);

        private EnemyActionController subscribedController;
        private Enemy subscribedEnemy;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Start()
        {
            // 첫 예견은 BattleManager.Start에서 일어난다.
            // 이 Start가 그보다 늦게 실행되면 이벤트를 놓치므로 현재 예견 값을 한 번 더 읽는다.
            EnemyActionRuntime predicted = subscribedController != null ? subscribedController.PredictedAction : null;
            Show(predicted != null ? predicted.Data : null);
        }

        private void Subscribe()
        {
            Unsubscribe();

            if (enemy == null) return;

            subscribedEnemy = enemy;
            subscribedEnemy.OnDeath += HandleDeath;
            subscribedEnemy.OnEscaped += HandleEscaped;

            subscribedController = enemy.ActionController;
            if (subscribedController != null)
                subscribedController.OnActionPredicted += Show;
        }

        private void Unsubscribe()
        {
            if (subscribedEnemy != null)
            {
                subscribedEnemy.OnDeath -= HandleDeath;
                subscribedEnemy.OnEscaped -= HandleEscaped;
            }

            if (subscribedController != null)
                subscribedController.OnActionPredicted -= Show;

            subscribedEnemy = null;
            subscribedController = null;
        }

        private void Show(EnemyActionData action)
        {
            if (action == null || enemy == null || enemy.IsDead || enemy.IsEscaped)
            {
                SetVisible(false);
                return;
            }

            if (typeText != null) typeText.text = BuildTypeLabel(action);
            if (nameText != null) nameText.text = action.ActionName;
            if (background != null) background.color = GetColor(action.ActionType);

            SetVisible(true);

            // 새 예견이 떴다는 것을 알 수 있도록 살짝 튕긴다.
            transform.DOKill();
            transform.localScale = Vector3.one * 0.8f;
            transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }

        private string BuildTypeLabel(EnemyActionData action)
        {
            switch (action.ActionType)
            {
                case EnemyActionType.Attack:
                    int damage = GetExpectedDamage(action);
                    return damage > 0 ? $"공격 {damage}" : "공격";

                case EnemyActionType.Defense: return "방어";
                case EnemyActionType.Buff: return "강화";
                case EnemyActionType.Debuff: return "약화";
                default: return "특수";
            }
        }

        private int GetExpectedDamage(EnemyActionData action)
        {
            EnemyActionExecutor executor = subscribedController != null ? subscribedController.ActionExecutor : null;
            if (executor == null) return 0;

            int damage = executor.GetExpectedDamage(action);

            // Player.TakeDamage와 같은 식으로 취약을 반영한다.
            if (player != null && player.IsVulnerable)
                damage = Mathf.CeilToInt(damage * 1.5f);

            return damage;
        }

        private Color GetColor(EnemyActionType type)
        {
            switch (type)
            {
                case EnemyActionType.Attack: return attackColor;
                case EnemyActionType.Defense: return defenseColor;
                case EnemyActionType.Buff: return buffColor;
                case EnemyActionType.Debuff: return debuffColor;
                default: return uniqueColor;
            }
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = visible ? 1f : 0f;
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        private void HandleDeath(CharacterBase unit) => SetVisible(false);

        private void HandleEscaped(Enemy escapedEnemy) => SetVisible(false);

        private void OnDestroy()
        {
            transform.DOKill();
            if (canvasGroup != null) canvasGroup.DOKill();
        }
    }
}
