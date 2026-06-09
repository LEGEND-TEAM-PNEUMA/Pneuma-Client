using System;
using UnityEngine;
using Battle;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    public BattleState CurrentState { get; private set; } = BattleState.None;
    public int CurrentTurn { get; private set; } = 0;

    // TODO: Player, Enemy 구현 후 참조 연결
    //public Player CurrentPlayer { get; private set; }
    //public List<Enemy> Enemies { get; private set; }

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
    }

    // TODO: Player, Enemy 구현 후 참조 연결
    // private void Initialize(Player player, List<Enemy> enemies)
    // {
    //     CurrentPlayer = player;
    //     Enemies = enemies;
    // }

    // 상태 전환 메서드
    public void ChangeState(BattleState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        Debug.Log($"[BattleManager] State Changed : {CurrentState}");

        OnBattleStateChanged?.Invoke(CurrentState);
    }

    public void IncreaseTurn()
    {
        // 플레이어 턴에 턴 수 증가
        CurrentTurn++;
    }

    public void CheckBattleResult()
    {
        // 매개변수를 통해 승패를 판단하려고 했지만
        // 호출할 때마다 계속 전달해야하므로 
        // BattleManager에서 Player와 Enemy의 체력을 직접 참조하여 승패를 판단하도록 변경

        // TODO: Player, Enemy 구현 후 참조 연결
        // if(CurrentPlayer.CurrentHp <= 0)
        // {
        //     ChangeState(BattleState.Defeat);
        //     return;
        // }
        // if(Enemies.All(enemy => enemy.IsDead))
        // {
        //     ChangeState(BattleState.Victory);
        // }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}