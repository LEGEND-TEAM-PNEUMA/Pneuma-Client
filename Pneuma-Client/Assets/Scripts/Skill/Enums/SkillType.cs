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
    [InspectorName("예언 슬롯 증가")] IncreaseProphecySlot,
    [InspectorName("에너지 회복")] EnergyHeal,
    [InspectorName("취약")] Vulnerable,
    [InspectorName("약화")] Weaken,
    [InspectorName("강화")] Upgrade,
    [InspectorName("소환")] Summon,
    [InspectorName("도주")] Escape,
    [InspectorName("낙인")] Brand,
    [InspectorName("패시브 - 예언 시 트리거")] ProphecyTriggerPassive,
    [InspectorName("동적 피해 - 예언슬롯 수 기반")] ProphecySlotDamage,
    [InspectorName("허약")] Frail,
    [InspectorName("방어도 부여")] GrantBlock,
    [InspectorName("공격력 강화")] AttackPowerBuff,
    [InspectorName("패시브 - 턴 시작 시 방어도 부여")] TurnStartBlockPassive,
    [InspectorName("패시브 - 예언 처치 시 에너지 회복")] ProphecyKillEnergyPassive,
    [InspectorName("에너지 0 턴 종료 시 다음 턴 에너지 회복")] EmptyEnergyNextTurnEnergyPassive
}
