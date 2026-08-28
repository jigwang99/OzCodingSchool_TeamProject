using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UISettingsPopup : MonoBehaviour
{
    [Header("Content Panels")]
    [SerializeField] private GameObject soundContentPanel;
    [SerializeField] private GameObject convenienceContentPanel;

    [Header("Tab Images")]
    [SerializeField] private Image soundTabImage;
    [SerializeField] private Image convenienceTabImage;

    [Header("Tab Colors")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

    [Header("Pause Option")]
    [SerializeField] private bool pauseGameOnOpen = true;

    private void OnEnable()
    {
        OnClickSoundTab();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (gameObject.activeSelf)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        OnClickSoundTab();

        if (pauseGameOnOpen)
        {
            Time.timeScale = 0f;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);

        if (pauseGameOnOpen)
        {
            Time.timeScale = 1f;
        }
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

    public void OnClickExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}