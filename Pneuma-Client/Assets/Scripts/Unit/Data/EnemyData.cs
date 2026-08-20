using UnityEngine;
//적 기본 정보와 행동 데이터 SO 참조
namespace Pneuma.Unit
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Pneuma/Unit/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Enemy Info")]
        [SerializeField] private string enemyId;
        [SerializeField] private string enemyName;

        [Header("Status")]
        [SerializeField, Min(1)] private int maxHp = 100;

        [Header("Behavior")]
        [SerializeField] private EnemyBehaviorData behaviorData;

        [Header("Visual")]
        [SerializeField] private Sprite enemySprite;
        [SerializeField] private RuntimeAnimatorController animatorController;

        public string EnemyId => enemyId;
        public string EnemyName => enemyName;
        public int MaxHp => maxHp;

        public EnemyBehaviorData BehaviorData => behaviorData;

        public Sprite EnemySprite => enemySprite;
        public RuntimeAnimatorController AnimatorController => animatorController;
    }
}