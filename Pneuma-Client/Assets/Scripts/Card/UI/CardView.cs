using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CardView : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Image")]
    [SerializeField] private Image cardImage;

    private Button button;

    public CardInstance BoundCard { get; private set; }
    public event Action<CardView> Clicked;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
        button.interactable = false;
    }

    public void SetDisplayReferences(
        TMP_Text cardNameText,
        TMP_Text costText,
        TMP_Text descriptionText,
        Image cardImage = null)
    {
        this.cardNameText = cardNameText;
        this.costText = costText;
        this.descriptionText = descriptionText;
        this.cardImage = cardImage;
    }

    public void Bind(CardInstance card)
    {
        BoundCard = card;
        SetInteractable(card != null);
        Refresh();
    }

    public void SetInteractable(bool value)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = value && BoundCard != null;
    }

    public void Refresh()
    {
        if (BoundCard == null)
        {
            SetText(cardNameText, string.Empty);
            SetText(costText, string.Empty);
            SetText(descriptionText, string.Empty);
            SetImage(null);
            return;
        }

        CardData data = BoundCard.Data;
        // TODO: 한글 TMP 폰트 에셋 추가 후 카드 텍스트 표시를 활성화합니다.
        // SetText(cardNameText, data.CardName);
        // SetText(costText, BoundCard.CurrentCost.ToString());
        // SetText(descriptionText, data.CardDescription);
        SetImage(data.CardIllust);
    }

    private void SetImage(Sprite sprite)
    {
        if (cardImage == null)
        {
            return;
        }

        cardImage.sprite = sprite;
        cardImage.enabled = sprite != null;
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target == null)
        {
            return;
        }

        target.text = value;
    }

    private void HandleClick()
    {
        if (BoundCard == null)
        {
            return;
        }

        Clicked?.Invoke(this);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }
}
