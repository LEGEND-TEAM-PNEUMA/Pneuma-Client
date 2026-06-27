using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform card;

    [Header("Animation Settings")]
    [SerializeField] private float appearAnimationDuration = 0.5f;
    [SerializeField] private float disappearAnimationDuration = 0.5f;
    [SerializeField] private Ease startAnimationEase = Ease.OutQuad;
    [SerializeField] private float clickAnimationDuration = 0.5f;

    [Header("Hover Animation")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverYOffset = 40f;
    [SerializeField] private float hoverAnimationDuration = 0.2f;
    [SerializeField] private Ease hoverEase = Ease.OutQuad;

    [Header("Select Animation")]
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float selectedYOffset = 80f;
    [SerializeField] private float selectAnimationDuration = 0.2f;
    [SerializeField] private Ease selectEase = Ease.OutBack;

    [Header("Events")]
    public UnityEvent onClick;
    public UnityEvent onDeselect;

    // 카드가 클릭 가능한 상태인지 여부
    // 애니메이션 등장 시 클릭되지 않도록 하기 위해 사용
    private bool clickable = false;
    private int currentIndex = -1;
    private bool isSelected = false;
    private bool isHovered = false;
    // 사라지는 애니메이션이 시작된 카드. 정렬 계산에서 제외하기 위해 사용
    private bool isRemoving = false;
    // CardLayout이 부채꼴 정렬로 지정한 Z 회전값. 호버/선택 해제 시 이 각도로 복귀한다.
    private float baseRotation = 0f;

    public int Index => currentIndex;
    public bool IsSelected => isSelected;
    public bool IsRemoving => isRemoving;

    private void Start()
    {
        // 시작 애니메이션
        card.anchoredPosition = new Vector2(0, -Screen.height);

        // 등장 애니메이션이 끝난 뒤에야 클릭을 허용한다.
        card.DOAnchorPosY(0f, appearAnimationDuration)
            .SetEase(startAnimationEase)
            .OnComplete(() => clickable = true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // TODO: 카드 클릭 시 애니메이션 추가 및 이벤트 호출
        // 이떄 이벤트는 해당 카드를 선택 관리자에 할당하는 역할을 해야 함.

        if (!clickable) return;
        onClick?.Invoke();
    }

    public void OnDeselect()
    {
        if (!isSelected) return;

        isSelected = false;
        // 여전히 호버 중이면 호버 상태를, 아니면 평상시 상태로 복귀한다.
        if (isHovered)
        {
            // 호버 중이므로 맨 앞 유지
            AnimateScale(hoverScale, selectAnimationDuration, selectEase);
            AnimateRotation(0f, selectAnimationDuration, selectEase);
            AnimateRise(hoverYOffset, selectAnimationDuration, selectEase);
        }
        else
        {
            // 원래 렌더(형제) 순서로 복귀
            transform.SetSiblingIndex(currentIndex);
            AnimateScale(1f, selectAnimationDuration, selectEase);
            AnimateRotation(baseRotation, selectAnimationDuration, selectEase);
            AnimateRise(0f, selectAnimationDuration, selectEase);
        }
        onDeselect?.Invoke();
    }

    public void OnSelected()
    {
        if (isSelected) return;

        isSelected = true;
        // 다른 카드 위로 그려지도록 맨 앞(마지막 형제)으로 이동
        transform.SetAsLastSibling();
        // 확대 + 정면(0도) + 위로 올려 카드가 다 보이도록 한다.
        AnimateScale(selectedScale, selectAnimationDuration, selectEase);
        AnimateRotation(0f, selectAnimationDuration, selectEase);
        AnimateRise(selectedYOffset, selectAnimationDuration, selectEase);
    }

    // CardLayout이 정한 부채꼴 회전값을 적용한다.
    // 호버/선택 중에는 정면(0도)을 유지해야 하므로 값만 저장하고 적용은 미룬다.
    public void SetLayoutRotation(float zRotation, float duration)
    {
        baseRotation = zRotation;
        if (isHovered || isSelected) return;
        transform.DORotateQuaternion(Quaternion.Euler(0, 0, zRotation), duration);
    }

    private void AnimateScale(float scale, float duration, Ease ease)
        => card.DOScale(Vector3.one * scale, duration).SetEase(ease);

    private void AnimateRotation(float zRotation, float duration, Ease ease)
        => transform.DORotateQuaternion(Quaternion.Euler(0, 0, zRotation), duration).SetEase(ease);

    // 카드를 위로 올려(혹은 제자리로 내려) 다른 카드에 가리지 않고 다 보이게 한다.
    private void AnimateRise(float yOffset, float duration, Ease ease)
        => card.DOAnchorPosY(yOffset, duration).SetEase(ease);

    public void RemoveCard(System.Action onComplete = null)
    {
        if (isRemoving) return;

        // 즉시 정렬 대상에서 제외 + 클릭 차단
        isRemoving = true;
        clickable = false;

        card.DOAnchorPosY(-Screen.height, disappearAnimationDuration)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                Destroy(gameObject);
            });
    }

    public void SetIndex(int idx) => currentIndex = idx;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!clickable || isHovered) return;
        isHovered = true;

        // 선택된 카드는 이미 확대·정면·상승 상태이므로 그대로 둔다.
        if (isSelected) return;

        // 다른 카드 위로 그려지도록 맨 앞(마지막 형제)으로 이동
        transform.SetAsLastSibling();
        // 호버 시 살짝 확대 + 정면(0도) + 위로 올려 다 보이게 한다.
        AnimateScale(hoverScale, hoverAnimationDuration, hoverEase);
        AnimateRotation(0f, hoverAnimationDuration, hoverEase);
        AnimateRise(hoverYOffset, hoverAnimationDuration, hoverEase);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovered) return;
        isHovered = false;

        // 선택된 카드는 선택 상태를 유지한다.
        if (isSelected) return;

        // 원래 렌더(형제) 순서로 복귀
        transform.SetSiblingIndex(currentIndex);
        // 평상시 크기·부채꼴 각도·제자리로 복귀한다.
        AnimateScale(1f, hoverAnimationDuration, hoverEase);
        AnimateRotation(baseRotation, hoverAnimationDuration, hoverEase);
        AnimateRise(0f, hoverAnimationDuration, hoverEase);
    }
}
