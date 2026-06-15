using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    [SerializeField] private int maxHp = 100;

    public int MaxHp => maxHp;
    public int CurrentHp { get; private set; }
    public int CurrentShield { get; private set; }
    public bool IsDead => CurrentHp <= 0;

    private void Awake()
    {
        CurrentHp = maxHp;
        CurrentShield = 0;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead) return;

        int remainingDamage = damage;

        if (CurrentShield > 0)
        {
            int blockedDamage = Mathf.Min(CurrentShield, remainingDamage);
            CurrentShield -= blockedDamage;
            remainingDamage -= blockedDamage;
        }

        if (remainingDamage > 0)
        {
            CurrentHp -= remainingDamage;
            CurrentHp = Mathf.Max(CurrentHp, 0);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead) return;

        CurrentHp += amount;
        CurrentHp = Mathf.Min(CurrentHp, MaxHp);
    }

    public void AddShield(int amount)
    {
        if (amount <= 0 || IsDead) return;

        CurrentShield += amount;
    }

    public void ClearShield()
    {
        CurrentShield = 0;
    }
}