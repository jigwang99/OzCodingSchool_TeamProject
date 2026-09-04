using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private UISettingsPopup settingsPopup;

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

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPopup != null)
            {
                settingsPopup.Toggle();
            }
        }
    }
    public void OpenSettingsPopup()
    {
        if (settingsPopup == null) return;

        if (!settingsPopup.gameObject.activeSelf)
        {
            settingsPopup.Toggle();
        }
    }
}
