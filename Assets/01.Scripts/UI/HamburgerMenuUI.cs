using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HamburgerMenuUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private RectTransform menuRect;
    [SerializeField] private string titleSceneName = "TitleScene";

    [Header("애니메이션 설정")]
    [SerializeField] private float duration = 0.15f;

    private CanvasGroup rootCanvasGroup;
    private CanvasGroup menuCanvasGroup;
    private bool isOpen = false;
    private Coroutine menuCoroutine;

    private void Awake()
    {
        if (!TryGetComponent(out rootCanvasGroup))
        {
            rootCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SetMenuStateImmediate(false);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        CheckTitleScene(currentScene);
        RefreshChildButtons(currentScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (menuCoroutine != null)
            StopCoroutine(menuCoroutine);

        SetMenuStateImmediate(false);
        CheckTitleScene(scene.name);
        RefreshChildButtons(scene.name);
    }

    private void CheckTitleScene(string currentSceneName)
    {
        bool isTitle = (currentSceneName == titleSceneName);

        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = isTitle ? 0f : 1f;
            rootCanvasGroup.blocksRaycasts = !isTitle;
            rootCanvasGroup.interactable = !isTitle;
        }
    }

    private void RefreshChildButtons(string currentSceneName)
    {
        SceneTransitionButton[] buttons = GetComponentsInChildren<SceneTransitionButton>(true);
        foreach (var btn in buttons)
        {
            btn.CheckAndToggleVisibility(currentSceneName);
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == titleSceneName) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMenu();
        }
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
        CloseMenu();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenSettingsPopup();
        }
    }

    public void OnClickSceneButton(string sceneName)
    {
        CloseMenu();
        SceneManager.LoadScene(sceneName);
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