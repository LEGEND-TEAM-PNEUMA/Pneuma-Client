using TMPro;
using UnityEngine;
using UnityEngine.UI;

// [임시 코드] UI 테스트용 캔버스. 추후 정식 UI 도입 시 제거할 것
public class UITestCanvas : MonoBehaviour
{
    [SerializeField] private CardLayout cardLayout;
    // [임시 코드] 현재 선택된 카드 이름을 보여줄 텍스트
    [SerializeField] private Text selectedCardText;

    private void OnEnable()
    {
        if (cardLayout != null)
            cardLayout.OnSelectionChanged += UpdateSelectedCardText;
    }

    private void OnDisable()
    {
        if (cardLayout != null)
            cardLayout.OnSelectionChanged -= UpdateSelectedCardText;
    }

    public void AddRandomCard() => cardLayout.AddRandomCard();

    // [임시 코드] 현재 선택된 카드를 제거한다. (선택 해제되며 표시 텍스트도 비워짐)
    public void RemoveSelectedCard() => cardLayout.RemoveSelectedCard();

    public void RespawnCard()
    {
        cardLayout.RespawnCard();
    }

    // [임시 코드] 선택된 카드의 게임오브젝트 이름을 텍스트로 표시한다. 선택 해제되면 비운다.
    private void UpdateSelectedCardText(CardUI card)
    {
        if (selectedCardText == null) return;

        selectedCardText.text = card != null ? card.gameObject.name : string.Empty;
    }
}
