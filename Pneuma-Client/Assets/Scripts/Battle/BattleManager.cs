using System;
using UnityEngine;
using Battle;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }
    public BattleState CurrentState { get; private set; } = BattleState.None;
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

    // 상태 전환 메서드
    public void ChangeState(BattleState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        Debug.Log($"Battle State Changed : {CurrentState}");

        OnBattleStateChanged?.Invoke(CurrentState);
    }

    public void CheckBattleResult()
    {
        // 매개변수를 통해 승패를 판단하려고 했지만
        // 호출할 때마다 계속 전달해야하므로 
        // BattleManager에서 Player와 Enemy의 체력을 직접 참조하여 승패를 판단하도록 변경

        // TODO: Player와 EnemyManager가 추가된 후 아래 코드 활성화
        
        // if(Player.Instance.CurrentHp <= 0)
        // {
        //     ChangeState(BattleState.Defeat);
        //     return;
        // }

        // if(EnemyManager.Instance.IsAllDead())
        // {
        //     ChangeState(BattleState.Victory);
        // }
    }
}