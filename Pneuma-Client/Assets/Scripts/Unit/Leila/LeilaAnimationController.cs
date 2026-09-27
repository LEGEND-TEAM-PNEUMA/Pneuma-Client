using UnityEngine;

public class LeilaAnimationController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Debug")]
    [SerializeField] private bool enableKeyboardTest = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!enableKeyboardTest || animator == null)
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
}