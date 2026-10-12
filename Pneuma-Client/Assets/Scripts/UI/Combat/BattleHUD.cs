using System.Collections;
using Battle;
using DG.Tweening;
using Pneuma.UI.Card;
using Pneuma.Unit;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pneuma.UI.Combat
{
    /// <summary>
    /// 전투 화면 공통 HUD입니다. 턴 정보, 에너지, 카드 더미 수, 턴 종료 버튼, 턴 배너, 안내 문구, 결과 창을 담당합니다.
    ///
    /// 모든 표시는 이벤트로만 갱신하고 Update에서 값을 매 프레임 읽지 않습니다.
    ///  - BattleManager.OnBattleStateChanged : 턴·배너·결과
    ///  - Player.OnEnergyChanged             : 에너지
    ///  - CardManager.OnCardMoved            : 카드 더미 수
    ///  - UIBattleCardController 이벤트       : 턴 종료 버튼 잠금, 거절 안내
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private UIBattleCardController cardController;
        [SerializeField] private CardManager cardManager;

        [Header("턴")]
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private Button endTurnButton;

        [Header("에너지")]
        [SerializeField] private TMP_Text energyText;

        [Header("카드 더미")]
        [SerializeField] private TMP_Text drawPileText;
        [SerializeField] private TMP_Text discardPileText;
        [SerializeField] private TMP_Text exhaustPileText;

        [Header("턴 배너")]
        [SerializeField] private CanvasGroup turnBanner;
        [SerializeField] private TMP_Text turnBannerText;
        [SerializeField] private float bannerFadeTime = 0.2f;
        [SerializeField] private float bannerHoldTime = 0.8f;

        [Header("안내 문구")]
        [SerializeField] private CanvasGroup noticeGroup;
        [SerializeField] private TMP_Text noticeText;
        [SerializeField] private float noticeHoldTime = 1.2f;

        [Header("결과")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultDetailText;
        [SerializeField] private Button restartButton;

        [Tooltip("사망·승리 모션을 볼 수 있도록 결과 창을 늦게 띄웁니다.")]
        [SerializeField] private float resultDelay = 1.2f;

        private Player subscribedPlayer;
        private bool isResolvingCard;

        private void Awake()
        {
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();

            if (endTurnButton != null) endTurnButton.onClick.AddListener(HandleEndTurnClicked);
            if (restartButton != null) restartButton.onClick.AddListener(HandleRestartClicked);

            if (resultPanel != null) resultPanel.SetActive(false);
            HideImmediate(turnBanner);
            HideImmediate(noticeGroup);
        }

        private void OnEnable()
        {
            if (battleManager != null)
                battleManager.OnBattleStateChanged += HandleBattleStateChanged;

            if (cardController != null)
            {
                cardController.OnCardRejected += ShowNotice;
                cardController.OnResolvingChanged += HandleResolvingChanged;
            }

            if (cardManager != null)
                cardManager.OnCardMoved += HandleCardMoved;

            subscribedPlayer = battleManager != null ? battleManager.CurrentPlayer : null;
            if (subscribedPlayer != null)
                subscribedPlayer.OnEnergyChanged += HandleEnergyChanged;
        }

        private void OnDisable()
        {
            if (battleManager != null)
                battleManager.OnBattleStateChanged -= HandleBattleStateChanged;

            if (cardController != null)
            {
                cardController.OnCardRejected -= ShowNotice;
                cardController.OnResolvingChanged -= HandleResolvingChanged;
            }

            if (cardManager != null)
                cardManager.OnCardMoved -= HandleCardMoved;

            if (subscribedPlayer != null)
                subscribedPlayer.OnEnergyChanged -= HandleEnergyChanged;

            subscribedPlayer = null;
        }

        // BattleManager.Start가 먼저 돌았든 나중에 돌든 현재 상태로 한 번 맞춘다.
        private void Start()
        {
            RefreshTurn();
            RefreshEndTurnButton();
            RefreshPiles();

            if (subscribedPlayer != null)
                HandleEnergyChanged(subscribedPlayer.CurrentEnergy, subscribedPlayer.MaxEnergy);
        }

        private void HandleBattleStateChanged(BattleState state)
        {
            RefreshTurn();
            RefreshEndTurnButton();

            switch (state)
            {
                case BattleState.PlayerTurn:
                    ShowBanner($"{battleManager.CurrentTurn} 턴 · 나의 차례");
                    break;

                case BattleState.EnemyTurn:
                    ShowBanner("적의 차례");
                    break;

                case BattleState.Victory:
                case BattleState.Defeat:
                case BattleState.Escape:
                    StartCoroutine(ShowResultRoutine(state));
                    break;
            }
        }

        private void HandleResolvingChanged(bool resolving)
        {
            isResolvingCard = resolving;
            RefreshEndTurnButton();
        }

        private void HandleEnergyChanged(int current, int max)
        {
            if (energyText != null) energyText.text = $"{current}/{max}";
        }

        private void HandleCardMoved(RuntimeCard card, CardListType from, CardListType to) => RefreshPiles();

        private void HandleEndTurnClicked()
        {
            if (battleManager != null) battleManager.EndPlayerTurn();
        }

        private void HandleRestartClicked()
        {
            // 남은 트윈이 파괴된 오브젝트를 건드리지 않도록 정리하고 다시 불러온다.
            DOTween.KillAll();

            Scene scene = SceneManager.GetActiveScene();

#if UNITY_EDITOR
            // 테스트 씬은 Build Settings에 없을 수 있으므로 에디터에서는 경로로 불러온다.
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                scene.path, new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(scene.buildIndex);
#endif
        }

        private void RefreshTurn()
        {
            if (battleManager == null) return;

            if (turnText != null) turnText.text = $"{Mathf.Max(battleManager.CurrentTurn, 1)} 턴";
            if (phaseText != null) phaseText.text = GetPhaseLabel(battleManager.CurrentState);
        }

        private void RefreshEndTurnButton()
        {
            if (endTurnButton == null) return;

            bool isPlayerTurn = battleManager != null && battleManager.CurrentState == BattleState.PlayerTurn;
            endTurnButton.interactable = isPlayerTurn && !isResolvingCard;
        }

        private void RefreshPiles()
        {
            if (cardManager == null) return;

            SetCount(drawPileText, cardManager.GetCards(CardListType.DRAW).Count);
            SetCount(discardPileText, cardManager.GetCards(CardListType.DISCARD).Count);
            SetCount(exhaustPileText, cardManager.GetCards(CardListType.EXHAUST).Count);
        }

        private void ShowBanner(string message)
        {
            if (turnBanner == null) return;

            if (turnBannerText != null) turnBannerText.text = message;

            turnBanner.DOKill();
            turnBanner.alpha = 0f;

            DOTween.Sequence()
                .Append(turnBanner.DOFade(1f, bannerFadeTime))
                .AppendInterval(bannerHoldTime)
                .Append(turnBanner.DOFade(0f, bannerFadeTime))
                .SetTarget(turnBanner);
        }

        private void ShowNotice(string message)
        {
            if (noticeGroup == null) return;

            if (noticeText != null) noticeText.text = message;

            noticeGroup.DOKill();
            noticeGroup.alpha = 1f;

            DOTween.Sequence()
                .AppendInterval(noticeHoldTime)
                .Append(noticeGroup.DOFade(0f, 0.25f))
                .SetTarget(noticeGroup);
        }

        private IEnumerator ShowResultRoutine(BattleState state)
        {
            if (endTurnButton != null) endTurnButton.interactable = false;

            yield return new WaitForSeconds(resultDelay);

            if (resultPanel == null) yield break;

            if (resultTitleText != null) resultTitleText.text = GetResultTitle(state);
            if (resultDetailText != null) resultDetailText.text = GetResultDetail(state);

            resultPanel.SetActive(true);

            CanvasGroup group = resultPanel.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.DOKill();
                group.alpha = 0f;
                group.DOFade(1f, 0.3f);
            }
        }

        private string GetResultDetail(BattleState state)
        {
            int turn = battleManager != null ? battleManager.CurrentTurn : 0;

            switch (state)
            {
                case BattleState.Victory: return $"{turn}턴 만에 적을 쓰러뜨렸습니다.";
                case BattleState.Defeat: return $"{turn}턴에 쓰러졌습니다.";
                case BattleState.Escape: return "적이 시간 너머로 달아났습니다.";
                default: return string.Empty;
            }
        }

        private static string GetResultTitle(BattleState state)
        {
            switch (state)
            {
                case BattleState.Victory: return "승리";
                case BattleState.Defeat: return "패배";
                case BattleState.Escape: return "도주";
                default: return string.Empty;
            }
        }

        private static string GetPhaseLabel(BattleState state)
        {
            switch (state)
            {
                case BattleState.PlayerTurn: return "나의 차례";
                case BattleState.EnemyTurn: return "적의 차례";
                case BattleState.Victory: return "승리";
                case BattleState.Defeat: return "패배";
                case BattleState.Escape: return "전투 종료";
                default: return "전투 준비";
            }
        }

        private static void SetCount(TMP_Text target, int count)
        {
            if (target != null) target.text = count.ToString();
        }

        private static void HideImmediate(CanvasGroup group)
        {
            if (group == null) return;

            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        private void OnDestroy()
        {
            if (turnBanner != null) turnBanner.DOKill();
            if (noticeGroup != null) noticeGroup.DOKill();
        }
    }
}
