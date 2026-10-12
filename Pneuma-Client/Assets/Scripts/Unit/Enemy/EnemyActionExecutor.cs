using System.Collections;
using Pneuma.Unit;
using UnityEngine;
//이미 선택된 행동의 실제 효과 실행
public abstract class EnemyActionExecutor : MonoBehaviour
{
    public abstract IEnumerator Execute(
        Enemy source,
        Player target,
        EnemyActionData actionData);

    /// <summary>
    /// 예견 UI에 표시할 예상 피해량을 반환한다. 피해가 없는 행동이면 0.
    /// 몬스터마다 피해 계산식이 다르므로 각 Executor가 재정의한다.
    /// </summary>
    public virtual int GetExpectedDamage(EnemyActionData actionData) => 0;
}