using UnityEngine;

public enum UpgradeType
{
    WeaponPower,
    RestaurantExpansion
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Scriptable Objects/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [Header("고유 정보")]
    public int id;

    [Header("UI에 표시될 업그레이드 이름")]
    public string upgradeName;

    [Header("업그레이드 카테고리")]
    public UpgradeType type;

    [Header("업그레이드 설정")]
    [Tooltip("레벨 1 -> 2 로 갈 때 드는 기본 비용")]
    //기본 비용 수정 필요
    public double baseCost = 100.0;

    [Tooltip("레벨이 오를 때마다 비용이 증가하는 계수 (현재: 1.15 = 15%씩 증가)")]
    //비용 계수 수정 필요
    public double costMultiplier = 1.15;

    [Tooltip("최대 도달 가능 레벨")]
    public int maxLevel = 3;
}
