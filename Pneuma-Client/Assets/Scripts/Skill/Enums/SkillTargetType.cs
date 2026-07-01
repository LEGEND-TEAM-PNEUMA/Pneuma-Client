using UnityEngine;

/// <summary>
/// 카드 스킬 효과가 적용될 대상 범위를 정의합니다.
/// </summary>
public enum SkillTargetType
{
    [InspectorName("없음")] None,
    [InspectorName("자신")] Self,
    [InspectorName("적")] Enemy,
    [InspectorName("랜덤 적")] RandomEnemy,
    [InspectorName("모든 적")] AllEnemy
}
