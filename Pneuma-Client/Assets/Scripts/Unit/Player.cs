using System;
using UnityEngine;

namespace Pneuma.Unit
{
    /// <summary>
    /// Player 전용 정보와 전투 자원(Energy)을 관리한다.
    /// HP, Shield, Death 처리는 CharacterBase에서 담당한다.
    /// </summary>
    public class Player : CharacterBase
    {
        [Header("Player Info")]
        [SerializeField] private string characterName;

        [Header("Battle Resource")]
        [SerializeField] private int maxEnergy = 3;

        public string CharacterName => characterName;

        public int MaxEnergy => maxEnergy;
        public int CurrentEnergy { get; private set; }

        public event Action<int, int> OnEnergyChanged; // (current, max)

        protected override void Awake()
        {
            base.Awake();

            CurrentEnergy = maxEnergy;
        }

        public void OnPlayerTurnStarted()
        {
            RefillEnergy();
            ClearShield();
        }

        public bool CanUseEnergy(int cost)
        {
            if (IsDead) return false;
            if (cost < 0) return false;

            return CurrentEnergy >= cost;
        }

        public bool TryUseEnergy(int cost)
        {
            if (!CanUseEnergy(cost))
                return false;

            CurrentEnergy -= cost;
            OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);

            return true;
        }

        public void RefillEnergy()
        {
            CurrentEnergy = maxEnergy;
            OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        }

        public void RecoverEnergy(int amount)
        {
            if (amount <= 0 || IsDead) return;

            CurrentEnergy = Mathf.Min(CurrentEnergy + amount, maxEnergy);
            OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        }
    }
}