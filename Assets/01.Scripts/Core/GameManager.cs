using UnityEngine;

/// <summary>
/// 게임 전체에서 공유하는 플레이어 진행 데이터를 관리합니다.
/// </summary>
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

    /// <summary>
    /// 저장된 데이터를 적용합니다.
    /// </summary>
    public void SetPlayerData(PlayerData playerData)
    {
        PlayerData = playerData ?? new PlayerData();
    }

    /// <summary>
    /// 새 게임에 사용할 기본 데이터를 생성합니다.
    /// </summary>
    public void CreateNewPlayerData()
    {
        PlayerData = new PlayerData();
    }
}
