using Pneuma.Unit;
using UnityEngine;

public class CharacterTest : MonoBehaviour
{
    [SerializeField] private CharacterBase character;

    private void OnEnable()
    {
        character.OnHpChanged += OnHpChanged;
        character.OnShieldChanged += OnShieldChanged;
        character.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        character.OnHpChanged -= OnHpChanged;
        character.OnShieldChanged -= OnShieldChanged;
        character.OnDeath -= OnDeath;
    }

    private void Start()
    {
        character.TakeDamage(30);
        character.AddShield(20);
        character.Heal(10);
        character.TakeDamage(100);
    }

    private void OnHpChanged(int current, int max)
    {
        Debug.Log($"HP Changed: {current}/{max}"); 
    }

    private void OnShieldChanged(int shield)
    {
        Debug.Log($"Shield Changed: {shield}");
    }

    private void OnDeath(CharacterBase deadCharacter)
    {
        Debug.Log($"Dead: {deadCharacter.name}");
    }
}