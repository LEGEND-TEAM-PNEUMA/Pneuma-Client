using System.Collections;
using Pneuma.Unit;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Enemy))]
public class RabbitAnimationController : MonoBehaviour
{
    private static readonly int IdleHash = Animator.StringToHash("Idle");

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private static readonly int HitHash = Animator.StringToHash("Hit");

    private static readonly int DefeatHash = Animator.StringToHash("Defeat");

    private static readonly int RunHash = Animator.StringToHash("Run");

    [Header("Run")]
    [SerializeField, Min(0f)]
    private float runSpeed = 5f;

    [Tooltip("1.0이 화면 오른쪽 끝. 1.1이면 화면보다 10% 더 나간 뒤 종료")]
    [SerializeField, Min(1f)]
    private float exitViewportX = 1.1f;

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

    public IEnumerator PlayRunAndExit()
    {
        if (isDefeated)
            yield break;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(
                "[RabbitAnimationController] Main Camera를 찾을 수 없습니다.");

            yield break;
        }

        animator.SetTrigger(RunHash);

        while (true)
        {
            // 화면 밖인지 판단
            //x = 0        화면 왼쪽
            //x = 0.5      화면 중앙
            //x = 1        화면 오른쪽
            Vector3 viewportPosition =
                mainCamera.WorldToViewportPoint(transform.position);

            if (viewportPosition.x >= exitViewportX)
            {
                break;
            }

            transform.position +=
                Vector3.right * runSpeed * Time.deltaTime;

            yield return null;
        }

        Debug.Log(
            "[RabbitAnimationController] 화면 밖으로 이동 완료");
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