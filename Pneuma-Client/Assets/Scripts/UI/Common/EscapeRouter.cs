using System.Collections.Generic;
using UnityEngine;

namespace Pneuma.UI.Common
{
    // ESC / 뒤로가기 입력을 지금 화면 맨 위에 있는 대상에게 전달합니다. (UI 기획서 2.1)
    //
    // 기획서 1.3의 "하위 화면에서 ESC 시 상위 화면으로 복귀"를 스택으로 표현합니다.
    // 화면·오버레이가 열릴 때 Push, 닫힐 때 Pop 하면 스택 맨 위가 항상 "지금 보이는 것"이 되고,
    // 오버레이가 떠 있을 때 ESC가 뒤쪽 화면이 아니라 오버레이에 먼저 가는 규칙이 자동으로 지켜집니다.
    //
    // 씬에 미리 배치할 필요 없이 처음 Push 될 때 스스로 생성됩니다.
    [DefaultExecutionOrder(-100)]
    public class EscapeRouter : MonoBehaviour
    {
        private static EscapeRouter instance;

        // 애플리케이션 종료 중에 Instance를 건드리면 이미 파괴된 오브젝트가 되살아나
        // "Some objects were not cleaned up" 경고가 납니다. 종료 중에는 생성하지 않습니다.
        private static bool isQuitting;

        // 맨 뒤가 스택의 top 입니다. List로 두면 중간에서 제거하는 Unregister도 가능합니다.
        private readonly List<IEscapeHandler> handlers = new List<IEscapeHandler>();

        /// <summary>
        /// 현재 ESC 스택에 쌓인 대상 수입니다. (디버그·테스트용)
        /// </summary>
        public int HandlerCount => handlers.Count;

        private static EscapeRouter Instance
        {
            get
            {
                if (instance != null || isQuitting)
                    return instance;

                GameObject routerObject = new GameObject(nameof(EscapeRouter));
                instance = routerObject.AddComponent<EscapeRouter>();
                DontDestroyOnLoad(routerObject);

                return instance;
            }
        }

        /// <summary>
        /// ESC를 받을 대상을 스택 맨 위에 올립니다. 화면·오버레이가 열릴 때 호출합니다.
        /// </summary>
        public static void Push(IEscapeHandler handler)
        {
            if (handler == null || isQuitting)
                return;

            EscapeRouter router = Instance;

            if (router == null)
                return;

            // 같은 대상이 두 번 쌓이면 ESC를 두 번 눌러야 닫히므로, 올리기 전에 기존 항목을 뺍니다.
            router.handlers.Remove(handler);
            router.handlers.Add(handler);
        }

        /// <summary>
        /// 대상을 스택에서 내립니다. 화면·오버레이가 닫힐 때 호출합니다.
        /// 맨 위가 아니어도 안전하게 제거됩니다.
        /// </summary>
        public static void Pop(IEscapeHandler handler)
        {
            if (handler == null || instance == null)
                return;

            instance.handlers.Remove(handler);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // activeInputHandler가 Both(2)로 설정되어 있어 레거시 Input을 그대로 씁니다.
            // BattleManager도 같은 API를 쓰고 있어 입력 방식을 통일합니다.
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            DispatchEscape();
        }

        private void DispatchEscape()
        {
            // 씬이 바뀌면 Pop 없이 파괴된 대상이 남습니다. 전달 직전에 걷어냅니다.
            PruneDestroyedHandlers();

            // 맨 위부터 내려가며, 입력을 소비한 대상에서 멈춥니다.
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                IEscapeHandler handler = handlers[i];

                if (handler == null)
                    continue;

                if (handler.HandleEscape())
                    return;
            }
        }

        // 파괴된 MonoBehaviour는 C# 참조가 남아 있어도 Unity의 == 연산자에서 null로 판정됩니다.
        // 이 판정을 쓰려면 UnityEngine.Object로 캐스팅해야 하므로 인터페이스만으로는 걸러지지 않습니다.
        private void PruneDestroyedHandlers()
        {
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                IEscapeHandler handler = handlers[i];

                if (handler == null)
                {
                    handlers.RemoveAt(i);
                    continue;
                }

                if (handler is UnityEngine.Object unityObject && unityObject == null)
                    handlers.RemoveAt(i);
            }
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}
