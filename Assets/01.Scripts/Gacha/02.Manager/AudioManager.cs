using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip bgm;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip BottonSFX;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // BGM AudioSource 자동 생성
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        // SFX AudioSource 자동 생성
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        // BGM 설정
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        // SFX 설정
        sfxSource.playOnAwake = false;
    }

    private void Start()
    {
        PlayBusinessSceneBGM();
    }

    /// <summary>
    /// BusinessScene 배경음악 재생
    /// </summary>
    public void PlayBusinessSceneBGM()
    {
        if (bgm == null)
        {
            return;
        }

        bgmSource.clip = bgm;
        bgmSource.Play();
    }

    /// <summary>
    /// 배경음악 정지
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// 버튼 클릭 효과음
    /// </summary>
    public void PlayButtonSound()
    {
        PlaySFX(BottonSFX);
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}