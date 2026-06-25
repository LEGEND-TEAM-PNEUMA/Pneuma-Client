using UnityEngine;

/// <summary>
/// 카드 스킬이 전투에서 수행할 효과의 종류를 정의합니다.
/// </summary>
public enum SkillType
{
    [InspectorName("피해")] Damage,
    [InspectorName("방어도")] Block,
    [InspectorName("회복")] Heal,
    [InspectorName("드로우")] Draw,
    [InspectorName("카드 추가")] AddCard,
    [InspectorName("예언 슬롯 증가")] IncreaseProphecySlot
}
