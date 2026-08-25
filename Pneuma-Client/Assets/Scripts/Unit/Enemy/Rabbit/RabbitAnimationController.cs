using Pneuma.Unit;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Enemy))]
public class RabbitAnimationController : MonoBehaviour
{
    private static readonly int IdleHash =
        Animator.StringToHash("Idle");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int HitHash =
        Animator.StringToHash("Hit");

    private static readonly int DefeatHash =
        Animator.StringToHash("Defeat");

    private static readonly int RunHash =
        Animator.StringToHash("Run");

    private Animator animator;
    private Enemy enemy;

    private bool isDefeated;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        if (enemy != null)
        {
            enemy.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath(CharacterBase deadUnit)
    {
        PlayDefeat();
    }

    public void PlayAttack()
    {
        if (isDefeated)
        {
            return;
        }

        animator.SetTrigger(AttackHash);
    }

    public void PlayHit()
    {
        if (isDefeated)
        {
            return;
        }

        animator.SetTrigger(HitHash);
    }

    public void PlayDefeat()
    {
        if (isDefeated)
        {
            return;
        }

        isDefeated = true;

        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(HitHash);
        animator.ResetTrigger(RunHash);

        animator.SetTrigger(DefeatHash);
    }

    public void PlayRun()
    {
        if (isDefeated)
        {
            return;
        }

        animator.SetTrigger(RunHash);
    }

    public void ResetAnimationState()
    {
        isDefeated = false;

        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(HitHash);
        animator.ResetTrigger(DefeatHash);
        animator.ResetTrigger(RunHash);

        animator.Play(IdleHash, 0, 0f);
    }
}