using System;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Pneuma.Unit;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Battle Units")]
    [SerializeField] private Player currentPlayer;
    [SerializeField] private List<Enemy> enemies = new(); // enemy 리스트 연결

    public Player CurrentPlayer => currentPlayer;
    public IReadOnlyList<Enemy> Enemies => enemies;

    public BattleState CurrentState { get; private set; } = BattleState.None;
    public int CurrentTurn { get; private set; } = 0;

    public event Action<BattleState> OnBattleStateChanged;

    private bool isBattleStarted; // 전투 시작 여부를 추적하는 플래그
    private Coroutine enemyTurnCoroutine;

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
        int firstTurn = CurrentTurn + 1;
        PredictEnemyActions(firstTurn);

        // 배틀 인트로 UI, 배경 음악 재생 등 초기화 작업 수행
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

        // TODO: 덱 매니저 연결 후 매 턴 드로우 처리
        // deckManager.DrawForTurn();
    }

    /// 플레이어의 턴 종료 요청을 받아 EnemyTurn으로 전환한다.
    public void EndPlayerTurn()
    {
        if (!isBattleStarted)
        {
            Debug.LogWarning(
                "[BattleManager] 전투가 시작되지 않아 PlayerTurn을 종료할 수 없습니다.");

            return;
        }

        if (CurrentState != BattleState.PlayerTurn)
        {
            Debug.LogWarning(
                $"[BattleManager] PlayerTurn에서만 턴을 종료할 수 있습니다. " +
                $"CurrentState: {CurrentState}");

            return;
        }

        ChangeState(BattleState.EnemyTurn);
    }

    private void EnterEnemyTurn() // 적 턴 시작 : 행동 수행 후 다음 행동 큐에 넣고 턴 종료
    {
        if (enemyTurnCoroutine != null)
        {
            Debug.LogWarning(
                "[BattleManager] EnemyTurn이 이미 실행 중입니다.");

            return;
        }

        enemyTurnCoroutine =
            StartCoroutine(ExecuteEnemyTurnRoutine());
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

    /// 살아 있는 모든 적이 지정된 턴에 실행할 행동을 예견한다.
    private void PredictEnemyActions(int targetTurn)
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || enemy.IsDead)
                continue;

            enemy.PredictAction(targetTurn);
        }
    }

    /// 현재 살아 있는 적들이 미리 예견한 행동을 순서대로 실행한다.
    /// 모든 행동이 끝나면 다음 턴 행동을 예견하고 PlayerTurn으로 복귀한다.
    private IEnumerator ExecuteEnemyTurnRoutine()
    {
        Debug.Log(
            $"[BattleManager] Enemy Turn Started. Turn: {CurrentTurn}");

        List<Enemy> actingEnemies =
            new List<Enemy>(enemies);

        foreach (Enemy enemy in actingEnemies)
        {
            // 행동 도중 Victory 또는 Defeat 등으로 상태가 바뀌면 중단한다.
            if (CurrentState != BattleState.EnemyTurn)
            {
                enemyTurnCoroutine = null;
                yield break;
            }

            if (enemy == null || enemy.IsDead)
                continue;

            if (currentPlayer == null || currentPlayer.IsDead)
                break;

            yield return enemy.ExecutePredictedAction(
                CurrentTurn,
                currentPlayer);
        }

        enemyTurnCoroutine = null;

        // 마지막 행동 실행 도중 Victory/Defeat로 전환됐다면
        // 다음 턴 예견과 PlayerTurn 복귀를 진행하지 않는다.
        if (CurrentState != BattleState.EnemyTurn)
        {
            Debug.Log(
                $"[BattleManager] Enemy Turn 종료 시점에 " +
                $"{CurrentState} 상태라 턴 전환을 중단합니다.");

            yield break;
        }

        Debug.Log(
            $"[BattleManager] Enemy Turn Actions Completed. " +
            $"Turn: {CurrentTurn}");

        // 다음 PlayerTurn에서 보여줄 적 행동을 먼저 결정한다.
        int nextTurn = CurrentTurn + 1;
        PredictEnemyActions(nextTurn);

        ChangeState(BattleState.PlayerTurn);
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