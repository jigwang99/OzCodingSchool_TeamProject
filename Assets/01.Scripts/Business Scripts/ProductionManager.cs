using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductionManager : MonoBehaviour
{
    public static ProductionManager instance;

    public MakeFood chef1;
    public MakeFood chef2;
    public Food[] foods;

    [Header("요리할 등급 선택")]
    [SerializeField] private TextMeshProUGUI selectRarityText; // 선택 등급 표시 (선택)
    public int SelectedRarity { get; private set; } = 0;       // 구 nowSelectRarity

    Queue<Customer> orderQueue = new Queue<Customer>();
    bool chef1Cooking = false;
    bool chef2Cooking = false;

    int[] foodStartIndex = { 0, 3, 6, 8 };

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
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

        if (!chef1Cooking)
            StartCoroutine(CookQueue(chef1, 1));

        if (FacilityManager.instance.makeDouble == 1 && !chef2Cooking)
            StartCoroutine(CookQueue(chef2, 2));
    }

    IEnumerator CookQueue(MakeFood chef, int chefNumber)
    {
        if (chefNumber == 1) chef1Cooking = true;
        else chef2Cooking = true;

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
        else chef2Cooking = false;

        if (orderQueue.Count > 0)
        {
            if (!chef1Cooking) StartCoroutine(CookQueue(chef1, 1));
            if (!chef2Cooking) StartCoroutine(CookQueue(chef2, 2));
        }
    }

    // 선택 등급부터 하위로 폴백하며 maxUse만큼 소모. 실제 소모 마리수 반환.
    int UseFish(int rarity, out int usedRarity)
    {
        usedRarity = -1;

        for (int j = rarity; j >= 0; j--)
        {
            int maxUse = (j == 0 || j == 1) ? 3 : (j == 2 ? 2 : 1);

            int used = CurrencyManager.instance.SpendFromGrade((FishGrade)j, maxUse);
            if (used > 0)
            {
                usedRarity = j;
                return used;   // 표시 갱신은 SpendFromGrade가 OnFishChanged로 처리
            }
        }
        return 0;
    }

    public void StartChef2()
    {
        if (FacilityManager.instance.makeDouble == 1 && !chef2Cooking && orderQueue.Count > 0)
            StartCoroutine(CookQueue(chef2, 2));
    }
}