using NUnit.Framework;
using UnityEngine;

namespace Pneuma.UI.Card
{
    // 현재 UI 카드 오브젝트가 가지고 있는 카드 데이터와 애니메이션 코드를 관리하는 컴포넌트입니다.
    [RequireComponent(typeof(UICardVisual))]
    public class UICardData : MonoBehaviour
    {
        [SerializeField] private CardData cardData;

        private UICardVisual cardVisual;

        private void Start()
        {
            cardVisual = GetComponent<UICardVisual>();
            if (cardData != null) Debug.LogWarning("[Warning] 카드 데이터가 존재하지 않습니다!");
        }
    }

}