using UnityEngine;

public class LeilaAnimationController : MonoBehaviour
{
    // 트리거 이름을 한 곳에서 해시로 관리
    private static readonly int AttackHash  = Animator.StringToHash("Attack");
    private static readonly int BuffHash    = Animator.StringToHash("Buff");
    private static readonly int HitHash     = Animator.StringToHash("Hit");
    private static readonly int DefeatHash  = Animator.StringToHash("Defeat");
    private static readonly int VictoryHash = Animator.StringToHash("Victory");

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Debug")]
    [SerializeField] private bool enableKeyboardTest = false;

    private bool isDefeated;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!enableKeyboardTest)
            return;

        // 버프
        if (Input.GetKeyDown(KeyCode.B))
            PlayAnimation("Buff");

        // 공격
        else if (Input.GetKeyDown(KeyCode.A))
            PlayAnimation("Attack");

        // 피격
        else if (Input.GetKeyDown(KeyCode.H))
            PlayAnimation("Hit");

        // 패배
        else if (Input.GetKeyDown(KeyCode.D))
            PlayAnimation("Defeat");

        // 승리
        else if (Input.GetKeyDown(KeyCode.V))
            PlayAnimation("Victory");
    }

    private void PlayAnimation(string triggerName)
    {
        animator.SetTrigger(triggerName);

        Debug.Log($"[LeilaAnimationController] {triggerName}");
    }

    [ContextMenu("Test/Attack")]
    public void PlayAttack() => Play(AttackHash, "Attack");

    [ContextMenu("Test/Buff")]
    public void PlayBuff() => Play(BuffHash, "Buff");

    [ContextMenu("Test/Hit")]
    public void PlayHit() => Play(HitHash, "Hit");

    [ContextMenu("Test/Victory")]
    public void PlayVictory() => Play(VictoryHash, "Victory");

    [ContextMenu("Test/Defeat")]
    public void PlayDefeat()
    {
        if (isDefeated || animator == null)
            return;

        isDefeated = true;

        // 이미 걸려 있던 트리거가 Defeat 이후에 발동하지 않도록 비운다.
        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(BuffHash);
        animator.ResetTrigger(HitHash);

        animator.SetTrigger(DefeatHash);
        Debug.Log("[LeilaAnimationController] Defeat");
    }

    private void Play(int triggerHash, string label)
    {
        if (isDefeated || animator == null)
            return;

        animator.SetTrigger(triggerHash);
        Debug.Log($"[LeilaAnimationController] {label}");
    }
}