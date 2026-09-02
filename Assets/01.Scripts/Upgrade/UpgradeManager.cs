using System;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    public event Action<UpgradeData, int> OnUpgradePurchased;

    //업그레이드 비용 계산 메서드
    public double GetUpgradeCost(UpgradeData data, int currentLevel)
    {
        // currentLevel은 1레벨 기준 & 보통 0부터 시작하는 지수 계산에 맞추기 위해 currentLevel - 1 사용
        int levelIndex = Mathf.Max(0, currentLevel - 1);
        return data.baseCost * Math.Pow(data.costMultiplier, levelIndex);
    }

    //업그레이드 시도 메서드
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

            OnUpgradePurchased?.Invoke(data, updatedLevel);

            Debug.Log($"{data.upgradeName} 업그레이드 완료! 현재 레벨: {updatedLevel}, 남은 골드: {playerData.gold}");
        }
        else
        {
            Debug.Log($"{data.upgradeName} 업그레이드 실패");
        }
    }

    // 데이터 타입에 따라 현재 플레이어의 레벨을 가져오는 메서드
    public int GetCurrentLevel(UpgradeData data, PlayerData playerData)
    {
        switch (data.type)
        {
            case UpgradeType.WeaponPower:
                return playerData.weaponLevel;
            case UpgradeType.RestaurantExpansion:
                return playerData.restaurantLevel;
            default:
                return 1;
        }
    }

    // 데이터 타입에 따라 레벨을 1 올려주는 메서드
    private void SetNextLevel(UpgradeData data, PlayerData playerData)
    {
        switch (data.type)
        {
            case UpgradeType.WeaponPower:
                playerData.weaponLevel++;
                break;
            case UpgradeType.RestaurantExpansion:
                playerData.restaurantLevel++;
                break;
        }
    }
}
