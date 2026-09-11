using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
    [SerializeField] private Vector3 pressScale = new Vector3(0.92f, 0.92f, 1f);
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private Ease easeType = Ease.OutBack;

    private Vector3 defaultScale;
    private bool isHovered = false;

    private void Awake()
    {
        defaultScale = transform.localScale;
    }

    // 마우스가 버튼 위에 올라갔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        AnimateScale(hoverScale, duration, easeType);
    }

    // 마우스가 버튼 밖으로 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        AnimateScale(defaultScale, duration, Ease.OutQuad);
    }

    // 버튼을 눌렀을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateScale(pressScale, duration * 0.5f, Ease.OutQuad);
    }

    // 버튼에서 손을 땠을 때
    public void OnPointerUp(PointerEventData eventData)
    {
        Vector3 targetScale = isHovered ? hoverScale : defaultScale;
        AnimateScale(targetScale, duration, easeType);
    }

    private void AnimateScale(Vector3 targetScale, float animDuration, Ease ease)
    {
        transform.DOKill();
        transform.DOScale(targetScale, animDuration)
                 .SetEase(ease)
                 .SetUpdate(true);
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = defaultScale;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
