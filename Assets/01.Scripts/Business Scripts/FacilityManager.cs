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

    private void Awake()
    {
        gasstove = PlayerPrefs.GetInt("가스레인지", 0);
        microwaveOven = PlayerPrefs.GetInt("전자레인지", 0);
        steamer = PlayerPrefs.GetInt("찜기", 0);
        deepfryer = PlayerPrefs.GetInt("튀김기", 0);
        refrigerator = PlayerPrefs.GetInt("냉장고", 0);
        oven = PlayerPrefs.GetInt("오븐", 0);
        CookCatNum = PlayerPrefs.GetInt("직원", 0);
        RestaurantLevel = PlayerPrefs.GetInt("식당", 1);
        ChefCatLevel = PlayerPrefs.GetInt("요리사레벨", 0);
        if (gasstove == 1) gasstoveBtn.interactable = false;
        if (microwaveOven == 1) microwaveOvenBtn.interactable = false;
        if (steamer == 1) steamerBtn.interactable = false;
        if (deepfryer == 1) deepfryerBtn.interactable = false;
        if (refrigerator == 1) refrigeratorBtn.interactable = false;
        if (oven == 1) ovenBtn.interactable = false;
        if (CookCatNum == 1) chefBtn.interactable = false;
        if (RestaurantLevel >= 3) { RestaurantLevel = 3; restaurantUpgradeBtn.interactable = false; }
        if (ChefCatLevel >= 9) { ChefCatLevel = 9; chefLevelBtn.interactable = false; }
    }

    private void Start()
    {
        chefLevelText.text = $"Chef Level : {ChefCatLevel}";
        UpgradeRestaurant();
    }
    public void OnClickGasstoveBtn(Button btn)
    {
        MakeSpeed += 0.1f;
        btn.interactable = false;

        gasstove = 1;
        PlayerPrefs.SetInt("가스레인지", gasstove);
    }

    public void OnClickMicrowaveovenBtn(Button btn)
    {
        GoldBonus += 0.15f;
        btn.interactable = false;

        microwaveOven = 1;
        PlayerPrefs.SetInt("전자레인지", microwaveOven);
    }
    public void OnClickSteamerBtn(Button btn)
    {
        MakeSpeed += 0.05f;
        GoldBonus += 0.05f;
        btn.interactable = false;

        steamer = 1;
        PlayerPrefs.SetInt("찜기", steamer);
    }
    public void OnClickDeepfryerBtn(Button btn)
    {
        MakeSpeed += 0.2f;
        GoldBonus += 0.05f;
        btn.interactable = false;

        deepfryer = 1;
        PlayerPrefs.SetInt("튀김기", deepfryer);
    }
    public void OnClickRefrigeratorBtn(Button btn)
    {
        NoUseFishChance += 0.05f;
        btn.interactable = false;

        refrigerator = 1;
        PlayerPrefs.SetInt("냉장고", refrigerator);
    }
    public void OnClickOvenBtn(Button btn)
    {
        SpecialChance += 0.3f;
        btn.interactable = false;

        oven = 1;
        PlayerPrefs.SetInt("오븐", oven);
    }
    public void OnClickCookerBtn(Button btn)
    {
        CookCatNum += 1;
        btn.interactable = false;
        if (RestaurantLevel > CookCatNum) //식당 1레벨 -> 직원 1명 가능 , 2 > 1 가능
            btn.interactable = true;

        for (int i = 1; i <= CookCatNum; i++)
        {
            ProductionManager.instance.StartChef(i);
        }

        PlayerPrefs.SetInt("직원", CookCatNum);
    }

    public void OnClickRestaurantBtn(Button btn)
    {
        RestaurantLevel += 1;

        if (RestaurantLevel > 3)
            RestaurantLevel = 3;

        if (RestaurantLevel == 3)
            btn.interactable = false;

        ProductionManager.instance.CancelCook();

        UpgradeRestaurant();

        CustomerSpawn.instance.waitingCustomers.Clear();
        BObjectPoolManager.instance.Refresh();

        PlayerPrefs.SetInt("식당", RestaurantLevel);
    }

    public void OnClickChefLevelUpBtn(Button btn)
    {
        ChefCatLevel += 1;

        if (ChefCatLevel == 9)
            btn.interactable = false;

        chefLevelText.text = $"Chef Level : {ChefCatLevel}";
        PlayerPrefs.SetInt("요리사레벨", ChefCatLevel);
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

            chefBtn.interactable = true;
        }
        else if (RestaurantLevel == 3)
        {
            restaurants[0].gameObject.SetActive(false);
            restaurants[1].gameObject.SetActive(false);
            restaurants[2].gameObject.SetActive(true);
            restaurants[2].GetComponent<RestaurantPosition>().seats.ResetSeats();

            chefBtn.interactable = true;
        }
        ProductionManager.instance.ChefPosition();
    }
}
