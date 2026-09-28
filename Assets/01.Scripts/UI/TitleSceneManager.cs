using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "BusinessScene";
    [SerializeField] private AudioClip startButtonSound;
    [SerializeField] private LoadingScreen loadingScreen;
    [SerializeField] private UISettingsPopup settingsPopup;
    [SerializeField] private ExitPopup exitPopup;

    private void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (loadingScreen == null)
        {
            loadingScreen = FindAnyObjectByType<LoadingScreen>(FindObjectsInactive.Include);
        }

        if (settingsPopup == null)
        {
            settingsPopup = FindAnyObjectByType<UISettingsPopup>(FindObjectsInactive.Include);
        }
    }

    public void OnClickStart()
    {
        if (SoundManager.instance != null && startButtonSound != null)
        {
            SoundManager.instance.PlaySFX(startButtonSound);
        }

        if (loadingScreen != null)
        {
            loadingScreen.LoadScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OnClickSettings()
    {
        if (settingsPopup == null)
        {
            settingsPopup = FindAnyObjectByType<UISettingsPopup>(FindObjectsInactive.Include);
        }

        if (settingsPopup != null)
        {
            settingsPopup.gameObject.SetActive(true);
        }
    }

    public void OnClickExit()
    {
        if (exitPopup == null)
        {
            exitPopup = FindAnyObjectByType<ExitPopup>(FindObjectsInactive.Include);
        }

        if (exitPopup != null)
        {
            exitPopup.OpenPopup();
        }
    }
}
