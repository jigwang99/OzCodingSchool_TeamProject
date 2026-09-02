using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;

public class GachaResultItemDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI gradeText;

    public void SetItemInfo(GachaItem item)
    {
        if (item == null)
            return;

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (rarityText != null)
            rarityText.text = item.rarity.ToString();

        if (gradeText != null)
            gradeText.text = $"Lv.{item.grade}";
    }
}