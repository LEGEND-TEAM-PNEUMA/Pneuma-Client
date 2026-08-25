using System;
using UnityEngine;

namespace Pneuma.Unit
{
    public class CharacterBase : MonoBehaviour
    {
        [Header("Status Settings")]
        [SerializeField] private int maxHp = 100;

        public int MaxHp => maxHp;
        public int CurrentHp { get; private set; }
        public int CurrentShield { get; private set; }
        public bool IsDead => CurrentHp <= 0;

        // 상태 변화를 알리기 위한 이벤트
        public event Action<int, int> OnHpChanged; // (current, max)
        public event Action<int> OnShieldChanged;
        public event Action<CharacterBase> OnDeath;

        protected virtual void Awake()
        {
            InitializeStatus(maxHp);
        }

        protected void InitializeStatus(int newMaxHp)
        {
            maxHp = newMaxHp;
            CurrentHp = maxHp;
            CurrentShield = 0;

            OnHpChanged?.Invoke(CurrentHp, maxHp);
            OnShieldChanged?.Invoke(CurrentShield);
        }

        public virtual void TakeDamage(int damage)
        {
            if (damage <= 0 || IsDead) return;

            bool wasAlive = !IsDead;
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

            Debug.Log("TakeDamage: " + damage);
            Debug.Log("CurrentHp: " + CurrentHp);

            if (wasAlive && IsDead)
            {
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

            Debug.Log("Heal: " + amount);
            Debug.Log("CurrentHp: " + CurrentHp);
        }

        public void IncreaseMaxHp(int amount)
        {
            if (amount <= 0 || IsDead) return;

            maxHp += amount;
            CurrentHp += amount;

            OnHpChanged?.Invoke(CurrentHp, maxHp);
        }

        public void AddShield(int amount)
        {
            if (amount <= 0 || IsDead) return;

            CurrentShield += amount;
            OnShieldChanged?.Invoke(CurrentShield);

            Debug.Log("AddShield: " + amount);
            Debug.Log("CurrentShield: " + CurrentShield);
        }

        public void ClearShield()
        {
            if (CurrentShield == 0) return;

            CurrentShield = 0;
            OnShieldChanged?.Invoke(CurrentShield);
        }

        /// <summary>
        /// 데미지 계산 없이 즉시 사망 처리한다.
        /// 도주 등 데미지가 아닌 사유로 전투 이탈을 표현할 때 사용한다.
        /// </summary>
        public void Kill()
        {
            if (IsDead) return;

            CurrentShield = 0;
            CurrentHp = 0;

            OnShieldChanged?.Invoke(CurrentShield);
            OnHpChanged?.Invoke(CurrentHp, maxHp);

            Debug.Log($"{name} Kill 처리");

            OnDeath?.Invoke(this);
        }
    }
}