using System.Collections.Generic;
using UnityEngine;

namespace Pneuma.UI.Card
{
    // 손패 카드들을 부채꼴로 배치합니다.
    // 위치를 직접 쓰지 않고 각 카드의 UICardDrag에 제자리(home)만 알려줍니다.
    // 실제 이동·회전은 카드가 스스로 수행하므로 드래그와 위치 충돌이 없습니다.
    public class CardHandLayout : MonoBehaviour
    {
        [Header("배치")]
        // 카드 간 가로 간격.
        // 호에서 유도하지 않고 따로 두어야 각도와 무관하게 간격만 조절할 수 있다.
        [SerializeField] private float spacing = 90f;

        // 카드 한 장당 벌어지는 각도(도).
        [SerializeField] private float cardAngle = 5f;

        // 세로 처짐을 계산할 호의 반지름. 클수록 완만해집니다.
        [SerializeField] private float arcRadius = 1000f;

        // 부채꼴 전체를 평행 이동시킵니다.
        [SerializeField] private Vector2 offset = Vector2.zero;

        [Header("호버 시 이웃 카드 벌어짐")]
        [Tooltip("호버된 카드 바로 옆 카드가 바깥쪽으로 밀려나는 거리(캔버스 단위)")]
        [SerializeField] private float hoverSpread = 60f;

        [Tooltip("한 칸 멀어질 때마다 밀려나는 거리에 곱하는 비율. 1이면 모두 같은 거리, 작을수록 먼 카드는 덜 움직입니다.")]
        [SerializeField, Range(0f, 1f)] private float hoverSpreadFalloff = 0.6f;

        [Header("기즈모")]
        [SerializeField] private bool drawGizmos = true;

        // 플레이 중이 아닐 때 미리 그려볼 카드 수.
        [SerializeField] private int previewCount = 5;

        // 슬롯마다 그릴 카드 방향선의 길이.
        [SerializeField] private float slotLineLength = 200f;

        // 카드 자리를 표시하는 구의 반지름.
        [SerializeField] private float slotMarkerSize = 4f;

        // 마지막으로 정렬한 손패. CardHandView의 리스트를 그대로 참조하므로 항상 최신 상태다.
        // 인스펙터에서 값을 바꿨을 때 다시 배치하기 위해 들고 있는다.
        private IReadOnlyList<UICardData> currentCards;

        // 지금 마우스가 올라가 있는 카드. 없으면 null.
        private UICardData hoveredCard;

        /// <summary>
        /// 마우스가 올라간 카드를 지정하고 이웃 카드를 옆으로 벌립니다.
        /// 실제 이동은 각 카드의 UICardDrag가 제자리(home)를 부드럽게 따라가며 처리합니다.
        /// </summary>
        public void SetHoveredCard(UICardData card)
        {
            if (hoveredCard == card) return;

            hoveredCard = card;
            ApplyLayout();
        }

        /// <summary>
        /// 지정한 카드가 지금 호버된 카드일 때만 호버를 풀고 원래 간격으로 되돌립니다.
        /// 다른 카드로 바로 옮겨 간 경우(새 카드의 Set이 먼저 온 경우) 그 상태를 지우지 않기 위함입니다.
        /// </summary>
        public void ClearHoveredCard(UICardData card)
        {
            if (hoveredCard != card) return;

            hoveredCard = null;
            ApplyLayout();
        }

        /// <summary>
        /// 손패 순서대로 부채꼴 위치를 계산해 각 카드에 지정합니다.
        /// </summary>
        /// <param name="cards">손패 순서대로 정렬된 카드 목록</param>
        public void UpdateLayout(IReadOnlyList<UICardData> cards)
        {
            currentCards = cards;
            ApplyLayout();
        }

#if UNITY_EDITOR
        // 플레이 중 인스펙터에서 배치 값을 조절하면 즉시 반영한다.
        // 이 호출이 없으면 카드가 생기거나 사라질 때까지 변경이 보이지 않는다.
        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            ApplyLayout();
        }
#endif

