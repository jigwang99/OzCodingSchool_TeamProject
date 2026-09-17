using System;
using UnityEngine;
using PixelRestaurant.Gacha; // new

public class CurrencyManager : Singleton<CurrencyManager>, ICurrencyProvider // new , ICurrencyProvider 인터페이스 구현
{
    private PlayerData data => GameManager.instance.PlayerData;

    public event Action<FishGrade> OnFishChanged;
    public event Action OnGoldChanged;

    protected override void Awake()
    {
        base.Awake();
    }

    //골드 획득
    public void AddGold(BigNumber amount)
    {
        if (amount.IsZeroOrNegative)
            return;

        data.gold += amount;

        OnGoldChanged?.Invoke();
    }

    //골드 소비
    public bool SpendGold(BigNumber amount)
    {
        if (amount.IsZeroOrNegative)
            return false;

        if (data.gold < amount)
            return false;

        data.gold -= amount;

        OnGoldChanged?.Invoke();

        return true;
    }

    //new 
    //현재 골드 조회

    public BigNumber GetCurrentGold()
    {
        return data.gold;
    }

    //특정 종의 물고기 획득
    public void AddFish(FishGrade grade, int species, int count)
    {
        if (count <= 0)
            return;

        int[] fishArray = data.GetFishArray(grade);

        if (!IsValidSpecies(fishArray, species))
            return;

        fishArray[species] += count;

        OnFishChanged?.Invoke(grade);
    }

    //특정 종의 물고기 수량 조회
    public int GetFish(FishGrade grade, int species)
    {
        int[] fishArray = data.GetFishArray(grade);

        if (!IsValidSpecies(fishArray, species))
            return 0;

        return fishArray[species];
    }

    //특정 종의 물고기 소비
    public bool SpendFish(FishGrade grade, int species, int count)
    {
        if (count <= 0)
            return false;

        int[] fishArray = data.GetFishArray(grade);

        if (!IsValidSpecies(fishArray, species))
            return false;

        if (fishArray[species] < count)
            return false;

        if (FacilityManager.instance.NoUseFishChance < UnityEngine.Random.Range(0f, 1f))    //물고기 안쓰기 확률
        {
            fishArray[species] -= count;
        }

        OnFishChanged?.Invoke(grade);

        return true;
    }

    //해당 등급의 전체 물고기 수량
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

    // 해당 등급에서 물고기 소비
    // 종 번호가 낮은 것부터 소비
    public int SpendFromGrade(FishGrade grade, int count)
    {
        if (count <= 0)
            return 0;

        int[] fishArray = data.GetFishArray(grade);

        if (fishArray == null)
            return 0;

        int remaining = count;
        int usedCount = 0;

        for (int i = 0; i < fishArray.Length; i++)
        {
            if (remaining <= 0)
                break;

            int consumeAmount = Mathf.Min(fishArray[i], remaining);

            if (consumeAmount <= 0)
                continue;

            fishArray[i] -= consumeAmount;

            remaining -= consumeAmount;
            usedCount += consumeAmount;
        }

        if (usedCount > 0)
        {
            OnFishChanged?.Invoke(grade);
        }

        return usedCount;
    }

    private bool IsValidSpecies(int[] fishArray, int species)
    {
        return fishArray != null &&
               species >= 0 &&
               species < fishArray.Length;
    }
}