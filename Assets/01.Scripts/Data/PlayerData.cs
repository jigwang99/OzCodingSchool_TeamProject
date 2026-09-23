using PixelRestaurant.Gacha;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OwnedWeaponData
{
    public string weaponId;
    public int count = 1;
    public int level = 1;
}

[Serializable]
public class OwnedFurnitureData
{
    public string furnitureId;
    public int count = 1;
}

[Serializable]
public class OwnedRecipeData
{
    public string recipeId;
    public int count = 1;
}

[Serializable]
public class PlayerData
{
    // 재화 - 골드
    public BigNumber gold;

    // 재화 - 물고기 (등급별 종 재고)
    public int[] commonFish = new int[8];
    public int[] rareFish = new int[4];
    public int[] uniqueFish = new int[2];
    public int[] epicFish = new int[1];

    // 진행도
    public int currentStage = 1;
    public int highestUnlockedStage = 1;
    public int stageProgressVersion;    // 0: 해금 기록이 없는 기존 저장 데이터
    public bool isRetryEnabled;

    // 성장 요소 (기존)
    public int weaponLevel = 1;         // 이전 저장 데이터 이관 전용. 신규 강화는 ownedWeapons의 level 사용.
    public string equippedWeaponId;     // 비어 있는 기존 저장 데이터는 기본 무기를 장착한다.
    public List<OwnedWeaponData> ownedWeapons = new List<OwnedWeaponData>();
    public int weaponInventoryVersion;
    public int healthLevel = 1;
    public int fishDropRateLevel = 1;
    public int restaurantLevel = 1;      // ← 식당 레벨은 이 값을 공용으로 사용
    public List<OwnedFurnitureData> ownedFurniture = new List<OwnedFurnitureData>();
    public List<OwnedRecipeData> ownedRecipes = new List<OwnedRecipeData>();

    // 무기,가구,레시피 아이템
    public GachaInventoryData gachaInventory = new GachaInventoryData();

    // 식당 상태 (구 PlayerPrefs 저장분)
    public int chefCatLevel = 1;         // 셰프고양이 레벨
    public int cookCatNum = 0;           // 직원고양이 수
    public int[] foodMachine = new int[6]; // 가구 6칸
                                           // 0 가스레인지
                                           // 1 전자레인지
                                           // 2 찜기
                                           // 3 튀김기
                                           // 4 냉장고
                                           // 5 오븐
                                           // 식당에 배치된 가구 정보
                                           // 0 = 빈 슬롯
                                           // 1~6 = 배치된 가구 종류
                                           // 가구 배치 시스템
    public int[] foodMachine2 = new int[5];
    public int[] MachineCount = new int[6];

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
    public int[] pendingIdleCommonFish = new int[8];
    public int[] pendingIdleRareFish = new int[4];
    public int[] pendingIdleUniqueFish = new int[2];
    public int[] pendingIdleEpicFish = new int[1];
    public double pendingIdleSeconds;


    // 상태 변경 이벤트 (직렬화 대상 아님)
    [field: NonSerialized] public Action OnRetryChanged;
    [field: NonSerialized] public Action OnStageChanged;
    [field: NonSerialized] public Action OnWeaponsChanged;

    // 새 게임의 기본 검 지급 및 공통 강화 시절의 저장 데이터 이관. 여러 번 호출해도 중복 지급하지 않는다.
    public void InitializeWeapons(string defaultWeaponId = "swords_0")
    {
        if (ownedWeapons == null)
            ownedWeapons = new List<OwnedWeaponData>();

        if (weaponInventoryVersion < 1)
        {
            string previousId = string.IsNullOrEmpty(equippedWeaponId) ? defaultWeaponId : equippedWeaponId;
            if (GetOwnedWeapon(previousId) == null)
            {
                ownedWeapons.Add(new OwnedWeaponData
                {
                    weaponId = previousId,
                    level = Mathf.Max(1, weaponLevel)
                });
            }
            weaponInventoryVersion = 1;
        }

        if (GetOwnedWeapon(defaultWeaponId) == null)
            ownedWeapons.Add(new OwnedWeaponData { weaponId = defaultWeaponId });

        if (!OwnsWeapon(equippedWeaponId))
            equippedWeaponId = defaultWeaponId;
    }

    public OwnedWeaponData GetOwnedWeapon(string weaponId)
    {
        if (string.IsNullOrEmpty(weaponId) || ownedWeapons == null)
            return null;

        return ownedWeapons.Find(weapon => weapon != null && weapon.weaponId == weaponId && weapon.count > 0);
    }

    public bool OwnsWeapon(string weaponId) => GetOwnedWeapon(weaponId) != null;
    public int GetWeaponLevel(string weaponId) => Mathf.Max(1, GetOwnedWeapon(weaponId)?.level ?? 1);

    // 지급할 ID의 카탈로그 유효성은 획득 시스템에서 검사한다. 중복 획득은 강화 레벨을 변경하지 않는다.
    public bool AddWeapon(string weaponId, int count = 1)
    {
        if (string.IsNullOrWhiteSpace(weaponId) || count <= 0)
            return false;

        if (ownedWeapons == null)
            ownedWeapons = new List<OwnedWeaponData>();

        OwnedWeaponData weapon = GetOwnedWeapon(weaponId);
        if (weapon == null)
            ownedWeapons.Add(new OwnedWeaponData { weaponId = weaponId, count = count });
        else
        {
            if (weapon.count > int.MaxValue - count)
                return false;
            weapon.count += count;
        }

        OnWeaponsChanged?.Invoke();
        return true;
    }

