using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeScene(string targetSceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeAndLoadRoutine(targetSceneName));
    }

    private IEnumerator FadeAndLoadRoutine(string targetSceneName)
    {
        isTransitioning = true;

     
        string currentScene = SceneManager.GetActiveScene().name;
        SceneTimeManager.Instance?.RecordExitTime(currentScene);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(targetSceneName);
        while (!op.isDone) yield return null;

        if (fadeCanvasGroup != null)
        {
            float t = fadeDuration;
            while (t > 0f)
            {
                t -= Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.blocksRaycasts = false;
        }

        isTransitioning = false;
    }
}
