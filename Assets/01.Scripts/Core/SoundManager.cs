using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip titleBgm;
    [SerializeField] private AudioClip managementBgm; // 경영 씬 브금
    //[SerializeField] private AudioClip combatBgm;     // 전투 씬 브금
    //[SerializeField] private AudioClip bossBgm;       // 보스 씬 브금

    private const string BGM_KEY = "BGMVolume";
    private const string SFX_KEY = "SFXVolume";

    protected override void Awake()
    {
        base.Awake();

        if (instance != this) return;

        isDontDestroy = true;
        DontDestroyOnLoad(gameObject);

        float savedBgm = PlayerPrefs.GetFloat(BGM_KEY, 0.5f);
        float savedSfx = PlayerPrefs.GetFloat(SFX_KEY, 0.5f);

        SetBGMVolume(savedBgm);
        SetSFXVolume(savedSfx);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름이 확정되면 아래 주석을 풀고 실제 씬 이름으로 변경
        switch (scene.name)
        {
            case "TitleScene":
                PlayBGM(titleBgm);
                break;


            case "MainScene": // 경영 씬 이름
                PlayBGM(managementBgm);
                break;

            //case "CombatScene": // 전투 씬 이름
            //    PlayBGM(combatBgm);
            //    break;

            //case "BossScene": // 보스 씬 이름
            //    PlayBGM(bossBgm);
            //    break;


            default:
                break;
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = volume;
        }

        PlayerPrefs.SetFloat(BGM_KEY, volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = volume;
        }

        PlayerPrefs.SetFloat(SFX_KEY, volume);
    }

    public float GetBGMVolume() => bgmAudioSource != null ? bgmAudioSource.volume : 0.5f;
    public float GetSFXVolume() => sfxAudioSource != null ? sfxAudioSource.volume : 0.5f;

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        if (bgmAudioSource.isPlaying && bgmAudioSource.clip == clip)
            return;

        bgmAudioSource.clip = clip;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        if (sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(clip);
        }
    }
}
