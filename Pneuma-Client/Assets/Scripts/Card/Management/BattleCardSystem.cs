using UnityEngine;

namespace Pneuma.Card.Management
{
    /// <summary>
    /// BattleManager가 카드 시스템에 요청하는 기능만 모은 추상 클래스입니다.
    /// BattleManager는 이 타입만 알고, 실제 구현은 씬에서 갈아끼웁니다.
    ///  - BattleCardController   : 버튼 클릭형 (CardMotionTestScene)
    ///  - UIBattleCardController : 드래그 앤 드롭형 (UITestScene)
    ///
    /// 인터페이스가 아닌 추상 MonoBehaviour인 이유:
    /// Unity 인스펙터는 인터페이스 타입 필드를 직렬화하지 못해 드래그로 연결할 수 없습니다.
    /// </summary>
    public abstract class BattleCardSystem : MonoBehaviour
    {
        /// <summary>
        /// 카드 연출·효과를 처리하는 중인지 여부입니다. 처리 중에는 턴 종료가 막힙니다.
        /// </summary>
        public abstract bool IsResolvingCard { get; }

        /// <summary>
        /// 전투 시작 시 덱을 구성합니다. (BattleState.Start)
        /// </summary>
        public abstract void InitializeBattleDeck();

        /// <summary>
        /// 플레이어 턴 시작 시 카드를 뽑습니다. (BattleState.PlayerTurn)
        /// </summary>
        public abstract void DrawForPlayerTurn();

        /// <summary>
        /// 턴 종료 시 손패를 모두 버립니다.
        /// </summary>
        public abstract void DiscardHand();
    }
}
