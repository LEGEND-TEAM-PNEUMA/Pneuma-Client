using System;
using UnityEngine;

namespace Pneuma.UI.Card
{
    // 현재 UI 카드 오브젝트가 가지고 있는 카드 데이터와 애니메이션 코드를 관리하는 컴포넌트입니다.
    // 표기(UICardVisual)·연출(UICardAnimator)은 자식 Visual 오브젝트에 위치합니다.
    public class UICardData : MonoBehaviour
    {
        [SerializeField] private CardData cardData;

        public CardData CardData => cardData;

        // 스폰된 카드만 보유합니다. 씬에 직접 배치된 카드는 null입니다.
        public RuntimeCard RuntimeCard { get; private set; }

        private UICardVisual cardVisual;
        private UICardAnimator cardAnimator;
        private UICardDrag cardDrag;

        // 정렬(CardHandLayout)이 제자리를 지정할 대상입니다.
        public UICardDrag CardDrag => cardDrag;

        // 호버 등 상호작용이 연출을 요청할 대상입니다.
        public UICardAnimator CardAnimator => cardAnimator;

        /// <summary>
        /// 손패에서 이 카드가 몇 번째인지. 정렬(CardHandLayout)이 알려줍니다.
        /// 호버로 렌더 순서를 맨 앞에 올렸다가 되돌릴 때 기준이 됩니다.
        /// 씬에 직접 배치되어 정렬을 받지 않는 카드는 -1로 남습니다.
        /// </summary>
        public int HandIndex { get; private set; } = -1;

        // Bind로 데이터가 주입되었는지 여부. Start에서 중복 표기를 막는다.
        private bool isBound;

        private void Awake()
        {
            cardVisual = GetComponentInChildren<UICardVisual>();
            cardAnimator = GetComponentInChildren<UICardAnimator>();

            // 표기·연출과 달리 위치는 카드 루트가 담당하므로 자식에서 찾지 않는다.
            cardDrag = GetComponent<UICardDrag>();
        }

        /// <summary>
        /// CardHandView가 스폰 직후 호출해 런타임 카드를 주입합니다.
        /// </summary>
        /// <param name="card">이 오브젝트가 표기할 런타임 카드</param>
        public void Bind(RuntimeCard card)
        {
            isBound = true;

            RuntimeCard = card;
            cardData = card != null ? card.Data : null;

            ApplyCardData();
        }

        private void Start()
        {
            // 씬에 직접 배치된 카드는 인스펙터에 지정된 데이터로 표기한다.
            if (!isBound)
                ApplyCardData();
        }

        // 카드 데이터를 UI에 표기하고 등장 애니메이션을 재생
        private void ApplyCardData()
        {
            if (cardData == null)
            {
                Debug.LogWarning("[Warning] 카드 데이터가 존재하지 않습니다!");
                return;
            }

            if (cardVisual != null) cardVisual.SetCard(cardData);
            if (cardAnimator != null) cardAnimator.PlayAppear();
        }

        /// <summary>
        /// 손패에서 이 카드의 순서를 지정합니다. 정렬(CardHandLayout)이 호출합니다.
        /// </summary>
        /// <param name="index">손패 내 순서(0부터)</param>
        public void SetHandIndex(int index) => HandIndex = index;

        /// <summary>
        /// 손패에서 빠지는 연출을 재생하고, 끝난 뒤 콜백을 호출합니다.
        /// 연출을 붙이지 못한 카드는 곧바로 콜백을 호출해 호출자가 멈추지 않게 합니다.
        /// </summary>
        /// <param name="onComplete">연출이 끝난 뒤 실행할 동작</param>
        public void PlayDisappear(Action onComplete)
        {
            // 사라지는 중인 카드를 다시 집으면 드래그가 위치를 도로 가져가 연출이 어긋난다.
            if (cardDrag != null) cardDrag.Interactable = false;

            if (cardAnimator == null)
            {
                onComplete?.Invoke();
                return;
            }

            cardAnimator.PlayDisappear(onComplete);
        }
    }

}
