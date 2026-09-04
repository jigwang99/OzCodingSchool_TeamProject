using System.Collections;
using UnityEngine;

public class HamburgerMenuUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private RectTransform menuRect;

    [Header("애니메이션 설정")]
    [SerializeField] private float duration = 0.15f;

    private CanvasGroup menuCanvasGroup;
    private bool isOpen = false;
    private Coroutine menuCoroutine;

    private void Awake()
    {
        if (menuRect != null)
        {
            menuRect.pivot = new Vector2(0.5f, 1f);

            if (!menuRect.TryGetComponent(out menuCanvasGroup))
            {
                menuCanvasGroup = menuRect.gameObject.AddComponent<CanvasGroup>();
            }
        }

        SetMenuStateImmediate(false);
    }

    public void ToggleMenu()
    {
        if (isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        isOpen = true;
        StartMenuAnimation(1f, 1f);
    }

    public void CloseMenu()
    {
        isOpen = false;
        StartMenuAnimation(0f, 0f);
    }

    private void SetMenuStateImmediate(bool open)
    {
        isOpen = open;

        if (menuRect != null)
        {
            menuRect.localScale = open ? Vector3.one : new Vector3(1f, 0f, 1f);
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = open ? 1f : 0f;
                menuCanvasGroup.blocksRaycasts = open;
            }
        }
    }

    private void StartMenuAnimation(float targetScaleY, float targetAlpha)
    {
        if (menuCoroutine != null)
            StopCoroutine(menuCoroutine);

        menuCoroutine = StartCoroutine(AnimateMenuRoutine(targetScaleY, targetAlpha));
    }
    public void OnClickSettingButton()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenSettingsPopup();
        }

        CloseMenu();
    }

    private IEnumerator AnimateMenuRoutine(float targetScaleY, float targetAlpha)
    {
        Vector3 startScale = menuRect.localScale;
        Vector3 targetScale = new Vector3(1f, targetScaleY, 1f);

        float startAlpha = menuCanvasGroup.alpha;
        float elapsed = 0f;

        menuCanvasGroup.blocksRaycasts = (targetAlpha > 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            menuRect.localScale = Vector3.Lerp(startScale, targetScale, t);
            menuCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        menuRect.localScale = targetScale;
        menuCanvasGroup.alpha = targetAlpha;


    }
}