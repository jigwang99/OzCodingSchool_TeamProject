using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    [System.Serializable]
    public class GachaItem
    {
        [Header("Item Information")]
        [SerializeField] private string itemId;
        [SerializeField] private string itemName;
        [SerializeField] private GachaGroup group;
        [SerializeField] private GachaRarity rarity;
        [SerializeField] private int grade;
        [SerializeField] private int weight;

        [Header("Display")]
        [SerializeField] private Sprite itemIcon;

        public string ItemId => itemId;
        public string ItemName => itemName;
        public GachaGroup Group => group;
        public GachaRarity Rarity => rarity;
        public int Grade => grade;
        public int Weight => weight;
        public Sprite ItemIcon => itemIcon;

        public string GetDisplayName()
        {
            return $"{itemName} LV.{grade}";
        }

#if UNITY_EDITOR
        [ContextMenu("Validate Item ID")]
        private void ValidateItemId()
        {
            if (string.IsNullOrEmpty(itemId))
                Debug.LogError($"[{itemName}] itemId는 필수입니다!");

            if (grade < 1 || grade > 4)
                Debug.LogError($"[{itemName}] grade는 1~4 사이여야 합니다!");

            if (weight <= 0)
                Debug.LogWarning($"[{itemName}] weight는 0보다 커야 합니다!");

            if (itemIcon == null)
                Debug.LogWarning($"[{itemName}] 아이콘이 등록되지 않았습니다!");
        }
#endif
    }
}