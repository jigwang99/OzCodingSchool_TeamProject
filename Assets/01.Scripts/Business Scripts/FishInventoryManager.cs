using System;
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
    public int nowSelectRarity; //ProductionManager

    public FishCountView[] fishUI;  //커런트 매니저로 넘기기
    public event Action OnFishChanged;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

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
    public void UpdateFish()
    {
        OnFishChanged?.Invoke();
    }
    public void AddCommonFish(int index)   
    {
        commonFishNum[index]++; 
        OnFishChanged?.Invoke();            
    }
    public void AddRareFish(int index)    
    {
        rareFishNum[index]++;           
        OnFishChanged?.Invoke();         
    }
    public void AddUniqueFish(int index)  
    {
        uniqueFishNum[index]++;           
        OnFishChanged?.Invoke();          
    }
    public void AddEpicFish(int index)    //물고기 업데이트할때 이 함수 사용하세용 FishInventoryManager.instance.AddCommonFish(index);
    {                                     
        epicFishNum[index]++;        
        OnFishChanged?.Invoke();           
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
