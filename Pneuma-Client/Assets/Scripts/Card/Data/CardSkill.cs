using System;
using UnityEngine;

[Serializable]
public class CardSkill
{
    [SerializeField] private SkillType skillType;
    [SerializeField] private int value;
    [SerializeField, Min(1)] private int hitCount = 1;
    [SerializeField] private SkillTargetType target;
    [SerializeField, Min(0)] private int targetCount = 1;
    [SerializeField, Min(0)] private int statusEffectDuration;
    [SerializeField] private CardData createdCard;
    [SerializeField] private string summonGroupId;

    public SkillType SkillType => skillType;
    public int Value => value;
    public int HitCount => hitCount;
    public SkillTargetType Target => target;
    public int TargetCount => targetCount;
    public int StatusEffectDuration => statusEffectDuration;
    public CardData CreatedCard => createdCard;
    public string SummonGroupId => summonGroupId;
}
