using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SceneTimeTester : MonoBehaviour
{
    [Header("UI 텍스트 연결 (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI timeInfoText;

    private void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (SceneTimeManager.Instance != null)
        {
            float awaySeconds = SceneTimeManager.Instance.ConsumeElapsedSeconds(currentSceneName);

            if (timeInfoText != null)
            {
                if (awaySeconds > 0)
                {
                    timeInfoText.text = $"[{currentSceneName}]\n비워둔 시간: {awaySeconds:F1}초";
                }
                else
                {
                    timeInfoText.text = $"[{currentSceneName}]\n기록 없음 (0초)";
                }
            }

            Debug.Log($"[{currentSceneName}] 진입! 측정된 비운 시간: {awaySeconds:F1}초");
        }
        else
        {
            if (timeInfoText != null)
            {
                timeInfoText.text = "SceneTimeManager가 없습니다!";
            }
        }
    }
}
