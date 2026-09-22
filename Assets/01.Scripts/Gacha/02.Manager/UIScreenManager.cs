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

    private int currentScreen;
    private bool isSliding;

    private void Start()
    {
        currentScreen = Mathf.Clamp(
            startScreen, 0, totalScreens - 1
        );

        SetScreenPosition(currentScreen);
    }

    private void Update()
    {
        if (isSliding)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextScreen();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousScreen();
        }
    }

    public void NextScreen()
    {
        if (isSliding)
            return;

        if (currentScreen >= totalScreens - 1)
            return;

        MoveToScreen(currentScreen + 1);
    }

    public void PreviousScreen()
    {
        if (isSliding)
            return;

        if (currentScreen <= 0)
            return;

        MoveToScreen(currentScreen - 1);
    }

    public void MoveToScreen(int screenIndex)
    {
        if (isSliding)
            return;

        if (screenIndex < 0 || screenIndex >= totalScreens)
            return;

        if (screenIndex == currentScreen)
            return;

        StartCoroutine(SlideScreen(screenIndex));
    }

    private IEnumerator SlideScreen(int targetScreen)
    {
        isSliding = true;

        Vector2 startPosition =
            screenContainer.anchoredPosition;

        Vector2 targetPosition =
            GetScreenPosition(targetScreen);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, slideDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            t = Mathf.SmoothStep(0f, 1f, t);

            screenContainer.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        screenContainer.anchoredPosition =
            targetPosition;

        currentScreen = targetScreen;
        isSliding = false;
    }

    private Vector2 GetScreenPosition(int screenIndex)
    {
        float screenWidth = screenViewport.rect.width;

        float centerIndex = (totalScreens - 1) * 0.5f;

        float targetX =
            (centerIndex - screenIndex) * screenWidth;

        return new Vector2(targetX, 0f);
    }

    private void SetScreenPosition(int screenIndex)
    {
        screenContainer.anchoredPosition =
            GetScreenPosition(screenIndex);
    }
}