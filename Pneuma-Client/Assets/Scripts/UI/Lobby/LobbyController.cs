using Pneuma.UI.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 로비 화면(UI_003)입니다. (UI 기획서 3.4)
//
// 기획서 기준 동작
//   플레이 → 캐릭터 선택(UI_004)     ※ UI_004 미구현이라 현재는 전투로 직행합니다.
//   사전   → 사전 카테고리 선택(UI_007)  ※ 미구현
//   설정   → 딤드 오버레이 (화면 전환 아님, 기획서 2.2)
//   종료   → 확인창 후 앱 종료
//   (저장된 런이 있을 때만) 계속하기 → 전투(UI_005)
//
// ESC는 로비가 최상위 화면이므로 종료 확인창을 띄웁니다. (기획서 2.1)
public class LobbyController : MonoBehaviour, IEscapeHandler
{
    private const string BattleScene = "SampleScene";

    [Header("오버레이")]
    [Tooltip("설정 오버레이. 기획서 2.2에 따라 화면을 전환하지 않고 딤드 위에 표시합니다.")]
    [SerializeField] private UIOverlay settingsOverlay;

    [Tooltip("종료·경고 등에 공용으로 쓰는 확인창.")]
    [SerializeField] private UIConfirmPopup confirmPopup;

    [Header("저장된 런")]
    [Tooltip("저장된 런이 있을 때만 노출되는 계속하기 버튼. (기획서 3.4-다)")]
    [SerializeField] private Button continueButton;

    private void Start()
    {
        RefreshContinueButton();
    }

    private void OnEnable()
    {
        EscapeRouter.Push(this);
    }

    private void OnDisable()
    {
        EscapeRouter.Pop(this);
    }

    public void OnClickPlay()
    {
        // TODO: 기획서 3.4 기준 플레이는 캐릭터 선택(UI_004)으로 가야 합니다.
        //       UI_004가 생기면 이 줄을 캐릭터 선택 화면 진입으로 교체합니다.
        SceneManager.LoadScene(BattleScene);
    }

    public void OnClickCollection()
    {
        // TODO: 사전 카테고리 선택(UI_007) 구현 후 연결.
        Debug.Log("[LobbyController] 사전(UI_007) 미구현");
    }

    public void OnClickSettings()
    {
        if (settingsOverlay == null)
        {
            Debug.LogError("[LobbyController] settingsOverlay가 연결되지 않았습니다.");
            return;
        }

        // 기획서 2.2: 설정은 화면 전환이 아니라 딤드 오버레이입니다.
        // 예전처럼 로비 패널을 끄면 배경 아트가 사라져 기획서와 다른 화면이 됩니다.
        settingsOverlay.Show();
    }

    public void OnClickExitSettings()
    {
        if (settingsOverlay != null)
            settingsOverlay.Hide();
    }

    public void OnClickContinue()
    {
        // TODO: SaveData가 생기면 저장된 지점의 전투로 복귀합니다. (기획서 3.4-다)
        Debug.Log("[LobbyController] 계속하기 — SaveData 미구현");
    }

    public void OnClickExit()
    {
        RequestQuit();
    }

    /// <summary>
    /// ESC 입력을 처리합니다. 로비는 최상위 화면이므로 종료 확인창을 띄웁니다. (기획서 2.1)
    /// </summary>
    public bool HandleEscape()
    {
        RequestQuit();
        return true;
    }

    private void RequestQuit()
    {
        if (confirmPopup == null)
        {
            // 확인창이 없다고 조용히 종료해 버리면 사용자가 의도치 않게 게임을 잃습니다.
            Debug.LogError("[LobbyController] confirmPopup이 연결되지 않아 종료를 취소합니다.");
            return;
        }

        confirmPopup.Open("정말 종료하시겠습니까?", confirmed =>
        {
            if (confirmed)
                QuitApplication();
        });
    }

    private static void QuitApplication()
    {
        Debug.Log("[LobbyController] 앱 종료");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RefreshContinueButton()
    {
        if (continueButton == null)
            return;

        // TODO: SaveData_Table 연동 시 저장된 런 존재 여부로 교체합니다. (기획서 3.4)
        //       세이브가 손상·누락된 경우에는 비활성화 + 안내 문구가 필요합니다.
        bool hasSavedRun = false;

        continueButton.gameObject.SetActive(hasSavedRun);
    }
}
