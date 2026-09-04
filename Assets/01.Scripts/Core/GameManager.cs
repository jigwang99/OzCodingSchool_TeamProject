using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public PlayerData PlayerData { get; private set; }

    protected override void Awake()
    {
        isDontDestroy = true;
        base.Awake();

        if (instance != this)
        {
            return;
        }

        LoadPlayerData();
    }

    // 저장된 데이터가 있으면 불러오고,
    // 없으면 새 데이터를 생성
    private void LoadPlayerData()
    {
        PlayerData savedData = SaveManager.instance.Load();

        if (savedData != null)
        {
            SetPlayerData(savedData);

            Debug.Log($"[GameManager] 저장 데이터 적용 완료!");
        }
        else
        {
            CreateNewPlayerData();

            Debug.Log("[GameManager] 저장 데이터가 없어 새 게임 데이터를 생성했습니다.");
        }
    }

    // 저장된 데이터를 적용
    public void SetPlayerData(PlayerData playerData)
    {
        PlayerData = playerData ?? new PlayerData();
    }

    // 새 게임에 사용할 기본 데이터를 생성
    public void CreateNewPlayerData()
    {
        PlayerData = new PlayerData();
    }
}