using System;

[Serializable]
public class PlayerData
{
    // 재화
    public int gold;
    public int commonFish;

    // 진행도
    public int currentStage = 1;
    public bool isRetryEnabled;

    // 성장 요소
    public int weaponLevel = 1;
    public int restaurantLevel = 1;

    // 마지막 저장 시각 - 이후 오프라인 보상 계산에 사용
    public string lastSaveTime;
}