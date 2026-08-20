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
}