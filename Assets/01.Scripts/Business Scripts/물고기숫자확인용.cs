using TMPro;
using UnityEngine;

public class 물고기숫자확인용 : MonoBehaviour
{
    int fishNum;

    private void Start()
    {
        UpdateText();
    }
    public void GetFish()
    {
        if (name.Split()[0] == "Common")
           fishNum = ++FishInventoryManager.instance.commonFishNum[int.Parse(name.Split()[1]) - 1];
        else if (name.Split()[0] == "Rare")
            fishNum = ++FishInventoryManager.instance.rareFishNum[int.Parse(name.Split()[1]) - 1];
        else if (name.Split()[0] == "Unique")
            fishNum = ++FishInventoryManager.instance.uniqueFishNum[int.Parse(name.Split()[1]) - 1];
        else if (name.Split()[0] == "Epic")
            fishNum = ++FishInventoryManager.instance.epicFishNum[int.Parse(name.Split()[1]) - 1];


        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{name}\n{fishNum}";
    }

    public void UpdateText()
    {
        if (name.Split()[0] == "Common")
            fishNum = FishInventoryManager.instance.commonFishNum[int.Parse(name.Split()[1]) - 1];
        else if (name.Split()[0] == "Rare")
            fishNum = FishInventoryManager.instance.rareFishNum[int.Parse(name.Split()[1]) - 1];
        else if (name.Split()[0] == "Unique")
            fishNum = FishInventoryManager.instance.uniqueFishNum[int.Parse(name.Split()[1]) - 1];
        else if (name.Split()[0] == "Epic")
            fishNum = FishInventoryManager.instance.epicFishNum[int.Parse(name.Split()[1]) - 1];

        transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{name}\n{fishNum}";
    }
}
