using System;
using UnityEngine;

public class CurrencyManager : Singleton<CurrencyManager>
{
    private PlayerData data => GameManager.instance.PlayerData;

    public event Action<FishGrade> OnFishChanged;
    public event Action OnGoldChanged;

    protected override void Awake()
    {
        // 방치 보상은 전투 이외의 모든 씬에서 지급될 수 있으므로 재화 게이트도 유지한다.
        isDontDestroy = true;
        base.Awake();
    }

    //골드 획득
    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        data.gold += amount;

        Debug.Log($"[CurrencyManager] 골드 획득: +{amount} / 현재 골드: {data.gold}");

        OnGoldChanged?.Invoke();
    }

    //골드 소비
    public bool SpendGold(int amount)
    {
        if (amount <= 0)
            return false;

        if (data.gold < amount)
        {
            Debug.Log("[CurrencyManager] 골드가 부족합니다!");
            return false;
        }

        data.gold -= amount;

        Debug.Log($"[CurrencyManager] 골드 소비: -{amount} / 잔여 골드: {data.gold}");

        OnGoldChanged?.Invoke();

        return true;
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

        Debug.Log($"[CurrencyManager] {grade} / 종 {species} 물고기 획득: +{count} / 현재: {fishArray[species]}");

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
        {
            Debug.Log($"[CurrencyManager] {grade} / 종 {species} 물고기가 부족합니다!");

            return false;
        }

        fishArray[species] -= count;

        Debug.Log($"[CurrencyManager] {grade} / 종 {species} 물고기 소비: -{count} / 잔여: {fishArray[species]}");

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
            Debug.Log($"[CurrencyManager] {grade} 물고기 소비: -{usedCount}마리");

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
