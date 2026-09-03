using System;

[Serializable]
public class PlayerData
{
    // 재화 - 골드
    public int gold;

    // 재화 - 물고기 (등급별 종 재고, 배열 인덱스 = 종 번호)
    public int[] commonFish = new int[8];
    public int[] rareFish = new int[4];
    public int[] uniqueFish = new int[2];
    public int[] epicFish = new int[1];

    // 진행도
    public int currentStage = 1;
    public bool isRetryEnabled;

    // 성장 요소
    public int weaponLevel = 1;
    public int restaurantLevel = 1;

    // 마지막 저장 시각 - 이후 오프라인 보상 계산에 사용
    public string lastSaveTime;

    // 등급 → 해당 등급의 종 재고 배열
    public int[] GetFishArray(FishGrade grade)
    {
        switch (grade)
        {
            case FishGrade.Common: return commonFish;
            case FishGrade.Rare: return rareFish;
            case FishGrade.Unique: return uniqueFish;
            case FishGrade.Epic: return epicFish;
            default: return null;
        }
    }
}