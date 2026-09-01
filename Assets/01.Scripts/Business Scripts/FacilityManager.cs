using TMPro;
using UnityEngine;

public class FacilityManager : MonoBehaviour //시설 업그레이드, 가구 배치, 직원 고용, 
{
    //경영 요리고양이 1마리(+ 직원 고양이 업그레이드)

    //- 요리고양이 업그레이드(요리 해금)
    //- 직원고용(초당 골드 생산량 증가)
    //- 가게 크기 증가(구멍가게, 일반 상가, 큰 식당)
    //→ 가구 배치 칸 증가(ex 구멍가게→가구 배치 칸 1개),
    //직원고용 업그레이드 해금조건,
    //요리고양이 업그레이드 해금조건(ex 가계 2단계 이상시 직원 업그레이드 5레벨가능)
    //- 가게를 업그레이드 할 경우 다양한 손님 등장(특별한 손님 등장시 n초간 골드생산량 증가)

    public static FacilityManager instance;

    public int ChefCatLevel { get; set; }   //셰프고양이
    public int CookCatNum { get; set; }      //직원고양이
    public int RestaurantLevel { get; set; } //식당 레벨
    public int[] FoodMachine { get; set; }   //가구

    int Gold;
    public TextMeshProUGUI goldText;

    public float makeSpeed;
    public float goldBonus;
    public float specialChance;
    public float makeDouble;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        //ChefCatLevel = PlayerPrefs.GetInt("chefCatLevel", 1);
        //CookCatNum = PlayerPrefs.GetInt("cookCatNum", 0);
        //RestaurantLevel = PlayerPrefs.GetInt("restaurantLevel", 1);

        //FoodMachine = new int[5];
        //for (int i = 0; i < 5; i++)
        //{
        //    FoodMachine[i] = PlayerPrefs.GetInt($"foodMachine{i}", 0);
        //}
    }

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
        makeDouble += 1;
        ProductionManager.instance.StartChef2();    
    }
    public void GetGold(int foodPrice, bool special)
    {
        if (Random.Range(0f,1f) < FacilityManager.instance.specialChance)
        {
            Debug.Log("스페셜 성공!");
            special = true;
        }

        int addGold = (int)(foodPrice * (1f + FacilityManager.instance.goldBonus));

        Gold += addGold;
        if (special)
            Gold += addGold;

        goldText.text = Gold.ToString();
    }
}
