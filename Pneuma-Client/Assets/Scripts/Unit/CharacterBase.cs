using System;
using UnityEngine;

namespace Pneuma.Unit
{
    public class CharacterBase : MonoBehaviour
    {
        [Header("Status Settings")]
        [SerializeField, Min(1)] private int maxHp = 100;

        public int MaxHp => maxHp;
        public int CurrentHp { get; private set; }
        public int CurrentShield { get; private set; }
        public bool IsDead => CurrentHp <= 0;

        // 상태 변화를 알리기 위한 이벤트
        public event Action<int, int> OnHpChanged; // (current, max)
        public event Action<int> OnShieldChanged;
        public event Action<CharacterBase> OnDeath;

        private void Awake()
        {
            CurrentHp = maxHp;
            CurrentShield = 0;
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;

            bool wasAlive = !IsDead; //OnDeath 중복 호출 방지
            int remainingDamage = damage;

            if (CurrentShield > 0)
            {
                int blockedDamage = Mathf.Min(CurrentShield, remainingDamage);
                CurrentShield -= blockedDamage;
                remainingDamage -= blockedDamage;
                OnShieldChanged?.Invoke(CurrentShield);
            }

            if (remainingDamage > 0)
            {
                CurrentHp = Mathf.Max(CurrentHp - remainingDamage, 0);
                OnHpChanged?.Invoke(CurrentHp, maxHp);
            }

            if (wasAlive && IsDead)
            {
                // 사망 판단
                OnDeath?.Invoke(this);
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead) return;
            
            int prevHp = CurrentHp;
            CurrentHp = Mathf.Min(CurrentHp + amount, maxHp);

            if (prevHp == CurrentHp) return;
            OnHpChanged?.Invoke(CurrentHp, maxHp);
        }

        public void IncreaseMaxHp(int amount)
        {
            if (amount <= 0) return;

            maxHp += amount;
            CurrentHp += amount;

            OnHpChanged?.Invoke(CurrentHp, maxHp);
        }

        public void AddShield(int amount)
        {
            if (amount <= 0 || IsDead) return;

            CurrentShield += amount;
            OnShieldChanged?.Invoke(CurrentShield);
        }

        public void ClearShield()
        {
            if (CurrentShield == 0) return;
            
            CurrentShield = 0;
            OnShieldChanged?.Invoke(CurrentShield);
        }
    }
}