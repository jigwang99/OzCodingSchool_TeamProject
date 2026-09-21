using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public PlayerData PlayerData { get; private set; }

    private float lastMasterVolume = 1f;

    protected override void Awake()
    {
        isDontDestroy = true;
        base.Awake();

        if (instance != this)
            return;

        // [추가] 백그라운드에서도 게임이 멈추지 않고 진행되도록 설정
        Application.runInBackground = true;

        LoadPlayerData();

        // 씬에 별도 배치하지 않아도 방치 보상은 게임 전체에서 계속 동작해야 한다.
        if (GetComponent<IdleFishManager>() == null)
            gameObject.AddComponent<IdleFishManager>();
    }

    // [추가] 창 포커스를 잃으면(다른 창 클릭) 소리를 끄고, 돌아오면 다시 켬
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // 게임으로 돌아왔을 때는 이전 볼륨으로 복구
            AudioListener.volume = lastMasterVolume;
        }
        else
        {
            // 백그라운드로 나갈 때는 현재 볼륨을 저장하고 0으로 만듦
            // (소리는 일시정지되지 않고 무음 상태로 실시간 소모됨)
            lastMasterVolume = AudioListener.volume;
            AudioListener.volume = 0f;
        }
    }

    private void LoadPlayerData()
    {
        PlayerData savedData = SaveManager.instance.Load();

        if (savedData != null)
        {
            SetPlayerData(savedData);
        }
        else
        {
            CreateNewPlayerData();
        }
    }

    public void SetPlayerData(PlayerData playerData)
    {
        PlayerData = playerData ?? new PlayerData();
        PlayerData.InitializeStageProgress();
        PlayerData.InitializeWeapons();
        IdleFishManager.PrepareLoadedData(PlayerData);
    }

    public void CreateNewPlayerData()
    {
        SetPlayerData(new PlayerData());
    }
}
