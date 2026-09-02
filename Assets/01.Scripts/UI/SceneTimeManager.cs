using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTimeManager : MonoBehaviour
{
    public static SceneTimeManager Instance { get; private set; }

    private Dictionary<string, DateTime> exitTimes = new Dictionary<string, DateTime>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllSavedTimes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (!string.IsNullOrEmpty(scene.name))
        {
            RecordExitTime(scene.name);
        }
    }

    public void RecordExitTime(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        DateTime now = DateTime.UtcNow;
        exitTimes[sceneName] = now;

        PlayerPrefs.SetString($"ExitTime_{sceneName}", now.Ticks.ToString());
        PlayerPrefs.Save();

        Debug.Log($"[SceneTimeManager] '{sceneName}' 퇴장 시각 기록 완료: {now}");
    }

    public float ConsumeElapsedSeconds(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return 0f;

        if (!exitTimes.ContainsKey(sceneName))
        {
            string savedTimeStr = PlayerPrefs.GetString($"ExitTime_{sceneName}", "");
            if (!string.IsNullOrEmpty(savedTimeStr) && long.TryParse(savedTimeStr, out long ticks))
            {
                exitTimes[sceneName] = new DateTime(ticks, DateTimeKind.Utc);
            }
        }

        if (exitTimes.TryGetValue(sceneName, out DateTime exitTime))
        {
            TimeSpan span = DateTime.UtcNow - exitTime;
            float elapsedSeconds = (float)span.TotalSeconds;

            if (elapsedSeconds < 0f) elapsedSeconds = 0f;

            exitTimes.Remove(sceneName);
            PlayerPrefs.DeleteKey($"ExitTime_{sceneName}");
            PlayerPrefs.Save();

            Debug.Log($"[SceneTimeManager] '{sceneName}' 부재 시간 지급 완료: {elapsedSeconds:F1}초");
            return elapsedSeconds;
        }

        return 0f;
    }

    private void LoadAllSavedTimes()
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            string savedTimeStr = PlayerPrefs.GetString($"ExitTime_{sceneName}", "");
            if (!string.IsNullOrEmpty(savedTimeStr) && long.TryParse(savedTimeStr, out long ticks))
            {
                exitTimes[sceneName] = new DateTime(ticks, DateTimeKind.Utc);
            }
        }
    }

    private void OnApplicationQuit()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        RecordExitTime(currentScene);
    }
}