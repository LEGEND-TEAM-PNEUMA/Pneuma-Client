using Battle;
using Pneuma.Unit;
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
    
    [Header("Owner")]
    [SerializeField] private Player player;

    [Header("Debug")]
    [SerializeField] private bool enableKeyboardTest = false;

    private bool isDefeated;
    private BattleManager battleManager;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (player == null)
            player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        if (player == null)
            return;

        player.OnDamaged += HandleDamaged;
        player.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (player == null)
            return;

        player.OnDamaged -= HandleDamaged;
        player.OnDeath -= HandleDeath;
    }

    private void Start()
    {
        // Start는 모든 Awake 이후에 실행되므로 BattleManager.Instance가 준비되어 있다.
        battleManager = BattleManager.Instance;

        if (battleManager == null)
        {
            Debug.LogWarning("[LeilaAnimationController] BattleManager를 찾지 못해 Victory 모션을 연결하지 않습니다.");
            return;
        }

        battleManager.OnBattleStateChanged += HandleBattleStateChanged;
    }

    private void OnDestroy()
    {
        if (battleManager != null)
            battleManager.OnBattleStateChanged -= HandleBattleStateChanged;
    }

    private void Update()
    {
        if (!enableKeyboardTest)
            return;

        if (Input.GetKeyDown(KeyCode.B))      PlayBuff();
        else if (Input.GetKeyDown(KeyCode.A)) PlayAttack();
        else if (Input.GetKeyDown(KeyCode.H)) PlayHit();
        else if (Input.GetKeyDown(KeyCode.D)) PlayDefeat();
        else if (Input.GetKeyDown(KeyCode.V)) PlayVictory();
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

    private void HandleDamaged(CharacterBase damagedUnit)
    {
        // 이번 피해로 쓰러졌다면 Hit 대신 HandleDeath의 Defeat만 재생한다.
        if (player.IsDead)
            return;

        PlayHit();
    }

    private void HandleDeath(CharacterBase deadUnit)
    {
        PlayDefeat();
    }

    private void HandleBattleStateChanged(BattleState state)
    {
        if (state == BattleState.Victory)
            PlayVictory();
    }
}