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

        CreateNewPlayerData();
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
