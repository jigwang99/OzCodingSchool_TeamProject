using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public PlayerData PlayerData { get; private set; }

    protected override void Awake()
    {
        isDontDestroy = true;
        base.Awake();

        if (instance != this)
            return;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        LoadPlayerData();

        // 씬에 별도 배치하지 않아도 방치 보상은 게임 전체에서 계속 동작해야 함.
        if (GetComponent<IdleFishManager>() == null)
            gameObject.AddComponent<IdleFishManager>();


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
