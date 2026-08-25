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
        [Header("Player Data")]
        [SerializeField] private PlayerData playerData;

        private int runtimeMaxEnergy;

        public int MaxEnergy => runtimeMaxEnergy;

        // 프로퍼티
        public string CharacterId => playerData != null ? playerData.CharacterId : string.Empty;
        public PlayerData PlayerData => playerData;

        public string CharacterName => playerData != null ? playerData.CharacterName : string.Empty;
        public int CurrentEnergy { get; private set; }

        public event Action<int, int> OnEnergyChanged; // (current, max)

        public bool IsVulnerable { get; private set; }

        protected override void Awake()
        {
            if (playerData == null)
            {
                Debug.LogError("[Player] PlayerData가 연결되지 않았습니다.");
                base.Awake();
                runtimeMaxEnergy = 3;
            }
            else
            {
                InitializeStatus(playerData.MaxHp);
                runtimeMaxEnergy = playerData.MaxEnergy;
            }

            CurrentEnergy = runtimeMaxEnergy;
            OnEnergyChanged?.Invoke(CurrentEnergy, runtimeMaxEnergy);
        }

        public void OnPlayerTurnStarted() // 플레이어 턴 시작 시 에너지, 쉴드 수치 초기화
        {
            RefillEnergy();
            ClearShield();
        }

        public bool CanUseEnergy(int cost) // 에너지 사용가능 여부 확인
        {
            if (IsDead) return false;
            if (cost < 0) return false;

            return CurrentEnergy >= cost;
        }

        public bool TryUseEnergy(int cost) // cost만큼 에너지 차감 및 변화 알림
        {
            if (!CanUseEnergy(cost))
                return false;

            CurrentEnergy -= cost;
            OnEnergyChanged?.Invoke(CurrentEnergy, runtimeMaxEnergy);

            return true;
        }

        public void RefillEnergy() // 에너지 초기화 및 변화 알림
        {
            CurrentEnergy = runtimeMaxEnergy;
            OnEnergyChanged?.Invoke(CurrentEnergy, runtimeMaxEnergy);
        }

        public void RecoverEnergy(int amount) // amount 만큼 에너지 회복
        {
            if (amount <= 0 || IsDead) return;

            CurrentEnergy = CurrentEnergy + amount; // 에너지 최대치 이상 회복 가능
            OnEnergyChanged?.Invoke(CurrentEnergy, runtimeMaxEnergy);
        }

        public void ApplyVulnerable() // 플레이어에게 취약 상태 적용
        {
            if (IsDead)
                return;

            if (IsVulnerable)
            {
                Debug.Log(
                    $"[Player] {CharacterName}: 이미 취약 상태입니다.");

                return;
            }

            IsVulnerable = true;

            Debug.Log(
                $"[Player] {CharacterName}: 취약 상태 적용");
        }

        public override void TakeDamage(int damage) // 취약 상태 적용 여부에 따라 데미지 계산 후 HP 감소
        {
            if (damage <= 0 || IsDead)
                return;

            int finalDamage = damage;

            if (IsVulnerable)
            {
                finalDamage =
                    Mathf.CeilToInt(damage * 1.5f);

                Debug.Log(
                    $"[Player] 취약 피해 증가: " +
                    $"{damage} → {finalDamage}");
            }

            base.TakeDamage(finalDamage);
        }
    }
}