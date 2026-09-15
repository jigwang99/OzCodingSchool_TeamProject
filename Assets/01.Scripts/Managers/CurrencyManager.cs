
using System;
using UnityEngine;
using PixelRestaurant.Gacha;

public class CurrencyManager : Singleton<CurrencyManager>, ICurrencyProvider
{
    private PlayerData data => GameManager.instance.PlayerData;

    public event Action<FishGrade> OnFishChanged;
    public event Action OnGoldChanged;

    protected override void Awake()
    {
        base.Awake();
    }

    // =========================
    // Gold
    // =========================

    /// <summary>
    /// 골드 획득
    /// </summary>
    public void AddGold(BigNumber amount)
    {
        if (amount.IsZeroOrNegative)
            return;

        data.gold += amount;

        Debug.Log(
            $"[CurrencyManager] 골드 획득: +{amount} / 현재 골드: {data.gold}"
        );

        OnGoldChanged?.Invoke();
    }

    /// <summary>
    /// 골드 소비
    /// </summary>
    public bool SpendGold(BigNumber amount)
    {
        if (amount.IsZeroOrNegative)
            return false;

        if (data.gold < amount)
        {
            Debug.Log("[CurrencyManager] 골드가 부족합니다!");
            return false;
        }

        data.gold -= amount;

        Debug.Log(
            $"[CurrencyManager] 골드 소비: -{amount} / 잔여 골드: {data.gold}"
        );

        OnGoldChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// 현재 골드 조회
    /// </summary>
    public BigNumber GetCurrentGold()
    {
        return data.gold;
    }

    // =========================
    // Fish
    // =========================

    /// <summary>
    /// 특정 종의 물고기 획득
    /// </summary>
    public void AddFish(FishGrade grade, int species, int count)
    {
        if (count <= 0)
            return;

        int[] fishArray = data.GetFishArray(grade);

        if (!IsValidSpecies(fishArray, species))
            return;

        fishArray[species] += count;

        Debug.Log(
            $"[CurrencyManager] {grade} / 종 {species} 물고기 획득: +{count} / 현재: {fishArray[species]}"
        );

        OnFishChanged?.Invoke(grade);
    }

    /// <summary>
    /// 특정 종의 물고기 수량 조회
    /// </summary>
    public int GetFish(FishGrade grade, int species)
    {
        int[] fishArray = data.GetFishArray(grade);

        if (!IsValidSpecies(fishArray, species))
            return 0;

        return fishArray[species];
    }

    /// <summary>
    /// 특정 종의 물고기 소비
    /// </summary>
    public bool SpendFish(FishGrade grade, int species, int count)
    {
        if (count <= 0)
            return false;

        int[] fishArray = data.GetFishArray(grade);

        if (!IsValidSpecies(fishArray, species))
            return false;

        if (fishArray[species] < count)
        {
            Debug.Log(
                $"[CurrencyManager] {grade} / 종 {species} 물고기가 부족합니다!"
            );

            return false;
        }

        if (FacilityManager.instance.NoUseFishChance
            < UnityEngine.Random.Range(0f, 1f))
        {
            fishArray[species] -= count;
        }

        Debug.Log(
            $"[CurrencyManager] {grade} / 종 {species} 물고기 소비: -{count} / 잔여: {fishArray[species]}"
        );

        OnFishChanged?.Invoke(grade);

        return true;
    }

    /// <summary>
    /// 해당 등급의 전체 물고기 수량
    /// </summary>
    public int GetGradeTotal(FishGrade grade)
    {
        int[] fishArray = data.GetFishArray(grade);

        if (fishArray == null)
            return 0;

        int total = 0;

        for (int i = 0; i < fishArray.Length; i++)
        {
            total += fishArray[i];
        }

        return total;
    }

    /// <summary>
    /// 해당 등급의 물고기 소비
    /// </summary>
    public int SpendFromGrade(FishGrade grade, int count)
    {
        if (count <= 0)
            return 0;

        int total = GetGradeTotal(grade);

        if (total < count)
        {
            Debug.Log(
                $"[CurrencyManager] {grade} 등급 물고기가 부족합니다!"
            );

            return 0;
        }

        int[] fishArray = data.GetFishArray(grade);

        int remaining = count;

        for (int i = 0; i < fishArray.Length && remaining > 0; i++)
        {
            int spend = Mathf.Min(fishArray[i], remaining);

            fishArray[i] -= spend;
            remaining -= spend;
        }

        OnFishChanged?.Invoke(grade);

        return count;
    }

    private bool IsValidSpecies(int[] fishArray, int species)
    {
        return fishArray != null
            && species >= 0
            && species < fishArray.Length;
    }
}