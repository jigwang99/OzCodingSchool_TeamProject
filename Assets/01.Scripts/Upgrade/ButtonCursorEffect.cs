using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonCursorEffect : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler
{
    [SerializeField] private Image cursorImage;

    private Tween blinkTween;

    private void Awake()
    {
        cursorImage.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cursorImage.gameObject.SetActive(true);

        blinkTween?.Kill();

        Color color = cursorImage.color;
        color.a = 0f;
        cursorImage.color = color;

        blinkTween = cursorImage
            .DOFade(0.5f, 0.3f)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        blinkTween?.Kill();
        blinkTween = null;

        cursorImage.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        blinkTween?.Kill();
        blinkTween = null;

        cursorImage.DOFade(1f, 0.1f);
    }
}