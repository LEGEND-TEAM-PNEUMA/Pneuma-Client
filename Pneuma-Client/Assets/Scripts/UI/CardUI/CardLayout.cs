using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

[System.Serializable]
public class CardObject
{
    public string name;
    public CardUI cardUI;
}

// 카드의 정렬을 관리하고 추가 및 삭제를 관리합니다.
public class CardLayout : MonoBehaviour
{
    [Header("Element Settings")]
    [SerializeField] private float spacing = 50f;
    [SerializeField] private float cardRotation = 15f;
    [SerializeField] private float cardYOffset = 0.05f;
    [SerializeField] private Vector2 offset = new Vector2(0, 0);
    [SerializeField] private List<CardObject> cardObjects;

    [Header("Select Settings")]
    // 카드 선택 시 선택 카드 양옆을 바깥으로 밀어내는 고정폭
    [SerializeField] private float pushAmount = 40f;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.2f;

    // 프로토타입용 임시 변수
    private CardUI selectedCard = null;

    public event System.Action OnCardRemoved;
    // [임시 코드] UI 테스트용. 선택 카드가 바뀔 때마다 호출 (선택 해제 시 null)
    // 추후 정식 선택 관리자 도입 시 제거할 것
    public event System.Action<CardUI> OnSelectionChanged;

    private void Start()
    {
        StartCoroutine(TestSpawnCards(5));
    }

    private void AddCard(string name)
    {
        if(cardObjects == null || cardObjects.Count == 0)
        {
            Debug.LogError("님 카드 오브젝트 없어요");
            return;
        }

        int rand = Random.Range(0, cardObjects.Count);

        GameObject newCard = Instantiate(cardObjects[rand].cardUI.gameObject, transform);

        CardUI newCardUI = newCard.GetComponent<CardUI>();
        newCardUI.SetIndex(transform.childCount - 1);
        newCardUI.onClick.AddListener(() => SelectCard(newCardUI));

        UpdateLayout();
    }

    // 선택 상태를 갱신한다. 같은 카드를 다시 누르면 선택 해제(토글).
    private void SelectCard(CardUI card)
    {
        if (selectedCard == card)
        {
            card.OnDeselect();
            selectedCard = null;
        }
        else
        {
            if (selectedCard != null)
                selectedCard.OnDeselect();

            selectedCard = card;
            card.OnSelected();
        }

        UpdateLayout();

        // [임시 코드] UI 테스트용 선택 카드 표시 갱신
        OnSelectionChanged?.Invoke(selectedCard);
    }

    // 사라지는 중인 카드를 제외한 현재 카드들을 논리 인덱스 순으로 모은다.
    // 선택 시 형제 순서가 뒤섞이므로(SetAsLastSibling) Index 기준으로 정렬한다.
    private List<CardUI> GetActiveCards()
    {
        var cards = new List<CardUI>();

        for (int i = 0; i < transform.childCount; i++)
        {
            CardUI cardUI = transform.GetChild(i).GetComponent<CardUI>();
            if (cardUI != null && !cardUI.IsRemoving)
                cards.Add(cardUI);
        }

        cards.Sort((a, b) => a.Index.CompareTo(b.Index));
        return cards;
    }

    // 살아있는 카드만으로 부채꼴 정렬한다.
    // 삭제로 생긴 인덱스 구멍을 0..n-1로 다시 메우므로, 카드가 빠지면 자연히 자리를 채운다.
    // 위치 계산은 형제 순서가 아닌 논리 인덱스를 사용하므로,
    // 선택 카드를 렌더링상 맨 앞으로 옮겨도(SetAsLastSibling) 정렬은 흐트러지지 않는다.
    private void UpdateLayout()
    {
        List<CardUI> cards = GetActiveCards();
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        float totalWidth = (cardCount - 1) * spacing;
        float startX = -totalWidth / 2f;
        float mid = (cardCount - 1) / 2f;

        int selectedIndex = (selectedCard != null && !selectedCard.IsRemoving) ? selectedCard.Index : -1;

        for (int i = 0; i < cardCount; i++)
        {
            CardUI cardUI = cards[i];
            // 논리 인덱스를 0..n-1로 다시 매겨 삭제로 생긴 구멍을 제거한다.
            cardUI.SetIndex(i);

            Transform card = cardUI.transform;
            float t = i - mid;

            float targetX = startX + i * spacing + offset.x;
            float targetY = -t * t * cardYOffset + offset.y;
            float targetRot = -t * cardRotation;

            // 선택 카드 기준 좌/우로 고정폭만큼 밀어내 빈 공간을 만든다.
            if (selectedIndex >= 0)
            {
                if (i < selectedIndex) targetX -= pushAmount;
                else if (i > selectedIndex) targetX += pushAmount;
            }

            card.DOLocalMove(new Vector3(targetX, targetY, 0), animationDuration);
            // 회전은 CardUI가 관리한다(호버/선택 중에는 0도를 유지하기 위함).
            cardUI.SetLayoutRotation(targetRot, animationDuration);
        }
    }

    // [임시 코드] UI 테스트용. 현재 선택된 카드를 제거한다.
    public void RemoveSelectedCard()
    {
        if (selectedCard != null)
            RemoveCardInstance(selectedCard);
    }

    // 스폰된 카드 인스턴스 하나를 제거한다.
    // 삭제 시작 즉시 정렬 대상에서 빠지므로(IsRemoving), 카드가 떨어지는 동안 남은 카드가 빈 자리를 메운다.
    public void RemoveCardInstance(CardUI card)
    {
        if (card == null || card.IsRemoving) return;

        if (selectedCard == card)
        {
            selectedCard = null;
            // [임시 코드] UI 테스트용 선택 카드 표시 갱신
            OnSelectionChanged?.Invoke(null);
        }

        card.RemoveCard(() => OnCardRemoved?.Invoke());
        UpdateLayout();
    }

    // CardObject 리스트에 담겨 있는 랜덤한 카드를 가져오는 함수
    // 추후 플레이어가 보유 중인 카드 리스트에서 랜덤한 카드를 가져오는 함수로 변경할 것
    public void AddRandomCard()
    {
        int rand = Random.Range(0, cardObjects.Count);
        string randomCardName = cardObjects[rand].name;
        AddCard(randomCardName);
    }

    // 현재 보유중인 카드의 이름을 찾아 제거하는 함수
    // 추후 완전 제거가 아닌 더미 리스트에 보관할 것
    public void RemoveCard(string cardName)
    {
        foreach (var card in cardObjects)
        {
            if (card.name == cardName)
            {
                card.cardUI.RemoveCard();
                break;
            }
        }
    }

    #region UI 테스트용

    // UI 테스트용 함수
    // 스폰된 카드 인스턴스(자식)를 모두 제거한다.
    public void ClearAllCard()
    {
        selectedCard = null;
        // [임시 코드] UI 테스트용 선택 카드 표시 갱신
        OnSelectionChanged?.Invoke(null);

        foreach (CardUI card in GetActiveCards())
            card.RemoveCard();
    }

    // 테스트용 함수
    // 추후 제거할 것
    public void RespawnCard()
    {
        ClearAllCard();
        StartCoroutine(TestSpawnCards(5));
    }

    private IEnumerator TestSpawnCards(int count, float delay = 0.15f)
    {
        for (int i = 0; i < count; i++)
        {
            AddRandomCard();
            yield return new WaitForSeconds(delay);
        }
    }

    #endregion
}
