using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum FishRare
{
    Common,
    Rare,
    Unique,
    Epic,
}

public class ProductionManager : MonoBehaviour
{
    public static ProductionManager instance;

    public MakeFood chef1;
    public MakeFood chef2;

    public Food[] foods;

    Queue<Customer> orderQueue = new Queue<Customer>();

    bool chef1Cooking = false;
    bool chef2Cooking = false;

    int[] foodStartIndex = { 0, 3, 6, 8 };

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void OrderFood(Customer customer)
    {
        FishInventoryManager.instance.UpdateAllText();

        orderQueue.Enqueue(customer);

        // 1번 요리사 비어있으면 시작
        if (!chef1Cooking)
            StartCoroutine(CookQueue(chef1, 1));

        // 2번 요리사 비어있으면 시작
        if (FacilityManager.instance.makeDouble == 1)
            if (!chef2Cooking)
                StartCoroutine(CookQueue(chef2, 2));
    }

    IEnumerator CookQueue(MakeFood chef, int chefNumber)
    {
        if (chefNumber == 1)
            chef1Cooking = true;
        else
            chef2Cooking = true;

        while (true)
        {
            // 주문 없으면 종료
            if (orderQueue.Count == 0)
                break;

            int usedFishCount = 0;
            int cookRarity = 0;

            // 물고기가 생길 때까지 기다림
            while (usedFishCount == 0)
            {
                int selectRarity = FishInventoryManager.instance.nowSelectRarity;

                usedFishCount = UseFish(selectRarity, out cookRarity);

                if (usedFishCount == 0)
                {
                    yield return null;

                    // 기다리는 동안 다른 요리사가
                    // 주문을 전부 처리했을 수도 있음
                    if (orderQueue.Count == 0)
                        break;
                }
            }

            if (orderQueue.Count == 0)
                break;

            if (usedFishCount == 0)
                continue;

            Customer customer = orderQueue.Dequeue();

            int foodIndex =
                foodStartIndex[cookRarity] + usedFishCount - 1;

            if (foodIndex < 0 || foodIndex >= foods.Length)
            {
                continue;
            }

            Food food = foods[foodIndex];

            if (food == null)
            {
                Debug.Log("조건에 맞는 음식 없음");
                continue;
            }

            if (customer == null)
            {
                Debug.Log("Customer가 null");
                continue;
            }

            if (customer.mySeat == null)
            {
                Debug.Log("Customer 자리 없음");
                continue;
            }

            customer.eatTime = food.eatTime;

            Vector3 foodPosition = customer.mySeat.transform.GetChild(0).position;

            // 이 요리사가 조리를 끝낼 때까지 기다림
            yield return StartCoroutine(chef.StartCook(food, customer, foodPosition));

            FishInventoryManager.instance.UpdateAllText();
        }

        if (chefNumber == 1)
            chef1Cooking = false;
        else
            chef2Cooking = false;

        if (orderQueue.Count > 0)
        {
            if (!chef1Cooking)
                StartCoroutine(CookQueue(chef1, 1));

            if (!chef2Cooking)
                StartCoroutine(CookQueue(chef2, 2));
        }

        FishInventoryManager.instance.UpdateAllText();
    }

    int UseFish(int rarity, out int usedRarity)
    {
        usedRarity = -1;

        for (int j = rarity; j >= 0; j--)
        {
            int[] fishes = FishInventoryManager.instance.fishNums[j];

            int maxUse;

            if (j == 0 || j == 1)
                maxUse = 3;
            else if (j == 2)
                maxUse = 2;
            else
                maxUse = 1;

            int usedCount = 0;

            for (int i = 0; i < fishes.Length; i++)
            {
                if (fishes[i] > 0)
                {
                    fishes[i]--;
                    usedCount++;

                    if (usedCount >= maxUse)
                        break;
                }
            }

            if (usedCount > 0)
            {
                // 공용 변수 사용 X
                // 이 요리사가 사용한 희귀도를 따로 반환
                usedRarity = j;

                FishInventoryManager.instance.UpdateAllText();

                return usedCount;
            }
        }

        return 0;
    }

    public void StartChef2()
    {
        if (FacilityManager.instance.makeDouble == 1)
        {
            if (!chef2Cooking && orderQueue.Count > 0)
            {
                StartCoroutine(CookQueue(chef2, 2));
            }
        }
    }
}