    // Merge ownership is a single unlock per gacha item; preserve independent combat levels.
    // Call only after validating the gacha counts, next-stage mapping and catalog entries.
    public bool TryApplyGachaWeaponMerge(string sourceId, string resultId, bool keepSource)
    {
        if (string.IsNullOrWhiteSpace(sourceId) || string.IsNullOrWhiteSpace(resultId) ||
            sourceId == resultId || ownedWeapons == null) return false;

        OwnedWeaponData source = GetOwnedWeapon(sourceId);
        OwnedWeaponData result = GetOwnedWeapon(resultId);
        if (source == null) return false;
        if (result == null && ownedWeapons.Count == int.MaxValue) return false;

        // Never remove the last default weapon: InitializeWeapons would recreate it.
        if (!keepSource && sourceId == "swords_0") return false;

        if (result == null)
            ownedWeapons.Add(new OwnedWeaponData { weaponId = resultId, count = 1 });
        // An already unlocked result is not granted another combat copy.
        if (!keepSource)
        {
            ownedWeapons.Remove(source);
            if (equippedWeaponId == sourceId) equippedWeaponId = resultId;
        }
        OnWeaponsChanged?.Invoke();
        return true;
    }

    public bool TryEquipOwnedWeapon(string weaponId)
    {
        if (!OwnsWeapon(weaponId))
            return false;

        if (equippedWeaponId != weaponId)
        {
            equippedWeaponId = weaponId;
            OnWeaponsChanged?.Invoke();
        }
        return true;
    }

    // 비용/최대 레벨 검사는 UpgradeManager에서 수행한다.
    public bool TryIncreaseWeaponLevel(string weaponId, int maxLevel)
    {
        OwnedWeaponData weapon = GetOwnedWeapon(weaponId);
        if (weapon == null || GetWeaponLevel(weaponId) >= maxLevel)
            return false;

        weapon.level = GetWeaponLevel(weaponId) + 1;
        OnWeaponsChanged?.Invoke();
        return true;
    }

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

    // 기존 저장은 최초 한 번만 현재 위치까지 해금한다. 이후에는 저장된 해금 기록이 기준이다.
    public void InitializeStageProgress(int stageCount = int.MaxValue)
    {
        int maxStage = Mathf.Max(1, stageCount);
        if (stageProgressVersion < 1)
        {
            highestUnlockedStage = Mathf.Max(highestUnlockedStage, currentStage);
            stageProgressVersion = 1;
        }

        highestUnlockedStage = Mathf.Clamp(highestUnlockedStage, 1, maxStage);
        SetCurrentStage(currentStage);
    }

    public int[] GetPendingIdleFishArray(FishGrade grade)
    {
        switch (grade)
        {
            case FishGrade.Common: return pendingIdleCommonFish;
            case FishGrade.Rare: return pendingIdleRareFish;
            case FishGrade.Unique: return pendingIdleUniqueFish;
            case FishGrade.Epic: return pendingIdleEpicFish;
            default: return null;
        }
    }

    public void InitializePendingIdleFish()
    {
        // 기존 커먼 보상은 그대로 보존하고 새 등급 배열만 보완한다.
        if (pendingIdleCommonFish == null) pendingIdleCommonFish = new int[8];
        if (pendingIdleRareFish == null) pendingIdleRareFish = new int[4];
        if (pendingIdleUniqueFish == null) pendingIdleUniqueFish = new int[2];
        if (pendingIdleEpicFish == null) pendingIdleEpicFish = new int[1];
    }

    public bool UnlockNextStage(int completedStage, int stageCount)
    {
        if (completedStage < 1 || completedStage > highestUnlockedStage || completedStage >= stageCount)
            return false;

        int nextStage = completedStage + 1;
        if (nextStage <= highestUnlockedStage)
            return false;

        highestUnlockedStage = nextStage;
        return true;
    }

    // 현재 위치 변경만으로 잠긴 스테이지를 해금할 수 없다.
    public void SetCurrentStage(int stage)
    {
        int clamped = Mathf.Clamp(stage, 1, Mathf.Max(1, highestUnlockedStage));
        if (currentStage == clamped)
            return;

        currentStage = clamped;
        OnStageChanged?.Invoke();
    }

    public void SetRetryEnabled(bool enabled)
    {
        if (isRetryEnabled == enabled)
            return;

        isRetryEnabled = enabled;
        OnRetryChanged?.Invoke();
    }
    public bool AddRecipe(string recipeId) // 09.21 추가: 레시피 중복 지급 방지 new
    {
        if (string.IsNullOrWhiteSpace(recipeId))
            return false;

        if (ownedRecipes == null)
            ownedRecipes = new List<OwnedRecipeData>();

        // 이미 보유한 레시피인지 확인
        OwnedRecipeData recipe = ownedRecipes.Find(
            r => r != null && r.recipeId == recipeId
        );

        // 이미 보유 중이면 중복 지급하지 않음
        if (recipe != null)
        {
            recipe.count = 1;
            return false;
        }

        // 처음 획득한 레시피만 추가
        ownedRecipes.Add(new OwnedRecipeData
        {
            recipeId = recipeId,
            count = 1
        });

        return true;
    }
    /// <summary>
    /// 특정 레시피 보유 여부 확인
    /// </summary>
    public bool HasRecipe(string recipeId)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
            return false;

        if (ownedRecipes == null)
            return false;

        return ownedRecipes.Exists(
            recipe =>
                recipe != null &&
                recipe.recipeId == recipeId &&
                recipe.count > 0
        );
    }
}