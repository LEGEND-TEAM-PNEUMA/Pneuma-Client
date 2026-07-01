using System;
using UnityEngine;

[Serializable]
public class CardSkill
{
    [SerializeField] private SkillType skillType;
    [SerializeField] private int value;
    [SerializeField] private SkillTargetType target;
    [SerializeField, Min(0)] private int targetCount = 1;

    public SkillType SkillType => skillType;
    public int Value => value;
    public SkillTargetType Target => target;
    public int TargetCount => targetCount;
}
