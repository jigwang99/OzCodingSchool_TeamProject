using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    [Header("Button SFX")]
    [SerializeField] private AudioClip clickSound;

    [Header("Sound Cooldown")]
    [SerializeField] private float soundCooldown = 5f;

    private Button button;
    private float nextSoundTime = 0f;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClickSound);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (Time.unscaledTime < nextSoundTime)
            return;

        if (SoundManager.instance == null || clickSound == null)
            return;

        SoundManager.instance.PlaySFX(clickSound);
        nextSoundTime = Time.unscaledTime + soundCooldown;
    }
}