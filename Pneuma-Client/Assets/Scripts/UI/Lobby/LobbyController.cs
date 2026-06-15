using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pneuma.UI.Lobby
{
    public class LobbyController : MonoBehaviour
    {
        private const string BattleScene = "SampleScene";

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
        }

        public void OnClickExit()
        {
            Debug.Log("앱 종료");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
