using System.Collections.Generic;
using UnityEngine;

namespace Pneuma.Unit
{
    /// <summary>
    /// 한 몬스터가 사용할 수 있는 행동 목록을 보관하는 데이터다.
    /// 행동 선택과 쿨다운 계산은 EnemyActionController에서 담당한다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "EnemyBehaviorData",
        menuName = "Pneuma/Unit/Enemy Behavior Data")]
    public class EnemyBehaviorData : ScriptableObject
    {
        [Header("Enemy Actions")]
        [SerializeField] private List<EnemyActionData> actions = new();

        public IReadOnlyList<EnemyActionData> Actions => actions;
    }
}