using TMPro;
using UnityEngine;

public class FacilityManager : Singleton<FacilityManager> //시설 업그레이드, 가구 배치, 직원 고용, 
{
    //경영 요리고양이 1마리(+ 직원 고양이 업그레이드)

    //- 요리고양이 업그레이드(요리 해금)
    //- 직원고용(초당 골드 생산량 증가)
    //- 가게 크기 증가(구멍가게, 일반 상가, 큰 식당)
    //→ 가구 배치 칸 증가(ex 구멍가게→가구 배치 칸 1개),
    //직원고용 업그레이드 해금조건,
    //요리고양이 업그레이드 해금조건(ex 가계 2단계 이상시 직원 업그레이드 5레벨가능)
    //- 가게를 업그레이드 할 경우 다양한 손님 등장(특별한 손님 등장시 n초간 골드생산량 증가)

    private PlayerData data => GameManager.instance.PlayerData;

    // 저장 대상 상태 → PlayerData 프록시
    public int ChefCatLevel { get => data.chefCatLevel; set => data.chefCatLevel = value; }
    public int CookCatNum { get => data.cookCatNum; set => data.cookCatNum = value; }
    public int RestaurantLevel { get => data.restaurantLevel; set => data.restaurantLevel = value; }
    public int[] FoodMachine => data.foodMachine;

    // 업그레이드 효과값 → PlayerData 프록시 (기존 이름 유지 → 외부 호출부 안 깨짐)
    public float makeSpeed { get => data.makeSpeed; set => data.makeSpeed = value; }
    public float goldBonus { get => data.goldBonus; set => data.goldBonus = value; }
    public float specialChance { get => data.specialChance; set => data.specialChance = value; }
    public float makeDouble { get => data.makeDouble; set => data.makeDouble = value; }

    //ChefCatLevel = PlayerPrefs.GetInt("chefCatLevel", 1);
    //CookCatNum = PlayerPrefs.GetInt("cookCatNum", 0);
    //RestaurantLevel = PlayerPrefs.GetInt("restaurantLevel", 1);

    //FoodMachine = new int[5];
    //for (int i = 0; i < 5; i++)
    //{
    //    FoodMachine[i] = PlayerPrefs.GetInt($"foodMachine{i}", 0);
    //}

    //private void OnApplicationQuit()
    //{
    //    PlayerPrefs.SetInt("chefCatLevel", ChefCatLevel);
    //    PlayerPrefs.SetInt("cookCatNum", CookCatNum);
    //    PlayerPrefs.SetInt("restaurantLevel", RestaurantLevel);
    //    for (int i = 0; i < 5; i++)
    //    {
    //        PlayerPrefs.SetInt($"foodMachine{i}", FoodMachine[i]);
    //    }
    //}

    public void OnClickMakeFoodSpeedBtn()
    {
        makeSpeed += 0.1f;
    }

    public void OnClickGoldBonusBtn()
    {
        goldBonus += 0.2f;
    }
    public void OnClickSpecialChanceBtn()
    {
        specialChance += 0.3f;
    }

    public void OnClickMakeDoubleBtn()
    {
        data.makeDouble += 1;
        ProductionManager.instance.StartChef2();    
    }
    public void GetGold(int foodPrice, bool special)
    {
        if (Random.Range(0f, 1f) < specialChance)
        {
            Debug.Log("스페셜 성공!");
            special = true;
        }

        int addGold = (int)(foodPrice * (1f + goldBonus));
        if (special) addGold *= 2;                 // 기존 2배 로직 유지

        CurrencyManager.instance.AddGold(addGold);  // 로컬 누적 대신 게이트 통과
    }
}
