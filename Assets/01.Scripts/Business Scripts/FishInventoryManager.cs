using System.Text;
using TMPro;
using UnityEngine;

public class FishInventoryManager : MonoBehaviour
{
    public static FishInventoryManager instance;

    public int[][] fishNums;
    public int[] commonFishNum;
    public int[] rareFishNum;
    public int[] uniqueFishNum;
    public int[] epicFishNum;

    TextMeshProUGUI selectRarityText;
    public int nowSelectRarity;

    public 물고기숫자확인용[] fishUI;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        commonFishNum = new int[8];
        rareFishNum = new int[4];
        uniqueFishNum = new int[2];
        epicFishNum = new int[1];

        fishNums = new int[][]
        {
            commonFishNum,
            rareFishNum,
            uniqueFishNum,
            epicFishNum
        };

        selectRarityText = transform.Find("SelectRarityText").GetComponent<TextMeshProUGUI>();
    }

    private void Start()    //test
    {
        fishUI = FindObjectsOfType<물고기숫자확인용>();
    }
    public void UpdateAllText() //test
    {
        foreach (var ui in fishUI)
            ui.UpdateText();
    }

    public void SelectCommon()
    {
        selectRarityText.text = "Common";
        nowSelectRarity = 0;
    }

    public void SelectRare()
    {
        selectRarityText.text = "Rare";
        nowSelectRarity = 1;
    }

    public void SelectUnique()
    {
        selectRarityText.text = "Unique";
        nowSelectRarity = 2;
    }

    public void SelectEpic()
    {
        selectRarityText.text = "Epic";
        nowSelectRarity = 3;
    }

}
