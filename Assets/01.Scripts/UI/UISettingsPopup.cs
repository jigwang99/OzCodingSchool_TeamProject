using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro; // TMP_Dropdown 제어를 위해 추가

public class UISettingsPopup : MonoBehaviour
{
    [Header("Content Panels")]
    [SerializeField] private GameObject soundContentPanel;
    [SerializeField] private GameObject convenienceContentPanel;

    [Header("Confirm Delete Popup")]
    [SerializeField] private GameObject confirmDeletePanel; // [추가] 저장 파일 삭제 확인 팝업 패널

    [Header("Tab Images")]
    [SerializeField] private Image soundTabImage;
    [SerializeField] private Image convenienceTabImage;

    [Header("Tab Colors")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

    [Header("Pause Option")]
    [SerializeField] private bool pauseGameOnOpen = true;

    [Header("Sound Settings")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Convenience Settings")]
    [SerializeField] private TMP_Dropdown frameRateDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Damage Display Settings")]
    [SerializeField] private Toggle enemyDamageToggle;
    [SerializeField] private Toggle playerDamageToggle;

    [Header("Menu Return Button")]
    [SerializeField] private GameObject returnToMenuButton;
    [SerializeField] private ExitPopup exitPopup;

    private List<Resolution> resolutions = new List<Resolution>();

    public static bool IsShowEnemyDamage => PlayerPrefs.GetInt("ShowEnemyDamage", 1) == 1;
    public static bool IsShowPlayerDamage => PlayerPrefs.GetInt("ShowPlayerDamage", 1) == 1;

    private void Start()
    {
        if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // 편의 옵션 초기화 및 이벤트 연결
        InitConvenienceSettings();
    }

    private void OnEnable()
    {
        if (confirmDeletePanel != null)
            confirmDeletePanel.SetActive(false);

        OnClickSoundTab();

        if (SoundManager.instance != null)
        {
            if (bgmSlider != null) bgmSlider.value = SoundManager.instance.GetBGMVolume();
            if (sfxSlider != null) sfxSlider.value = SoundManager.instance.GetSFXVolume();
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        if (returnToMenuButton != null)
        {
            returnToMenuButton.SetActive(currentSceneName != "TitleScene");
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (exitPopup == null)
            {
                exitPopup = FindFirstObjectByType<ExitPopup>(FindObjectsInactive.Include);
            }

            if (exitPopup != null && exitPopup.gameObject.activeSelf)
            {
                return;
            }

            Toggle();
        }
    }

    #region Tab & Popup Control
    public void Toggle()
    {
        if (gameObject.activeSelf) Close();
        else Open();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        OnClickSoundTab();

        string currentSceneName = SceneManager.GetActiveScene().name;
        if (returnToMenuButton != null)
        {
            returnToMenuButton.SetActive(currentSceneName != "TitleScene");
        }

        if (pauseGameOnOpen) Time.timeScale = 0f;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (pauseGameOnOpen) Time.timeScale = 1f;
    }

    public void OnClickSoundTab()
    {
        if (soundContentPanel != null) soundContentPanel.SetActive(true);
        if (convenienceContentPanel != null) convenienceContentPanel.SetActive(false);

        if (soundTabImage != null) soundTabImage.color = activeColor;
        if (convenienceTabImage != null) convenienceTabImage.color = inactiveColor;
    }

    public void OnClickConvenienceTab()
    {
        if (soundContentPanel != null) soundContentPanel.SetActive(false);
        if (convenienceContentPanel != null) convenienceContentPanel.SetActive(true);

        if (soundTabImage != null) soundTabImage.color = inactiveColor;
        if (convenienceTabImage != null) convenienceTabImage.color = activeColor;
    }

    #endregion

    #region Sound Settings Logic
    private void OnBGMVolumeChanged(float value)
    {
        if (SoundManager.instance != null) SoundManager.instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.instance != null) SoundManager.instance.SetSFXVolume(value);
    }
    #endregion

    #region Convenience Settings Logic
    public void OnEnemyDamageChanged(bool isOn)
    {
        PlayerPrefs.SetInt("ShowEnemyDamage", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnPlayerDamageChanged(bool isOn)
    {
        PlayerPrefs.SetInt("ShowPlayerDamage", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void InitConvenienceSettings()
    {
        // 프레임 제한 초기화
        QualitySettings.vSyncCount = 0;
        if (frameRateDropdown != null)
        {
            frameRateDropdown.onValueChanged.RemoveAllListeners();
            int savedFrameIndex = PlayerPrefs.GetInt("FrameRateIndex", 1); // 기본값: 60FPS(Index 1)
            SetFrameRate(savedFrameIndex);
            frameRateDropdown.value = savedFrameIndex;
            frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
        }

        // 해상도 드롭다운 목록 자동 생성
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.ClearOptions();

            Resolution[] allResolutions = Screen.resolutions;
            HashSet<string> uniqueResSet = new HashSet<string>();
            resolutions.Clear();

            int currentResIndex = 0;
            List<string> options = new List<string>();

            for (int i = 0; i < allResolutions.Length; i++)
            {
                // 주사율 차이로 인한 중복 해상도 제거
                string option = $"{allResolutions[i].width} x {allResolutions[i].height}";
                if (!uniqueResSet.Contains(option))
                {
                    uniqueResSet.Add(option);
                    resolutions.Add(allResolutions[i]);
                    options.Add(option);

                    if (allResolutions[i].width == Screen.currentResolution.width &&
                        allResolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResIndex = resolutions.Count - 1;
                    }
                }
            }

            resolutionDropdown.AddOptions(options);

            int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResIndex);
            savedResIndex = Mathf.Clamp(savedResIndex, 0, resolutions.Count - 1);

            SetResolution(savedResIndex);
            resolutionDropdown.value = savedResIndex;
            resolutionDropdown.RefreshShownValue();

            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }

        // 전체화면 토글 초기화
        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveAllListeners();
            bool isFull = PlayerPrefs.GetInt("IsFullscreen", Screen.fullScreen ? 1 : 0) == 1;
            Screen.fullScreen = isFull;
            fullscreenToggle.isOn = isFull;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        // 적 데미지 토글 초기화
        if (enemyDamageToggle != null)
        {
            enemyDamageToggle.onValueChanged.RemoveAllListeners();
            bool showEnemy = PlayerPrefs.GetInt("ShowEnemyDamage", 1) == 1;
            enemyDamageToggle.isOn = showEnemy;
            enemyDamageToggle.onValueChanged.AddListener(OnEnemyDamageChanged);
        }

        // 내 데미지 토글 초기화
        if (playerDamageToggle != null)
        {
            playerDamageToggle.onValueChanged.RemoveAllListeners();
            bool showPlayer = PlayerPrefs.GetInt("ShowPlayerDamage", 1) == 1;
            playerDamageToggle.isOn = showPlayer;
            playerDamageToggle.onValueChanged.AddListener(OnPlayerDamageChanged);
        }
    }

    public void OnFrameRateChanged(int index)
    {
        SetFrameRate(index);
        PlayerPrefs.SetInt("FrameRateIndex", index);
        PlayerPrefs.Save();
    }

    private void SetFrameRate(int index)
    {
        switch (index)
        {
            case 0: Application.targetFrameRate = 30; break;
            case 1: Application.targetFrameRate = 60; break;
            case 2: Application.targetFrameRate = 120; break;
            case 3: Application.targetFrameRate = -1; break;
        }
    }

    public void OnResolutionChanged(int index)
    {
        SetResolution(index);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    private void SetResolution(int index)
    {
        if (index >= 0 && index < resolutions.Count)
        {
            Resolution res = resolutions[index];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);

#if UNITY_EDITOR
            // 에디터에서만 콘솔 창에 출력되는 확인용 로그
            Debug.Log($"[해상도 변경] {res.width} x {res.height} (전체화면: {Screen.fullScreen})");
#endif
        }
    }

    public void OnFullscreenChanged(bool isFull)
    {
        Screen.fullScreen = isFull;
        PlayerPrefs.SetInt("IsFullscreen", isFull ? 1 : 0);
        PlayerPrefs.Save();
    }

    #region Save File Delete Flow (Popup)
    public void OnClickDeleteSaveData()
    {
        if (confirmDeletePanel != null)
        {
            confirmDeletePanel.SetActive(true);
        }
        else
        {
            ExecuteDeleteSaveData();
        }
    }

    public void OnClickConfirmDeleteYes()
    {
        ExecuteDeleteSaveData();
    }

    public void OnClickConfirmDeleteNo()
    {
        if (confirmDeletePanel != null)
        {
            confirmDeletePanel.SetActive(false);
        }
    }

    private void ExecuteDeleteSaveData()
    {
        if (SaveManager.instance != null)
        {
            SaveManager.instance.DeleteSaveFile();

            if (GameManager.instance != null)
            {
                GameManager.instance.CreateNewPlayerData();
            }

            Close();
            Time.timeScale = 1f;

            SceneManager.LoadScene("TitleScene");
        }
    }
    #endregion
    #endregion

    #region Scene / Game Exit
    public void OnClickReturnToTitle()
    {
        Close();
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    public void OnClickExitGame()
    {
        if (exitPopup == null)
        {
            exitPopup = FindFirstObjectByType<ExitPopup>(FindObjectsInactive.Include);
        }

        if (exitPopup != null)
        {
            exitPopup.OpenPopup();
        }
    }
    #endregion
}