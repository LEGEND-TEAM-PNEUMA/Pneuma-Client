using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    private const string BattleScene = "SampleScene";
    [SerializeField] private GameObject MainPanel;
    [SerializeField] private GameObject SettingPanel;

    public void OnClickPlay()
    {
        SceneManager.LoadScene(BattleScene);
    }

    public void OnClickCollection()
    {
        // 도감 씬 이동
        Debug.Log("도감 씬으로 이동");
    }

    public void OnClickSettings()
    {
        // 설정 창 열기
        Debug.Log("세팅 UI 활성화");
        MainPanel.SetActive(false);
        SettingPanel.SetActive(true);
    }

    public void OnClickExitSettings()
    {
        // 설정 창 닫기
        Debug.Log("세팅 UI 비활성화");
        SettingPanel.SetActive(false);
        MainPanel.SetActive(true);
    }    

    public void OnClickExit()
    {
        Debug.Log("앱 종료");
        Application.Quit();
    }
}
