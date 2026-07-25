using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Pneuma/Card/Card Data")]
public class CardData : ScriptableObject
{
    [SerializeField] private string cardName;
    [SerializeField] private CardType cardType;
    [SerializeField] private CardRarity cardRarity;
    [SerializeField] private string cardCharacter;
    [SerializeField, Range(0, 3)] private int cardCost;

    [SerializeField] private List<CardSkill> cardSkills = new List<CardSkill>();

    [SerializeField, Min(0)] private int cardMaxCount;
    [SerializeField] private bool cardOncePerTurn;
    [SerializeField] private bool cardExhausts;
    [SerializeField] private bool cardCanProphecy;

    [SerializeField] private bool cardIsProphecy;
    [SerializeField] private bool cardIsUpgraded;
    [SerializeField] private CardData cardUpgradeTarget;
    [SerializeField] private CardData cardUpgradedFrom;

    [SerializeField] private bool cardHasCutscene;
    [SerializeField] private AudioClip cardCutsceneSound;

    [SerializeField] private Sprite cardIllust;
    [SerializeField, TextArea] private string cardDescription;

    public string CardName => cardName;
    public CardType CardType => cardType;
    public CardRarity CardRarity => cardRarity;
    public string CardCharacter => cardCharacter;
    public int CardCost => cardCost;
    public IReadOnlyList<CardSkill> CardSkills => cardSkills;
    public int CardMaxCount => cardMaxCount;
    public bool CardOncePerTurn => cardOncePerTurn;
    public bool CardExhausts => cardExhausts;
    public bool CardCanProphecy => cardCanProphecy;
    public bool CardIsProphecy => cardIsProphecy;
    public bool CardIsUpgraded => cardIsUpgraded;
    public CardData CardUpgradeTarget => cardUpgradeTarget;
    public CardData CardUpgradedFrom => cardUpgradedFrom;
    public bool CardHasCutscene => cardHasCutscene;
    public AudioClip CardCutsceneSound => cardCutsceneSound;
    public Sprite CardIllust => cardIllust;
    public string CardDescription => cardDescription;
}
