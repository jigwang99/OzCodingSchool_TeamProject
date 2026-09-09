using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class FacilityManager : Singleton<FacilityManager> //시설 업그레이드, 가구 배치, 직원 고용, 
{
    private PlayerData data => GameManager.instance.PlayerData;

    // 저장 대상 상태 → PlayerData 프록시
    public int ChefCatLevel { get => data.chefCatLevel; set => data.chefCatLevel = value; }
    public int CookCatNum { get => data.cookCatNum; set => data.cookCatNum = value; }
    public int RestaurantLevel { get => data.restaurantLevel; set => data.restaurantLevel = value; }
    public int[] FoodMachine => data.foodMachine;

    // 업그레이드 효과값 → PlayerData 프록시 (기존 이름 유지 → 외부 호출부 안 깨짐)
    public float MakeSpeed { get => data.MakeSpeed; set => data.MakeSpeed = value; }
    public float GoldBonus { get => data.GoldBonus; set => data.GoldBonus = value; }
    public float SpecialChance { get => data.SpecialChance; set => data.SpecialChance = value; }
    public float NoUseFishChance { get => data.NoUseFishChance; set => data.NoUseFishChance = value; }

    int Gold;
    public TextMeshProUGUI goldText;

    int gasstove;
    int microwaveOven;
    int steamer;
    int deepfryer;
    int refrigerator;
    int oven;

    public Button gasstoveBtn;
    public Button microwaveOvenBtn;
    public Button steamerBtn;
    public Button deepfryerBtn;
    public Button refrigeratorBtn;
    public Button ovenBtn;
    public Button chefBtn;
    public Button restaurantUpgradeBtn;
    public Button chefLevelBtn;

    public GameObject[] restaurants;

    public TextMeshProUGUI chefLevelText;

    private void Start()
    {
        gasstove = FoodMachine[0];
        microwaveOven = FoodMachine[1];
        steamer = FoodMachine[2];
        deepfryer = FoodMachine[3];
        refrigerator = FoodMachine[4];
        oven = FoodMachine[5];

        if (gasstove == 1) gasstoveBtn.interactable = false;
        if (microwaveOven == 1) microwaveOvenBtn.interactable = false;
        if (steamer == 1) steamerBtn.interactable = false;
        if (deepfryer == 1) deepfryerBtn.interactable = false;
        if (refrigerator == 1) refrigeratorBtn.interactable = false;
        if (oven == 1) ovenBtn.interactable = false;
        if (RestaurantLevel >= 3) { RestaurantLevel = 3; restaurantUpgradeBtn.interactable = false; }
        if (ChefCatLevel >= 9) { ChefCatLevel = 9; chefLevelBtn.interactable = false; }

        chefLevelText.text = $"Chef Level : {ChefCatLevel}";
        UpgradeRestaurant();

        // 식당 레벨만큼만 직원 고용 가능
        chefBtn.interactable = CookCatNum < RestaurantLevel;
    }
    public void OnClickGasstoveBtn(Button btn)
    {
        BigNumber price = new BigNumber(200);

        if (FoodMachine[0] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        MakeSpeed += 0.1f;
        btn.interactable = false;

        FoodMachine[0] = 1;
        SaveManager.instance.Save();
    }

    public void OnClickMicrowaveovenBtn(Button btn)
    {
        BigNumber price = new BigNumber(300);

        if (FoodMachine[1] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        GoldBonus += 0.15f;
        btn.interactable = false;

        FoodMachine[1] = 1;
        SaveManager.instance.Save();
    }
    public void OnClickSteamerBtn(Button btn)
    {
        BigNumber price = new BigNumber(400);

        if (FoodMachine[2] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        MakeSpeed += 0.05f;
        GoldBonus += 0.05f;
        btn.interactable = false;

        FoodMachine[2] = 1;
        SaveManager.instance.Save();
    }
    public void OnClickDeepfryerBtn(Button btn)
    {
        BigNumber price = new BigNumber(500);

        if (FoodMachine[3] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        MakeSpeed += 0.2f;
        GoldBonus += 0.05f;
        btn.interactable = false;

        FoodMachine[3] = 1;
        SaveManager.instance.Save();
    }
    public void OnClickRefrigeratorBtn(Button btn)
    {
        BigNumber price = new BigNumber(600);

        if (FoodMachine[4] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        NoUseFishChance += 0.05f;
        btn.interactable = false;

        FoodMachine[4] = 1;
        SaveManager.instance.Save();
    }
    public void OnClickOvenBtn(Button btn)
    {
        BigNumber price = new BigNumber(700);

        if (FoodMachine[5] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        SpecialChance += 0.3f;
        btn.interactable = false;

        FoodMachine[5] = 1;
        SaveManager.instance.Save();
    }
    public void OnClickCookerBtn(Button btn)
    {
        if (CookCatNum >= RestaurantLevel) return;

        int basePrice = 700;
        int currentPrice = basePrice * (CookCatNum + 1);

        if (!CurrencyManager.instance.SpendGold(new BigNumber(currentPrice))) return;

        CookCatNum += 1;
        btn.interactable = CookCatNum < RestaurantLevel;

        for (int i = 1; i <= CookCatNum; i++)
        {
            ProductionManager.instance.StartChef(i);
        }

        SaveManager.instance.Save();
    }

    public void OnClickRestaurantBtn(Button btn)
    {
        if (RestaurantLevel >= 3) return;

        int basePrice = 1000;
        int currentPrice = basePrice * (RestaurantLevel);

        if (!CurrencyManager.instance.SpendGold(new BigNumber(currentPrice))) return;

        RestaurantLevel += 1;

        if (RestaurantLevel > 3)
            RestaurantLevel = 3;

        if (RestaurantLevel == 3)
            btn.interactable = false;

        ProductionManager.instance.CancelCook();

        UpgradeRestaurant();

        CustomerSpawn.instance.waitingCustomers.Clear();
        BObjectPoolManager.instance.Refresh();

        SaveManager.instance.Save();
    }

    public void OnClickChefLevelUpBtn(Button btn)
    {
        if (ChefCatLevel >= 9) return;

        int basePrice = 300;
        int currentPrice = basePrice * (ChefCatLevel);

        if (!CurrencyManager.instance.SpendGold(new BigNumber(currentPrice))) return;

        ChefCatLevel += 1;

        if (ChefCatLevel >= 9)
        {
            ChefCatLevel = 9;
            btn.interactable = false;
        }

        chefLevelText.text = $"Chef Level : {ChefCatLevel}";
        SaveManager.instance.Save();
    }

    public void GetGold(int foodPrice, bool special)
    {
        //if (Random.Range(0f, 1f) < FacilityManager.instance.SpecialChance)    //손님에서 음식 다먹었을때 실행
        //{
        //    Debug.Log("스페셜 성공!");
        //    special = true;
        //}

        int addGold = (int)(foodPrice * (1f + FacilityManager.instance.GoldBonus));

        int goldAmount = addGold;

        if (special)
            goldAmount += addGold;

        CurrencyManager.instance.AddGold(new BigNumber(goldAmount));
    }

    public void UpgradeRestaurant()
    {
        if (RestaurantLevel == 2)
        {
            restaurants[0].gameObject.SetActive(false);
            restaurants[1].gameObject.SetActive(true);
            restaurants[2].gameObject.SetActive(false);
            restaurants[1].GetComponent<RestaurantPosition>().seats.ResetSeats();
        }
        else if (RestaurantLevel == 3)
        {
            restaurants[0].gameObject.SetActive(false);
            restaurants[1].gameObject.SetActive(false);
            restaurants[2].gameObject.SetActive(true);
            restaurants[2].GetComponent<RestaurantPosition>().seats.ResetSeats();
        }
        chefBtn.interactable = CookCatNum < RestaurantLevel;
        ProductionManager.instance.ChefPosition();
    }
}
