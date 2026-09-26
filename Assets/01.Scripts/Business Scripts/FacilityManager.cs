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
    public int[] FoodMachine2 => data.foodMachine2;
    public int[] MachineCount => data.MachineCount;

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

    public Button gasstoveBtn;  //가구 추가 1
    public Button microwaveOvenBtn;
    public Button steamerBtn;
    public Button deepfryerBtn;
    public Button refrigeratorBtn;
    public Button ovenBtn;
    public Button chefBtn;

    public GameObject[] restaurants;

    public TextMeshProUGUI chefLevelText;

    public TextMeshProUGUI infoText;

    public TextMeshProUGUI chefBuyText;

    public GameObject selectMachinesParentObj;   //가구 추가 2
    public Sprite[] selectMachinesImgs;
    public Image selectMachinesPopup;
    public TextMeshProUGUI selectMachinesText;
    Button[] selectMachinesObjs;
    GameObject nowSelectMachine;
    int nowSelectMachineNum;
    public TextMeshProUGUI[] selectMachineCountText;

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
        if (RestaurantLevel >= 3) { RestaurantLevel = 3;  }
        if (ChefCatLevel >= 9) { ChefCatLevel = 9; }

        chefLevelText.text = $"Chef Level : {ChefCatLevel}";
        selectMachinesObjs = selectMachinesParentObj.GetComponentsInChildren<Button>();
        UpgradeRestaurant();

        // 식당 레벨만큼만 직원 고용 가능
        chefBtn.interactable = CookCatNum < RestaurantLevel;
        InfoText();

        selectMachinesText.text = "";
        for (int i = 0; i < selectMachinesObjs.Length; i++)
        {
            if (FoodMachine2[i] != 0)   //저장 데이터가 0 이 아닐시 ( 가구 선택이 되있을시)
            {
                selectMachinesObjs[i].transform.GetChild(1).gameObject.SetActive(false);//+모양 비활성화
                selectMachinesObjs[i].transform.GetChild(2).gameObject.SetActive(true);//가구 이미지 활성화
                selectMachinesObjs[i].transform.GetChild(2).GetComponent<Image>().sprite = selectMachinesImgs[FoodMachine2[i] - 1];//숫자 이미지 추가
            }
        }
        for (int i = 0; i < selectMachineCountText.Length; i++)
            selectMachineCountText[i].text = MachineCount[i].ToString();
    }
    public void OnClickGasstoveBtn(Button btn)
    {
        BigNumber price = new BigNumber(3000);

        if (FoodMachine[0] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        MakeSpeed += 0.1f;
        btn.interactable = false;

        FoodMachine[0] = 1;
        SaveManager.instance.Save();
        InfoText();
    }

    public void OnClickMicrowaveovenBtn(Button btn)
    {
        BigNumber price = new BigNumber(5000);

        if (FoodMachine[1] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        GoldBonus += 0.15f;
        btn.interactable = false;

        FoodMachine[1] = 1;
        SaveManager.instance.Save();
        InfoText();
    }
    public void OnClickSteamerBtn(Button btn)
    {
        BigNumber price = new BigNumber(5000);

        if (FoodMachine[2] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        MakeSpeed += 0.05f;
        GoldBonus += 0.05f;
        btn.interactable = false;

        FoodMachine[2] = 1;
        SaveManager.instance.Save();
        InfoText();
    }
    public void OnClickDeepfryerBtn(Button btn)
    {
        BigNumber price = new BigNumber(6000);

        if (FoodMachine[3] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        MakeSpeed += 0.2f;
        GoldBonus += 0.05f;
        btn.interactable = false;

        FoodMachine[3] = 1;
        SaveManager.instance.Save();
        InfoText();
    }
    public void OnClickRefrigeratorBtn(Button btn)
    {
        BigNumber price = new BigNumber(7000);

        if (FoodMachine[4] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        NoUseFishChance += 0.05f;
        btn.interactable = false;

        FoodMachine[4] = 1;
        SaveManager.instance.Save();
        InfoText();
    }
    public void OnClickOvenBtn(Button btn)
    {
        BigNumber price = new BigNumber(9000);

        if (FoodMachine[5] == 1) return;
        if (!CurrencyManager.instance.SpendGold(price)) return;

        SpecialChance += 0.3f;
        btn.interactable = false;

        FoodMachine[5] = 1;
        SaveManager.instance.Save();
        InfoText();
    }
    public void OnClickCookerBtn(Button btn)
    {
        if (CookCatNum >= RestaurantLevel) return;

        int currentPrice = 2000 * (CookCatNum + 1);
        chefBuyText.text = $"{currentPrice}";

        if (!CurrencyManager.instance.SpendGold(new BigNumber(currentPrice))) return;

        CookCatNum += 1;
        btn.interactable = CookCatNum < RestaurantLevel;

        for (int i = 1; i <= CookCatNum; i++)
        {
            ProductionManager.instance.StartChef(i);
        }

        SaveManager.instance.Save();
        InfoText();
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
        InfoText();
    }

    public void OnClickChefLevelUpBtn(Button btn)
    {
        if (ChefCatLevel >= 9) return;

        int basePrice = 300;
        int currentPrice = basePrice * (ChefCatLevel);
        chefLevelText.text = $"{currentPrice}";

        if (!CurrencyManager.instance.SpendGold(new BigNumber(currentPrice))) return;

        ChefCatLevel += 1;

        if (ChefCatLevel >= 9)
        {
            ChefCatLevel = 9;
            btn.interactable = false;
        }

        chefLevelText.text = $"Chef Level : {ChefCatLevel}";
        SaveManager.instance.Save();
        InfoText();
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

    public void UpgradeRestaurant() //레스토랑 업그레이드 시 셰프 위치, 레스토랑 변경
    {
        selectMachinesObjs[2].gameObject.SetActive(false);
        selectMachinesObjs[3].gameObject.SetActive(false);
        selectMachinesObjs[4].gameObject.SetActive(false);

        if (RestaurantLevel == 2)
        {
            restaurants[0].gameObject.SetActive(false);
            restaurants[1].gameObject.SetActive(true);
            restaurants[2].gameObject.SetActive(false);
            restaurants[1].GetComponent<RestaurantPosition>().seats.ResetSeats();

            selectMachinesObjs[2].gameObject.SetActive(true);
            for (int i = 0; i < 3; i++)
                selectMachinesObjs[i].transform.localScale = new Vector3(.8f, .8f, .8f);
            selectMachinesObjs[0].transform.localPosition = new Vector3(-63, -43, 0);
            selectMachinesObjs[1].transform.localPosition = new Vector3(857, 0, 0);
        }
        else if (RestaurantLevel == 3)
        {
            restaurants[0].gameObject.SetActive(false);
            restaurants[1].gameObject.SetActive(false);
            restaurants[2].gameObject.SetActive(true);
            restaurants[2].GetComponent<RestaurantPosition>().seats.ResetSeats();

            selectMachinesObjs[2].gameObject.SetActive(true);
            selectMachinesObjs[3].gameObject.SetActive(true);
            selectMachinesObjs[4].gameObject.SetActive(true);
            for (int i = 0; i < 5; i++)
                selectMachinesObjs[i].transform.localScale = new Vector3(.6f, .6f, .6f);
            selectMachinesObjs[0].transform.localPosition = new Vector3(-140, 32, 0);
            selectMachinesObjs[1].transform.localPosition = new Vector3(204, 32, 0);
            selectMachinesObjs[2].transform.localPosition = new Vector3(432, 32, 0);
        }
        chefBtn.interactable = CookCatNum < RestaurantLevel;
        ProductionManager.instance.ChefPosition();
    }

    public void SelectMachines2(GameObject selectMachine)  // + 추가 버튼 누르면 팝업
    {
        nowSelectMachine = selectMachine;

        selectMachinesPopup.gameObject.SetActive(true);
    }
    public void SelectMachines2Select(int index)    //팝업에서 가구 1~6 선택
    {
        nowSelectMachineNum = index;
        switch (index)
        {
            case 0:
                selectMachinesText.text = $"Gas Stove\r\n\r\n\n Food Make Speed + 10%";
                break;
            case 1:
                selectMachinesText.text = $"Refrigerator\r\n\r\n\n No Use Fish Chance = 5%";
                break;
            case 2:
                selectMachinesText.text = $"Deepfryer\r\n\r\n Make Speed + 20%\n Gold Bonus + 5%";
                break;
            case 3:
                selectMachinesText.text = $"Oven\r\n\r\n\n Food Make Speed + 10%";
                break;
            case 4:
                selectMachinesText.text = $"Microwave Oven\r\n\r\n\n GoldBonus + 15%";
                break;
            case 5:
                selectMachinesText.text = $"Steamer\r\n\r\n Make Speed + 5%\n GoldBonus + 5%";
                break;
        }
    }
    public void SelectMachines2Use()    //팝업에서 가구 1~6 선택
    {
        int slotIndex = -1;

        // 지금 선택한 슬롯 번호 찾기
        for (int i = 0; i < selectMachinesObjs.Length; i++)
        {
            if (selectMachinesObjs[i].gameObject == nowSelectMachine)
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex == -1)
            return;


        // =========================
        // 1. 기존 가구 효과 제거
        // =========================

        // 저장값:
        // 0 = 비어있음
        // 1~6 = 가구
        int oldSavedMachine = FoodMachine2[slotIndex];

        if (oldSavedMachine != 0)
        {
            int oldMachineNum = oldSavedMachine - 1;

            RemoveMachineEffect(oldMachineNum);
            MachineCount[oldMachineNum]++; // 기존 가구 돌려받음
        }


        // =========================
        // 2. 새 가구 효과 추가
        // =========================

        // 보유 개수 없으면 설치 불가
        if (MachineCount[nowSelectMachineNum] <= 0)
            return;

        MachineCount[nowSelectMachineNum]--; // 하나 사용
        AddMachineEffect(nowSelectMachineNum);


        // =========================
        // 3. 이미지 변경
        // =========================

        nowSelectMachine.transform.GetChild(1).gameObject.SetActive(false);
        nowSelectMachine.transform.GetChild(2).gameObject.SetActive(true);

        nowSelectMachine.transform.GetChild(2)
            .GetComponent<Image>().sprite =
            selectMachinesImgs[nowSelectMachineNum];


        // =========================
        // 4. 새 가구 저장
        // =========================

        FoodMachine2[slotIndex] = nowSelectMachineNum + 1;

        SaveManager.instance.Save();

        InfoText();

        for (int i = 0; i < selectMachineCountText.Length; i++)
            selectMachineCountText[i].text = MachineCount[i].ToString();

        selectMachinesPopup.gameObject.SetActive(false);
    }

    public void SelectMachines2Close()    //팝업에서 가구 1~6 선택
    {
        selectMachinesPopup.gameObject.SetActive(false);
    }

    public void InfoText()
    {
        infoText.text = $" MakeSpeed : {MakeSpeed * 100}%\n GoldBonus : {GoldBonus * 100}%\n SpecialChance : {SpecialChance * 100}%\n NoUseFishChance : {NoUseFishChance * 100}%";
        chefLevelText.text = $"Chef Level : {(ChefCatLevel)}";
        chefBuyText.text = $"{2000 * (CookCatNum + 1)}";
    }

    private void AddMachineEffect(int machineNum)
    {
        switch (machineNum)
        {
            case 0: // 가스레인지
                MakeSpeed += 0.1f;
                break;

            case 1: // 냉장고
                NoUseFishChance += 0.05f;
                break;

            case 2: // 튀김기
                MakeSpeed += 0.2f;
                GoldBonus += 0.05f;
                break;

            case 3: // 오븐
                SpecialChance += 0.3f;
                break;

            case 4: // 전자레인지
                GoldBonus += 0.15f;
                break;

            case 5: // 찜기
                MakeSpeed += 0.05f;
                GoldBonus += 0.05f;
                break;
        }
    }

    private void RemoveMachineEffect(int machineNum)
    {
        switch (machineNum)
        {
            case 0:
                MakeSpeed -= 0.1f;
                break;

            case 1:
                NoUseFishChance -= 0.05f;
                break;

            case 2:
                MakeSpeed -= 0.2f;
                GoldBonus -= 0.05f;
                break;

            case 3:
                SpecialChance -= 0.3f;
                break;

            case 4:
                GoldBonus -= 0.15f;
                break;

            case 5:
                MakeSpeed -= 0.05f;
                GoldBonus -= 0.05f;
                break;
        }
    }
}
