using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class OwnedWeaponData
{
    public string weaponId;
    public int count = 1;
    public int level = 1;
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
    public bool isRetryEnabled;

    // 성장 요소 (기존)
    public int weaponLevel = 1;         // 이전 저장 데이터 이관 전용. 신규 강화는 ownedWeapons의 level 사용.
    public string equippedWeaponId;     // 비어 있는 기존 저장 데이터는 기본 무기를 장착한다.
    public List<OwnedWeaponData> ownedWeapons = new List<OwnedWeaponData>();
    public int weaponInventoryVersion;
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
