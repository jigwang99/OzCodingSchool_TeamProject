using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private AudioClip startButtonSound;
    [SerializeField] private UISettingsPopup settingsPopup;

    private void Awake()
    {
        if (settingsPopup == null)
        {
            settingsPopup = FindObjectOfType<UISettingsPopup>(true);
        }
    }
    public void OnClickStart()
    {
        SceneManager.LoadScene(gameSceneName);

        if (SoundManager.instance != null && startButtonSound != null)
        {
            SoundManager.instance.PlaySFX(startButtonSound);
        }

    }

    public void OnClickSettings()
    {
        if (settingsPopup != null)
        {
            settingsPopup.gameObject.SetActive(true);
        }
        else
        {
            settingsPopup = FindObjectOfType<UISettingsPopup>(true);
            if (settingsPopup != null)
            {
                settingsPopup.gameObject.SetActive(true);
            }
        }
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
