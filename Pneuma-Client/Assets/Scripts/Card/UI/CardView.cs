using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Image")]
    [SerializeField] private Image cardImage;

    public CardInstance BoundCard { get; private set; }

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
        Refresh();
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
}
