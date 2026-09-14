using UnityEngine;
using UnityEngine.InputSystem;

public class ExitPopup : MonoBehaviour
{
    private void Update()
    {
        if (gameObject.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnClickNo();
        }
    }

    public void OpenPopup()
    {
        gameObject.SetActive(true);
    }

    public void OnClickNo()
    {
        gameObject.SetActive(false);
    }

    public void OnClickYes()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
