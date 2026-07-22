using System;
using UnityEngine;

namespace Pneuma.Unit
{
    public enum EnemyActionSelectionType
    {
        RandomPool,
        FixedTurn
    }

    /// <summary>
    /// 몬스터 행동 테이블의 한 행을 나타내는 정적 데이터다.
    /// 전투 중 사용 횟수와 마지막 사용 턴은 EnemyActionRuntime에서 관리한다.
    /// </summary>
    [Serializable]
    public class EnemyActionData
    {
        [Header("Action Info")]
        [SerializeField] private string actionName;
        [SerializeField] private int skillGroupId;

        [Header("Selection Rule")]
        [SerializeField] private EnemyActionSelectionType selectionType;

        [Tooltip("FixedTurn 행동이 실행되는 턴. RandomPool이면 0")]
        [SerializeField, Min(0)] private int fixedTurn;

        [Tooltip("이 행동이 사용되는 몬스터 페이즈")]
        [SerializeField, Min(1)] private int phase = 1;

        [Tooltip("이 턴부터 RandomPool 후보에 포함된다.")]
        [SerializeField, Min(1)] private int minAppearTurn = 1;

        [Tooltip("전투 중 최대 사용 횟수. 0이면 제한 없음")]
        [SerializeField, Min(0)] private int maxAppearCount;

        [Tooltip("사용 후 다시 선택되지 않는 턴 수")]
        [SerializeField, Min(0)] private int cooldownTurns;

        public string ActionName => actionName;
        public int SkillGroupId => skillGroupId;
        public EnemyActionSelectionType SelectionType => selectionType;
        public int FixedTurn => fixedTurn;
        public int Phase => phase;
        public int MinAppearTurn => minAppearTurn;
        public int MaxAppearCount => maxAppearCount;
        public int CooldownTurns => cooldownTurns;
    }
}