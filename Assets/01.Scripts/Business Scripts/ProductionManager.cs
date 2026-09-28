using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductionManager : MonoBehaviour
{
    public static ProductionManager instance;

    public MakeFood[] chefs;
    public GameObject[] chefsSlider;
    public Food[] foods;

    private const float ChefBarGap = 12f;
    private Transform[] chefBarAnchors;
    private Vector3[] chefBarLocalPositions;
    private Canvas chefBarCanvas;
    private readonly Vector3[] barCorners = new Vector3[4];

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
    }

    private void OnEnable() => Canvas.willRenderCanvases += PositionChefBars;

    private void OnDisable() => Canvas.willRenderCanvases -= PositionChefBars;

    private void Start()
    {
        chefBarAnchors = new Transform[chefs.Length];
        chefBarLocalPositions = new Vector3[chefs.Length];
        for (int i = 0; i < chefs.Length; i++)
        {
            SpriteRenderer spriteRenderer = chefs[i].GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null) continue;

            // Cache the initial sprite height relative to the placement root, which
            // is not animated. Sprite asset bounds also work for inactive hired chefs.
            Transform anchor = chefs[i].transform.parent;
            Bounds bounds = spriteRenderer.sprite.bounds;
            Vector3 head = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            chefBarAnchors[i] = anchor;
            chefBarLocalPositions[i] = anchor.InverseTransformPoint(
                spriteRenderer.transform.TransformPoint(head));
        }

        chefBarCanvas = chefsSlider[0].GetComponentInParent<Canvas>().rootCanvas;
    }

    private void PositionChefBars()
    {
        if (chefBarAnchors == null || chefBarCanvas == null) return;

        // Use the scene camera assigned to the canvas (it is not tagged MainCamera).
        Camera worldCamera = chefBarCanvas.worldCamera;
        if (worldCamera == null) return;
        Camera uiCamera = chefBarCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null : worldCamera;

        for (int i = 0; i < chefs.Length; i++)
        {
            if (!chefsSlider[i].activeInHierarchy || !chefs[i].gameObject.activeInHierarchy
                || chefBarAnchors[i] == null || chefs[i].makeTimeBar == null) continue;

            RectTransform bar = (RectTransform)chefsSlider[i].transform;
            RectTransform parent = (RectTransform)bar.parent;
            Vector3 head = chefBarAnchors[i].TransformPoint(chefBarLocalPositions[i]);
            Vector3 screenPoint = worldCamera.WorldToScreenPoint(head);
            if (screenPoint.z <= 0f || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, screenPoint, uiCamera, out Vector2 headPosition)) continue;

            // Align the visible slider's bottom, including its prefab offset and scale.
            RectTransform slider = (RectTransform)chefs[i].makeTimeBar.transform;
            slider.GetWorldCorners(barCorners);
            float bottom = float.PositiveInfinity;
            float centerX = 0f;
            for (int corner = 0; corner < barCorners.Length; corner++)
            {
                Vector3 localCorner = parent.InverseTransformPoint(barCorners[corner]);
                bottom = Mathf.Min(bottom, localCorner.y);
                centerX += localCorner.x / barCorners.Length;
            }

            Vector3 position = bar.localPosition;
            position.x += headPosition.x - centerX;
            position.y += headPosition.y + ChefBarGap - bottom;
            bar.localPosition = position;
        }
    }
    
    // 등급 선택 버튼 → 여기로 재연결 (구 FishInventoryManager.SelectXxx)
    public void SelectCommon() => SetSelected(0, "일반");
    public void SelectRare() => SetSelected(1, "희귀");
    public void SelectUnique() => SetSelected(2, "유니크");
    public void SelectEpic() => SetSelected(3, "에픽");

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
        bool alreadyCooking = chefNumber switch
        {
            1 => chef1Cooking,
            2 => chef2Cooking,
            3 => chef3Cooking,
            4 => chef4Cooking,
            _ => true
        };
        if (alreadyCooking) yield break;

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

            chefsSlider[0].transform.localScale = new Vector3(.8f, .8f, .8f);
            chefsSlider[1].transform.localScale = new Vector3(.8f, .8f, .8f);

        }
        else if (FacilityManager.instance.RestaurantLevel == 3)
        {
            chefs[0].transform.parent.localScale = new Vector3(.6f, .6f, .6f);
            chefs[1].transform.parent.localScale = new Vector3(.6f, .6f, .6f);
            chefs[2].transform.parent.localScale = new Vector3(.6f, .6f, .6f);

            chefs[0].transform.parent.position = new Vector3(-0.6f, 0.3f);
            chefs[1].transform.parent.position = new Vector3(1f, 0.3f);
            chefs[2].transform.parent.position = new Vector3(-1.5f, 0.35f);

            chefsSlider[0].transform.localScale = new Vector3(.6f, .6f, .6f);
            chefsSlider[1].transform.localScale = new Vector3(.6f, .6f, .6f);
            chefsSlider[2].transform.localScale = new Vector3(.6f, .6f, .6f);

        }
    }

    public void CancelCook()   //식당 레벨업시 음식 캔슬
    {
        StopAllCoroutines();
        chef1Cooking = chef2Cooking = chef3Cooking = chef4Cooking = false;
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
