using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pneuma.UI.Card
{
    // 카드 더미 한 곳(뽑을/버린/소멸 등)의 장수를 화면에 표기하는 뷰입니다.
    // CardManager의 카드 이동을 구독해 카운트만 갱신하며, 카드 흐름에는 관여하지 않습니다.
    // (기획서 3.2.6)
    //
    // 더미 한 곳당 컴포넌트 하나를 붙이고 pileType으로 대상을 지정합니다.
    // 뽑을 더미와 버린 더미가 필요하면 오브젝트를 둘 두고 타입만 다르게 지정하면 됩니다.
    public class CardPileView : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] private CardManager cardManager;

        [Header("표기 대상")]
        // 이 오브젝트가 표기할 더미. DRAW(뽑을) / DISCARD(버린) 등
        [SerializeField] private CardListType pileType = CardListType.DRAW;

        [Header("UI 요소")]
        // 장수를 표기할 텍스트. 없으면 카운트 표기를 건너뜁니다.
        [SerializeField] private TMP_Text countText;

        // 더미가 비었을 때 숨길 오브젝트(카드 뒷면 등). 지정하지 않으면 항상 보입니다.
        // countText가 이 오브젝트의 자식이어도 카운트는 계속 보입니다. (아래 CacheBodyGraphics 참조)
        [SerializeField] private GameObject pileBody;

        // 카운트 표기 형식. {0}에 장수가 들어갑니다.
        [SerializeField] private string countFormat = "{0}";

        /// <summary>
        /// 이 뷰가 표기 중인 더미의 현재 장수입니다.
        /// </summary>
        public int Count { get; private set; }

        // 빈 더미일 때 끌 그래픽들. countText 쪽은 제외해 캐싱해둡니다.
        private readonly List<Graphic> bodyGraphics = new List<Graphic>();

        private void Awake()
        {
            CacheBodyGraphics();
        }

        // CardManager가 Start()에서 드로우하므로, 그보다 먼저 실행되는 OnEnable에서 구독한다.
        // (CardHandView와 같은 이유)
        private void OnEnable()
        {
            if (cardManager == null)
            {
                Debug.LogError($"[CardPileView] CardManager가 연결되지 않았습니다. ({pileType})");
                return;
            }

            cardManager.OnCardMoved += HandleCardMoved;
        }

        private void OnDisable()
        {
            if (cardManager != null)
                cardManager.OnCardMoved -= HandleCardMoved;
        }

        // CardManager의 더미 딕셔너리는 Awake에서 만들어지므로 Start에서 초기 표기를 맞춘다.
        // 이후 변화는 전부 OnCardMoved로 들어온다.
        private void Start()
        {
            Refresh();
        }

        // 이 더미가 출발지나 도착지일 때만 갱신한다. 무관한 이동에는 반응하지 않는다.
        private void HandleCardMoved(RuntimeCard card, CardListType from, CardListType to)
        {
            if (from != pileType && to != pileType) return;

            Refresh();
        }

        /// <summary>
        /// 더미의 현재 장수를 다시 읽어 표기를 갱신합니다.
        /// 카드 이동 시 자동으로 호출되며, 외부에서 강제로 맞출 때도 쓸 수 있습니다.
        /// </summary>
        public void Refresh()
        {
            if (cardManager == null) return;

            Count = cardManager.GetCards(pileType).Count;

            if (countText != null)
                countText.text = string.Format(countFormat, Count);

            // 빈 더미는 카드 뒷면을 감춰 "뽑을 카드가 없다"가 눈에 보이게 한다.
            // 오브젝트를 끄지 않고 그래픽만 끄므로, countText가 pileBody 자식이어도 0장 표기는 남는다.
            for (int i = 0; i < bodyGraphics.Count; i++)
            {
                if (bodyGraphics[i] != null)
                    bodyGraphics[i].enabled = Count > 0;
            }
        }

        // pileBody 하위의 그래픽을 모아둡니다. 단, countText와 그 하위는 제외합니다.
        // 카운트 텍스트를 카드 뒷면의 자식으로 두는 배치가 흔한데, 오브젝트를 통째로 끄면
        // 빈 더미에서 숫자까지 사라져 아무것도 안 보이기 때문입니다.
        private void CacheBodyGraphics()
        {
            bodyGraphics.Clear();

            if (pileBody == null) return;

            Transform countRoot = countText != null ? countText.transform : null;

            // 비활성 상태로 시작하는 그래픽도 포함해 모읍니다.
            foreach (Graphic graphic in pileBody.GetComponentsInChildren<Graphic>(true))
            {
                if (countRoot != null && graphic.transform.IsChildOf(countRoot)) continue;

                bodyGraphics.Add(graphic);
            }
        }
    }

}
