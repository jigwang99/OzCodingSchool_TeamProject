using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Cat Settings")]
    [SerializeField] private Image catImage;
    [SerializeField] private Sprite[] catSprites;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float minimumLoadingTime = 1.5f;

    [Header("Loading Tips")]
    [SerializeField]
    private string[] loadingTips = new string[]
    {
       
    };

    private Tween textPulseTween;
    private Tween catRotateTween;
    private Tween catBounceTween;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
            loadingCanvasGroup.interactable = false;
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }

        if (catImage != null && catSprites != null && catSprites.Length > 0)
        {
            catImage.sprite = catSprites[Random.Range(0, catSprites.Length)];
            catImage.SetNativeSize();

            catImage.transform.localRotation = Quaternion.identity;

            catRotateTween = catImage.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10f), 0.4f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);

            catBounceTween = catImage.transform
                .DOBlendableLocalMoveBy(new Vector3(0f, 15f, 0f), 0.35f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.blocksRaycasts = true;
            loadingCanvasGroup.interactable = true;
            yield return loadingCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).WaitForCompletion();
        }

        if (progressBar != null) progressBar.value = 0f;
        if (progressText != null) progressText.text = "0%";

        if (loadingText != null)
        {
            textPulseTween = loadingText.transform
                .DOScale(1.08f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float timer = 0f;
        float currentProgress = 0f;

        while (!op.isDone)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;

            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.unscaledDeltaTime * 2.0f);

            if (progressBar != null)
                progressBar.value = currentProgress;

            if (progressText != null)
                progressText.text = $"{Mathf.RoundToInt(currentProgress * 100f)}%";

            if (op.progress >= 0.9f && timer >= minimumLoadingTime && currentProgress >= 0.99f)
            {
                if (progressBar != null) progressBar.value = 1f;
                if (progressText != null) progressText.text = "100%";

                yield return new WaitForSecondsRealtime(0.2f);

                KillTweens();

                op.allowSceneActivation = true;
                yield break;
            }
        }
    }

    private void KillTweens()
    {
        if (textPulseTween != null && textPulseTween.IsActive()) textPulseTween.Kill();
        if (catRotateTween != null && catRotateTween.IsActive()) catRotateTween.Kill();
        if (catBounceTween != null && catBounceTween.IsActive()) catBounceTween.Kill();
        if (loadingCanvasGroup != null) loadingCanvasGroup.DOKill();
    }

    private void OnDisable()
    {
        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
    }
}