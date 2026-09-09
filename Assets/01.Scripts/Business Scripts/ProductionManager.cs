using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProductionManager : MonoBehaviour
{
    public static ProductionManager instance;

    public MakeFood[] chefs;
    public GameObject[] chefsSlider;
    public Food[] foods;

    [Header("요리할 등급 선택")]
    [SerializeField] private TextMeshProUGUI selectRarityText; // 선택 등급 표시 (선택)
    public int SelectedRarity { get; private set; } = 0;       // 구 nowSelectRarity

    Queue<Customer> orderQueue = new Queue<Customer>();
    bool chef1Cooking = false;
    bool chef2Cooking = false;
    bool chef3Cooking = false;
    bool chef4Cooking = false;

    int[] foodStartIndex = { 0, 3, 6, 8 };

    public int[] Recipes { get; set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        int cookCatNum = FacilityManager.instance.CookCatNum;
        for (int i = 1; i <= cookCatNum; i++)
        {
            chefs[i].transform.parent.gameObject.SetActive(true);
            chefsSlider[i].SetActive(true);
            StartChef(i);
        }
        Recipes = new int[4];
        for(int i = 0; i < Recipes.Length; i++)
        {
            if(Recipes[i] > 1)
                Recipes[i] = 1;

            PlayerPrefs.GetInt($"레시피{i}", 0);
        }
    }

    // 등급 선택 버튼 → 여기로 재연결 (구 FishInventoryManager.SelectXxx)
    public void SelectCommon() => SetSelected(0, "Common");
    public void SelectRare() => SetSelected(1, "Rare");
    public void SelectUnique() => SetSelected(2, "Unique");
    public void SelectEpic() => SetSelected(3, "Epic");

    private void SetSelected(int rarity, string label)
    {
        SelectedRarity = rarity;
        if (selectRarityText != null) selectRarityText.text = label;
    }

    public void OrderFood(Customer customer)
    {
        orderQueue.Enqueue(customer);   // UpdateAllText 호출 제거 (FishCountView가 자동 갱신)

        for (int i = 0; i < Recipes.Length; i++)
        {
            if (Recipes[i] > 1)
                Recipes[i] = 1;
        }

        if (!chef1Cooking)
            StartCoroutine(CookQueue(chefs[0], 1));

        if (FacilityManager.instance.CookCatNum >= 1 && !chef2Cooking)
            StartCoroutine(CookQueue(chefs[1], 2));
        if (FacilityManager.instance.CookCatNum >= 2 && !chef3Cooking)
            StartCoroutine(CookQueue(chefs[2], 3));
        if (FacilityManager.instance.CookCatNum >= 3 && !chef4Cooking)
            StartCoroutine(CookQueue(chefs[3], 4));
    }

    IEnumerator CookQueue(MakeFood chef, int chefNumber)
    {
        if (chefNumber == 1) chef1Cooking = true;
        if (chefNumber == 2) chef2Cooking = true;
        if (chefNumber == 3) chef3Cooking = true;
        if (chefNumber == 4) chef4Cooking = true;

        while (true)
        {
            if (orderQueue.Count == 0) break;

            int usedFishCount = 0;
            int cookRarity = 0;

            while (usedFishCount == 0)
            {
                usedFishCount = UseFish(SelectedRarity, out cookRarity);

                if (usedFishCount == 0)
                {
                    yield return null;
                    if (orderQueue.Count == 0) break;
                }
            }

            if (orderQueue.Count == 0) break;
            if (usedFishCount == 0) continue;

            Customer customer = orderQueue.Dequeue();

            int foodIndex = foodStartIndex[cookRarity] + usedFishCount - 1;
            if (foodIndex < 0 || foodIndex >= foods.Length) continue;

            Food food = foods[foodIndex];
            if (food == null) { Debug.Log("조건에 맞는 음식 없음"); continue; }
            if (customer == null) { Debug.Log("Customer가 null"); continue; }
            if (customer.mySeat == null) { Debug.Log("Customer 자리 없음"); continue; }

            customer.eatTime = food.eatTime;
            Vector3 foodPosition = customer.mySeat.transform.GetChild(0).position;

            yield return StartCoroutine(chef.StartCook(food, customer, foodPosition));
        }

        if (chefNumber == 1) chef1Cooking = false;
        else if (chefNumber == 2) chef2Cooking = false;
        else if (chefNumber == 3) chef3Cooking = false;
        else if (chefNumber == 4) chef4Cooking = false;

        if (orderQueue.Count > 0)
        {
            if (!chef1Cooking) StartCoroutine(CookQueue(chefs[0], 1));
            if (!chef2Cooking) StartCoroutine(CookQueue(chefs[1], 2));
            if (!chef3Cooking) StartCoroutine(CookQueue(chefs[2], 3));
            if (!chef4Cooking) StartCoroutine(CookQueue(chefs[3], 4));
        }
    }

    // 선택 등급부터 하위로 폴백하며 maxUse만큼 소모. 실제 소모 마리수 반환.
    int UseFish(int rarity, out int usedRarity)
    {
        usedRarity = -1;

        for (int j = rarity; j >= 0; j--)
        {
            int chefLevel = FacilityManager.instance.ChefCatLevel;
            //int maxUse = (j == 0 || j == 1) ? Mathf.Clamp(chefLevel,1,3) : (j == 2 ? 2 : 1);
            int maxUse = 0;

            if (j == 0)
            {
                maxUse = Mathf.Clamp(chefLevel, 1, 2 + Recipes[0]);    //셰프 레벨에 따라서 최소 1마리 ~ 3마리 ( 2 + 레시피해금 );
            }
            else if (j == 1 && chefLevel > 3)
            {
                maxUse = Mathf.Clamp(chefLevel - 3, 1, 2 + Recipes[1]);
            }
            else if (j == 2 && chefLevel > 6)
            {
                maxUse = Mathf.Clamp(chefLevel - 6, 1, 1 + Recipes[2]);
            }
            else if (j == 3 && chefLevel > 8)
            {
                maxUse = Recipes[3];
            }

            int used = CurrencyManager.instance.SpendFromGrade((FishGrade)j, maxUse);
            if (used > 0)
            {
                usedRarity = j;
                return used;   // 표시 갱신은 SpendFromGrade가 OnFishChanged로 처리
            }
        }
        return 0;
    }


    public void StartChef(int cookCatNUm) //FacilityManager.instance.CookCatNum
    {
        chefs[cookCatNUm].transform.parent.gameObject.SetActive(true);
        chefsSlider[cookCatNUm].SetActive(true);

        StartCoroutine(CookQueue(chefs[cookCatNUm], cookCatNUm + 1));
    }

    public void ChefPosition()
    {
        if (FacilityManager.instance.RestaurantLevel == 2)
        {
            chefs[0].transform.parent.localScale = new Vector3(.8f, .8f, .8f);
            chefs[1].transform.parent.localScale = new Vector3(.8f, .8f, .8f);

            chefs[0].transform.parent.position = new Vector3(-0.5f, 0.7f);
            chefs[1].transform.parent.position = new Vector3(0.7f, 0.7f);

            chefsSlider[0].transform.localPosition = new Vector3(-200, 390);
            chefsSlider[1].transform.localPosition = new Vector3(167, 390);
        }
        else if (FacilityManager.instance.RestaurantLevel == 3)
        {
            chefs[0].transform.parent.localScale = new Vector3(.6f, .6f, .6f);
            chefs[1].transform.parent.localScale = new Vector3(.6f, .6f, .6f);
            chefs[2].transform.parent.localScale = new Vector3(.6f, .6f, .6f);

            chefs[0].transform.parent.position = new Vector3(-0.6f, 0.3f);
            chefs[1].transform.parent.position = new Vector3(1f, 0.3f);
            chefs[2].transform.parent.position = new Vector3(-1.5f, 0.35f);

            chefsSlider[0].transform.localPosition = new Vector3(-219, 212);
            chefsSlider[1].transform.localPosition = new Vector3(271, 221);
            chefsSlider[2].transform.localPosition = new Vector3(-512, 234);
        }
    }

    public void CancelCook()   //식당 레벨업시 음식 캔슬
    {
        StopAllCoroutines();
        int cookCatNum = FacilityManager.instance.CookCatNum;
        for (int i = 0; i <= cookCatNum; i++)
            chefs[i].CancelCook();
        orderQueue.Clear();
        for (int i = 0; i <= cookCatNum; i++)
            StartCoroutine(CookQueue(chefs[i], i + 1));
    }

    private void OnApplicationQuit()
    {
        for (int i = 0; i < Recipes.Length; i++)
        {
            if (Recipes[i] > 1)
                Recipes[i] = 1;

            PlayerPrefs.SetInt($"레시피{i}", Recipes[i]);
        }
    }
}