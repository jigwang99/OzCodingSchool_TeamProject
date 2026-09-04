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

        LoadPlayerData();

        // 씬에 별도 배치하지 않아도 방치 보상은 게임 전체에서 계속 동작해야 한다.
        if (GetComponent<IdleFishManager>() == null)
            gameObject.AddComponent<IdleFishManager>();
    }

    private void LoadPlayerData()
    {
        PlayerData savedData = SaveManager.instance.Load();

        if (savedData != null)
        {
            SetPlayerData(savedData);
            Debug.Log("[GameManager] 저장 데이터 적용 완료!");
        }
        else
        {
            CreateNewPlayerData();
            Debug.Log("[GameManager] 저장 데이터가 없어 새 게임 데이터를 생성했습니다.");
        }
    }

    public void SetPlayerData(PlayerData playerData)
    {
        PlayerData = playerData ?? new PlayerData();
    }

    public void CreateNewPlayerData()
    {
        PlayerData = new PlayerData();
    }
}