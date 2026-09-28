using System.Collections;
using UnityEngine;

public class UIScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform screenContainer;
    [SerializeField] private RectTransform screenViewport;

    [Header("Screen Settings")]
    [SerializeField] private int totalScreens = 3;
    [SerializeField] private int startScreen = 0;
    [SerializeField] private float slideDuration = 0.4f;

    private RectTransform[] pages;
    private int currentScreen;
    private float displayedScreen;
    private float viewportWidth = -1f;
    private bool isSliding;

    private void Start()
    {
        if (screenContainer == null || screenViewport == null || screenContainer.childCount == 0)
        {
            enabled = false;
            return;
        }

        totalScreens = Mathf.Clamp(totalScreens, 1, screenContainer.childCount);
        pages = new RectTransform[totalScreens];
        for (int i = 0; i < totalScreens; i++)
            pages[i] = (RectTransform)screenContainer.GetChild(i);

        currentScreen = Mathf.Clamp(startScreen, 0, totalScreens - 1);
        displayedScreen = currentScreen;
        Canvas.ForceUpdateCanvases();
        RefreshLayout();
    }

    private void OnEnable()
    {
        viewportWidth = -1f;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isSliding = false;
        displayedScreen = currentScreen;
    }

    private void Update()
    {
        if (isSliding || pages == null)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow)) NextScreen();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) PreviousScreen();
    }

    private void LateUpdate() => RefreshLayout();

    private void RefreshLayout()
    {
        if (pages == null) return;
        float width = screenViewport.rect.width;
        if (width <= 0f || Mathf.Approximately(width, viewportWidth)) return;
        viewportWidth = width;

        // Keep each page's authored size; only the distance between page centers changes.
        screenContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * totalScreens);
        float centerIndex = (totalScreens - 1) * 0.5f;
        for (int i = 0; i < pages.Length; i++)
        {
            Vector2 position = pages[i].anchoredPosition;
            position.x = (i - centerIndex) * width;
            pages[i].anchoredPosition = position;
        }
        ApplyPosition();
    }

    public void NextScreen() => MoveToScreen(currentScreen + 1);
    public void PreviousScreen() => MoveToScreen(currentScreen - 1);

    public void MoveToScreen(int screenIndex)
    {
        if (!isActiveAndEnabled || pages == null || isSliding || screenIndex < 0 ||
            screenIndex >= totalScreens || screenIndex == currentScreen)
            return;
        StartCoroutine(SlideScreen(screenIndex));
    }

    private IEnumerator SlideScreen(int targetScreen)
    {
        isSliding = true;
        float start = displayedScreen;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, slideDuration);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            displayedScreen = Mathf.Lerp(start, targetScreen, t);
            RefreshLayout();
            ApplyPosition();
            yield return null;
        }
        currentScreen = targetScreen;
        displayedScreen = currentScreen;
        ApplyPosition();
        isSliding = false;
    }

    private void ApplyPosition()
    {
        // Interpolate page indices so resizing during a slide keeps its progress.
        float centerIndex = (totalScreens - 1) * 0.5f;
        screenContainer.anchoredPosition = new Vector2((centerIndex - displayedScreen) * viewportWidth, 0f);
    }
}