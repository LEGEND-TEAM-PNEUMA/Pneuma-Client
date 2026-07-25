using UnityEngine;

namespace Pneuma.Unit
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Pneuma/Unit/Player Data")]
    public class PlayerData : ScriptableObject
    {
        [Header("Player Info")]
        [SerializeField] private string characterId;
        [SerializeField] private string characterName;

        [Header("Status")]
        [SerializeField, Min(1)] private int maxHp = 100;

        [Header("Battle Resource")]
        [SerializeField, Range(0, 10)] private int maxEnergy = 3;

        [Header("Visual")]
        [SerializeField] private Sprite characterSprite;
        [SerializeField] private RuntimeAnimatorController animatorController;

        public string CharacterId => characterId;
        public string CharacterName => characterName;
        public int MaxHp => maxHp;
        public int MaxEnergy => maxEnergy;
        public Sprite CharacterSprite => characterSprite;
        public RuntimeAnimatorController AnimatorController => animatorController;
    }
}