        private void ApplyLayout()
        {
            IReadOnlyList<UICardData> cards = currentCards;
            if (cards == null || cards.Count == 0) return;

            int cardCount = cards.Count;

            // 중앙 인덱스. 짝수 장이면 두 카드 사이(x.5)가 중앙이 된다.
            float mid = (cardCount - 1) / 2f;

            // 호버된 카드의 손패 순서. 없으면 -1이라 아무 카드도 밀리지 않는다.
            int hoveredIndex = -1;
            for (int i = 0; i < cardCount; i++)
            {
                if (cards[i] == hoveredCard)
                {
                    hoveredIndex = i;
                    break;
                }
            }

            for (int i = 0; i < cardCount; i++)
            {
                UICardData card = cards[i];
                if (card == null) continue;

                // 손패 순서를 카드에 알려준다. 호버가 올렸던 렌더 순서를 되돌릴 때 기준이 된다.
                card.SetHandIndex(i);

                // 렌더 순서도 손패 순서와 맞춰 오른쪽 카드가 위로 겹치게 한다.
                card.transform.SetSiblingIndex(i);

                if (card.CardDrag == null) continue;

                GetSlot(i - mid, out Vector2 position, out float rotation);
                position.x += GetHoverSpread(i, hoveredIndex);
                card.CardDrag.SetHome(position, rotation);
            }

            // 위 루프가 렌더 순서를 손패 순서로 되돌렸으므로, 호버된 카드를 다시 맨 앞으로 올린다.
            if (hoveredIndex >= 0) cards[hoveredIndex].transform.SetAsLastSibling();
        }

        /// <summary>
        /// 호버된 카드 기준으로 i번째 카드가 밀려날 가로 거리를 구합니다.
        /// 왼쪽 카드는 왼쪽(-), 오른쪽 카드는 오른쪽(+)으로, 멀수록 falloff만큼 줄어듭니다.
        /// </summary>
        private float GetHoverSpread(int index, int hoveredIndex)
        {
            if (hoveredIndex < 0 || index == hoveredIndex) return 0f;

            int distance = Mathf.Abs(index - hoveredIndex);
            float amount = hoverSpread * Mathf.Pow(hoverSpreadFalloff, distance - 1);

            return index < hoveredIndex ? -amount : amount;
        }

        /// <summary>
        /// 중앙 기준 거리 t에 해당하는 카드 자리를 구합니다.
        /// 배치와 기즈모가 같은 식을 쓰도록 여기 한 곳에만 둡니다.
        /// </summary>
        /// <param name="t">중앙 기준 거리. 부호가 좌우, 크기가 중앙에서 떨어진 정도</param>
        /// <param name="position">부모 기준 위치(anchoredPosition)</param>
        /// <param name="rotation">카드가 기울 각도(Z)</param>
        private void GetSlot(float t, out Vector2 position, out float rotation)
        {
            // 세로 처짐과 회전은 같은 각도에서 유도해 서로 어긋나지 않게 한다.
            float angle = t * cardAngle;
            float rad = angle * Mathf.Deg2Rad;

            // 가로는 간격으로, 세로는 호로 결정한다.
            // 중앙(angle 0)이 가장 높고, 바깥으로 갈수록 자연히 내려간다.
            float x = t * spacing + offset.x;
            float y = arcRadius * (Mathf.Cos(rad) - 1f) + offset.y;

            position = new Vector2(x, y);

            // 호의 접선 방향. 왼쪽 카드는 왼쪽으로, 오른쪽 카드는 오른쪽으로 기운다.
            rotation = -angle;
        }

#if UNITY_EDITOR
        // 호 궤도와 각 카드 자리를 씬 뷰에 그립니다.
        // 플레이 중에는 실제 손패 수를, 아닐 때는 previewCount를 기준으로 그립니다.
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;

            int count = (currentCards != null && currentCards.Count > 0)
                ? currentCards.Count
                : previewCount;

            if (count <= 0) return;

            float mid = (count - 1) / 2f;

            // 1) 호 궤도 — 카드 사이도 이어지도록 촘촘히 샘플링해 곡선으로 잇는다.
            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.9f);

            const int segments = 64;
            Vector3 previous = SlotToWorld(-mid);

            for (int i = 1; i <= segments; i++)
            {
                float t = Mathf.Lerp(-mid, mid, i / (float)segments);
                Vector3 current = SlotToWorld(t);

                Gizmos.DrawLine(previous, current);
                previous = current;
            }

            // 2) 카드 자리 — 위치와 기울기를 함께 표시한다.
            for (int i = 0; i < count; i++)
            {
                float t = i - mid;

                GetSlot(t, out Vector2 position, out float rotation);
                Vector3 world = transform.TransformPoint(position);

                // 카드가 기운 방향(위쪽)을 선으로 그려 회전을 눈으로 확인한다.
                Vector3 up = transform.TransformDirection(
                    Quaternion.Euler(0f, 0f, rotation) * Vector3.up);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(world, world + up * slotLineLength);

                // 중앙 카드만 색을 달리해 기준점을 알아보기 쉽게 한다.
                Gizmos.color = Mathf.Approximately(t, 0f) ? Color.red : Color.yellow;
                Gizmos.DrawSphere(world, slotMarkerSize);
            }
        }

        // 기즈모용. GetSlot 결과를 월드 좌표로 바꾼다.
        private Vector3 SlotToWorld(float t)
        {
            GetSlot(t, out Vector2 position, out _);

            return transform.TransformPoint(position);
        }
#endif
    }

}
