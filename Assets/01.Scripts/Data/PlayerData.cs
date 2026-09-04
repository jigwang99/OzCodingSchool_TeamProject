using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    // 재화 - 골드
    public int gold;

    // 재화 - 물고기 (등급별 종 재고)
    public int[] commonFish = new int[8];
    public int[] rareFish = new int[4];
    public int[] uniqueFish = new int[2];
    public int[] epicFish = new int[1];

    // 진행도
    public int currentStage = 1;
    public bool isRetryEnabled;

    // 성장 요소 (기존)
    public int weaponLevel = 1;
    public int fishDropRateLevel = 1;
    public int restaurantLevel = 1;      // ← 식당 레벨은 이 값을 공용으로 사용

    // 식당 상태 (구 PlayerPrefs 저장분)
    public int chefCatLevel = 1;         // 셰프고양이 레벨
    public int cookCatNum = 0;           // 직원고양이 수
    public int[] foodMachine = new int[5]; // 가구 5칸

    // 식당 업그레이드 효과값
    public float MakeSpeed = 0f;
    public float GoldBonus = 0f;
    public float SpecialChance = 0f;
    public float makeDouble = 0f;
    public float NoUseFishChance = 0;

    // 마지막 저장 시각
    public string lastSaveTime;

    // 방치 물고기 생산 상태
    public long idleFishLastCollectionUtcTicks;
    public float idleFishFraction;
    public bool idleFishAccumulationEnabled;
    public int idleFishNextCommonSpecies;

    // 상태 변경 이벤트 (직렬화 대상 아님)
    [field: NonSerialized] public Action OnRetryChanged;
    [field: NonSerialized] public Action OnStageChanged;

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

    // 진행도 setter
    public void SetCurrentStage(int stage)
    {
        int clamped = Mathf.Max(1, stage);
        if (currentStage == clamped)
            return;

        currentStage = Mathf.Max(1, stage);
    }

    public void SetRetryEnabled(bool enabled)
    {
        if (isRetryEnabled == enabled)
            return;

        isRetryEnabled = enabled;
        OnRetryChanged?.Invoke();
    }
}