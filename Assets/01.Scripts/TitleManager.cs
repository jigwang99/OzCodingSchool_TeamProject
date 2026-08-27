using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string nextSceneName = "MainScene";

    [Header("Settings UI - Popup & Panels")]
    [SerializeField] private GameObject settingsPopup;
    [SerializeField] private GameObject soundContentPanel;
    [SerializeField] private GameObject convenienceContentPanel;

    [Header("Settings UI - Sound Controls")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        LoadSettings();
    }
    public void OnClickStart()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickSettings()
    {
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(true);
            OnClickSoundTab();
        }
    }

    public void OnClickCloseSettings()
    {
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(false);
        }
    }

    public void OnClickSoundTab()
    {
        if (soundContentPanel != null) soundContentPanel.SetActive(true);
        if (convenienceContentPanel != null) convenienceContentPanel.SetActive(false);
    }

    public void OnClickConvenienceTab()
    {
        if (soundContentPanel != null) soundContentPanel.SetActive(false);
        if (convenienceContentPanel != null) convenienceContentPanel.SetActive(true);
    }

    public void OnChangeBGMVolume(float value)
    {
        PlayerPrefs.SetFloat("BGMVolume", value);
        PlayerPrefs.Save();
    }

    public void OnChangeSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        // BGM 저장값 불러오기 (기본값 1.0)
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
        if (bgmSlider != null) bgmSlider.value = savedBGM;

        // SFX 저장값 불러오기 (기본값 1.0)
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        if (sfxSlider != null) sfxSlider.value = savedSFX;
    }
}
