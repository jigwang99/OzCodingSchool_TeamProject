using System;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    public event Action<UpgradeData, int> OnUpgradePurchased;

    // 업그레이드 비용 계산
    public double GetUpgradeCost(UpgradeData data, int currentLevel)
    {
        int levelIndex = Mathf.Max(0, currentLevel - 1);
        return data.baseCost * Math.Pow(data.costMultiplier, levelIndex);
    }

    // 업그레이드 시도
    public void TryUpgrade(UpgradeData data, PlayerData playerData)
    {
        int currentLevel = GetCurrentLevel(data, playerData);

        if (currentLevel >= data.maxLevel)
        {
            Debug.Log($"{data.upgradeName} 최대 레벨 도달!");
            return;
        }

        double cost = GetUpgradeCost(data, currentLevel);

        if (CurrencyManager.instance.SpendGold((int)cost))
        {
            SetNextLevel(data, playerData);
            int updatedLevel = GetCurrentLevel(data, playerData);

            // 효과 반영은 각 시스템의 바인더가 이 이벤트를 구독해 처리
            OnUpgradePurchased?.Invoke(data, updatedLevel);

            Debug.Log($"{data.upgradeName} 업그레이드 완료! 레벨: {updatedLevel}, 남은 골드: {playerData.gold}");
        }
        else
        {
            Debug.Log($"{data.upgradeName} 업그레이드 실패");
        }
    }

    // 타입별 현재 레벨 조회
    public int GetCurrentLevel(UpgradeData data, PlayerData playerData)
    {
        switch (data.type)
        {
            case UpgradeType.WeaponPower: return playerData.weaponLevel;
            case UpgradeType.FishDropRate: return playerData.fishDropRateLevel;
            case UpgradeType.RestaurantExpansion: return playerData.restaurantLevel;
            default: return 1;
        }
    }

    // 타입별 레벨 +1
    private void SetNextLevel(UpgradeData data, PlayerData playerData)
    {
        switch (data.type)
        {
            case UpgradeType.WeaponPower: playerData.weaponLevel++; break;
            case UpgradeType.FishDropRate: playerData.fishDropRateLevel++; break;
            case UpgradeType.RestaurantExpansion: playerData.restaurantLevel++; break;
        }
    }
}