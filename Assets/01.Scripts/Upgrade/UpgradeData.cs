using UnityEngine;

public enum UpgradeCategory
{
    Combat,
    Business
}

public enum UpgradeType
{
    WeaponPower,
    Health,
    FishDropRate,
    RestaurantExpansion,
    ChefCookingSkill
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Upgrade/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [Header("업그레이드 정보")]
    public int id;
    public string upgradeName;
    public UpgradeCategory category;
    public UpgradeType type;

    [Header("UI")]
    public Sprite icon;

    [Header("강화 설정")]
    public double baseCost = 100.0;
    public double costMultiplier = 1.15;
    public int maxLevel;

    [Header("드롭률 업그레이드 설정")]
    [Tooltip("FishDropRate 업그레이드 레벨당 증가하는 드롭률 배수")]
    public float dropChanceMultiplierPerLevel = 0.3f;

    public float GetDropChanceMultiplier(int level)
    {
        return 1f + Mathf.Max(0, level - 1) * dropChanceMultiplierPerLevel;
    }

    public float GetMaxHealth(int level)
    {
        return 100f + (level - 1) * 20f;
    }
}
