using System;
using UnityEngine;

public class UpgradeManager : Singleton<UpgradeManager>
{
    public event Action<UpgradeData, int> OnUpgradePurchased;

    // 업그레이드 비용 계산
    public BigNumber GetUpgradeCost(UpgradeData data, int currentLevel)
    {
        int levelIndex = Mathf.Max(0, currentLevel - 1);
        return new BigNumber(data.baseCost * Math.Pow(data.costMultiplier, levelIndex));
    }

    // 업그레이드 시도
    public void TryUpgrade(UpgradeData data, PlayerData playerData)
    {
        if (data == null || playerData == null || playerData != GameManager.instance.PlayerData)
            return;

        // 결제 시작 시 대상 무기를 고정한다. 골드 변경 이벤트에서 장비가 바뀌어도 대상이 바뀌지 않는다.
        string weaponId = playerData.equippedWeaponId;

        if (data.type == UpgradeType.WeaponPower && !playerData.OwnsWeapon(weaponId))
            return;

        int currentLevel = GetCurrentLevel(data, playerData);

        if (currentLevel >= data.maxLevel)
        {
            Debug.Log($"{data.upgradeName} 최대 레벨 도달!");
            return;
        }

        BigNumber cost = GetUpgradeCost(data, currentLevel);

        if (CurrencyManager.instance.SpendGold(cost))
        {
            if (data.type == UpgradeType.WeaponPower)
                playerData.TryIncreaseWeaponLevel(weaponId, data.maxLevel);
            else
                SetNextLevel(data, playerData);
            int updatedLevel = data.type == UpgradeType.WeaponPower
                ? playerData.GetWeaponLevel(weaponId)
                : GetCurrentLevel(data, playerData);

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
            case UpgradeType.WeaponPower: return playerData.GetWeaponLevel(playerData.equippedWeaponId);
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
            case UpgradeType.FishDropRate: playerData.fishDropRateLevel++; break;
            case UpgradeType.RestaurantExpansion: playerData.restaurantLevel++; break;
        }
    }
}