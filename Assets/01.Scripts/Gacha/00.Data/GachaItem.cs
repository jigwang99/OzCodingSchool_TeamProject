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

        [Header("Weapon Connection")]
        [Tooltip("실제 PlayerWeaponCatalog의 무기 ID")]
        [SerializeField] private string linkedWeaponId;

        [Header("Furniture Connection")]
        [Tooltip("비즈니스 씬에서 사용할 실제 가구 ID")]
        [SerializeField] private string linkedFurnitureId;


        [Header("Recipe Connection")]
        [Tooltip("실제 비즈니스 씬에서 사용하는 레시피 ID")]
        [SerializeField] private string linkedRecipeId;
        public string LinkedRecipeId => linkedRecipeId;

        [Header("Stats")]
        [SerializeField]
        private GachaItemStats stats = new GachaItemStats();

        public string ItemId => itemId;
        public string ItemName => itemName;
        public GachaGroup Group => group;
        public GachaRarity Rarity => rarity;
        public int Grade => grade;
        public int Weight => weight;
        public Sprite ItemIcon => itemIcon;

        // 실제 플레이어 무기와 연결되는 ID
        public string LinkedWeaponId => linkedWeaponId;

        public GachaItemStats Stats => stats;

        public string GetDisplayName()
        {
            return $"{itemName} LV.{grade}";
        }

#if UNITY_EDITOR
        [ContextMenu("Validate Item ID")]
        private void ValidateItemId()
        {
            if (string.IsNullOrEmpty(itemId))
             

            if (grade < 1 || grade > 4)
        

            if (weight <= 0)
            

            if (itemIcon == null)
             

            if (group == GachaGroup.Weapon &&
                string.IsNullOrEmpty(linkedWeaponId))
            {
         
             
            }
        }
#endif
    }
}