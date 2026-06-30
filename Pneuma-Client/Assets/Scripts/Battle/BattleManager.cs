using System;
using UnityEngine;
using Battle;
using Pneuma.Unit;

//[Summary]
// 해당 Manager가 호출되면 게임 시작 인트로 진행 후 플레이어턴 진입까지 자동 진행됨
// BattleState 변경 지시를 내림
// 플레이어 승리 / 패배를 통해 BattleState 변경
//[Summary]

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle Units")]
    [SerializeField] private Player currentPlayer;

    public Player CurrentPlayer => currentPlayer;
 
    public BattleState CurrentState { get; private set; } = BattleState.None;
    public int CurrentTurn { get; private set; } = 0;

    public event Action<BattleState> OnBattleStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ChangeState(BattleState.Start); 
        ChangeState(BattleState.PlayerTurn);
    }

    // 상태 전환 메서드
    public void ChangeState(BattleState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        Debug.Log($"[BattleManager] State Changed : {CurrentState}");

        EnterState(CurrentState);

        OnBattleStateChanged?.Invoke(CurrentState);
    }

    private void EnterState(BattleState state)
    {
        switch (state)
        {
            case BattleState.Start:
                EnterBattleStart();
                break;

            case BattleState.PlayerTurn:
                EnterPlayerTurn();
                break;

            case BattleState.EnemyTurn:
                EnterEnemyTurn();
                break;

            case BattleState.Victory:
                EnterVictory();
                break;

            case BattleState.Defeat:
                EnterDefeat();
                break;
        }
    }

    private void EnterBattleStart()
    {
        Debug.Log("[BattleManager] Battle Start");
    }

    private void EnterPlayerTurn()
    {
        if (currentPlayer == null)
        {
            Debug.LogError("[BattleManager] CurrentPlayer가 연결되지 않았습니다.");
            return;
        }
        
        IncreaseTurn();

        currentPlayer.OnPlayerTurnStarted(); // Energy 회복 및 Shield 초기화

        Debug.Log($"[BattleManager] Player Turn Started. Turn: {CurrentTurn}");
    }

    private void EnterEnemyTurn()
    {
        // Enermy와 연결되어 있지 않으면 return
        Debug.Log("[BattleManager] Enemy Turn Started");
        // Enermy 턴 시작 시 루틴 진행
    }

    private void EnterVictory()
    {
        Debug.Log("[BattleManager] Victory");
    }

    private void EnterDefeat()
    {
        Debug.Log("[BattleManager] Defeat");
    }

    public void IncreaseTurn()
    {
        // 플레이어 턴에 턴 수 증가
        CurrentTurn++;
    }

    public void OnPlayerDead()
    {
        ChangeState(BattleState.Defeat);
    }

    public void OnEnemyDead()
    {
        // 모든 적 or 특정 적 사망 시 Victory
        ChangeState(BattleState.Victory);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}