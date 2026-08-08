using System;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Pneuma.Card.Management;
using Pneuma.Unit;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle Units")]
    [SerializeField] private Player currentPlayer;
    [SerializeField] private List<Enemy> enemies = new(); // enemy 리스트 연결

    [Header("Card")]
    [SerializeField] private BattleCardController battleCardController;

    public Player CurrentPlayer => currentPlayer;
    public IReadOnlyList<Enemy> Enemies => enemies;

    public BattleState CurrentState { get; private set; } = BattleState.None;
    public int CurrentTurn { get; private set; } = 0;

    public event Action<BattleState> OnBattleStateChanged;

    private bool isBattleStarted; // 전투 시작 여부를 추적하는 플래그

    private void Awake() // 싱글톤 패턴
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start() // 초기화 및 전투 시작
    {
        InitializeSerializedUnits();
        StartBattle();
    }

    /// <summary>
    /// 현재 단계용 초기화.
    /// Hierarchy에 배치되어 Inspector에 연결된 Player와 Enemy를 등록한다.
    /// </summary>
    private void InitializeSerializedUnits()
    {
        if (currentPlayer != null)
        {
            currentPlayer.OnDeath += HandlePlayerDeath;
        }

        List<Enemy> initialEnemies = new List<Enemy>(enemies);
        enemies.Clear();

        foreach (Enemy enemy in initialEnemies)
        {
            RegisterEnemy(enemy);
        }
    }

    public void StartBattle() // 전투 시작 (처음 1회)
    {
        if (isBattleStarted)
        {
            Debug.LogWarning("[BattleManager] 이미 전투가 시작되었습니다.");
            return;
        }

        isBattleStarted = true;

        ChangeState(BattleState.Start);
        ChangeState(BattleState.PlayerTurn);
    }

    public void RegisterEnemy(Enemy enemy) // Enemy 등록
    {
        // Enemy 등록 : 리스트에 추가, OnDeath 이벤트 구독
        if (enemy == null)
        {
            Debug.LogError("[BattleManager] 등록하려는 Enemy가 null입니다.");
            return;
        }

        if (enemies.Contains(enemy))
        {
            Debug.LogWarning($"[BattleManager] 이미 등록된 Enemy입니다: {enemy.name}");
            return;
        }

        enemies.Add(enemy);
        enemy.OnDeath += HandleEnemyDeath; // Enemy의 OnDeath 이벤트 구독 : 죽었을 때 BattleManager가 처리하도록 연결

        Debug.Log($"[BattleManager] Enemy Registered: {enemy.name}");
    }

    private void UnregisterEnemy(Enemy enemy) // Enemy 등록 해제
    {
        if (enemy == null)
            return;

        enemy.OnDeath -= HandleEnemyDeath;
        enemies.Remove(enemy);

        Debug.Log($"[BattleManager] Enemy Unregistered: {enemy.name}");
    }

    private void UnsubscribeUnitEvents() // 유닛 이벤트 구독 해제
    {
        if (currentPlayer != null)
        {
            currentPlayer.OnDeath -= HandlePlayerDeath;
        }

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null)
                continue;

            enemy.OnDeath -= HandleEnemyDeath;
        }
    }

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

    private void EnterBattleStart() // 전투 시작 : 인트로, 덱 초기화
    {
        Debug.Log("[BattleManager] Battle Start");
        // 배틀 인트로 UI, 배경 음악 재생 등 초기화 작업 수행

        if (battleCardController != null)
        {
            battleCardController.InitializeBattleDeck();
        }
    }

    private void EnterPlayerTurn() // 플레이어 턴 시작 : 턴 횟수 증가, 드로우
    {
        if (currentPlayer == null)
        {
            Debug.LogError("[BattleManager] CurrentPlayer가 연결되지 않았습니다.");
            return;
        }

        IncreaseTurn();

        currentPlayer.OnPlayerTurnStarted();

        Debug.Log($"[BattleManager] Player Turn Started. Turn: {CurrentTurn}");

        if (battleCardController != null)
        {
            battleCardController.DrawForPlayerTurn();
        }
    }

    private void EnterEnemyTurn() // 적 턴 시작 : 행동 수행 후 다음 행동 큐에 넣고 턴 종료
    {
        Debug.Log("[BattleManager] Enemy Turn Started");

        // TODO: EnemyTurnController 또는 EnemyActionController 연결
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
        CurrentTurn++;
    }

    private void HandlePlayerDeath(CharacterBase deadUnit)
    {
        ChangeState(BattleState.Defeat);
    }

    private void HandleEnemyDeath(CharacterBase deadUnit)
    {
        Enemy deadEnemy = deadUnit as Enemy;

        if (deadEnemy == null)
            return;

        Debug.Log($"[BattleManager] Enemy Dead: {deadEnemy.name}");

        UnregisterEnemy(deadEnemy); // Enemy 등록 해제

        if (AreAllEnemiesDead()) // 모든 적 죽었다면 Victory 상태로 전환
        {
            ChangeState(BattleState.Victory);
        }
    }

    private bool AreAllEnemiesDead()
    {
        return enemies.Count == 0;
    }

    private void Update() // 디버그용 : 킬 코드 K 키를 눌러 첫 번째 적에게 999 데미지
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (enemies.Count > 0)
            {
                enemies[0].TakeDamage(999);
            }
        }
    }   

    private void OnDestroy()
    {
        UnsubscribeUnitEvents();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
