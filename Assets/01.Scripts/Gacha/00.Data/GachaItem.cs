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

        [Header("Weapon Merge")]
        [Tooltip("합치기 성공 시 획득할 다음 단계 가챠 무기의 ItemId")]
        [SerializeField] private string nextMergeItemId;

        public string NextMergeItemId => nextMergeItemId;
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
            if (string.IsNullOrWhiteSpace(itemId)) Debug.LogWarning("Gacha ItemId is empty");
            int maxGrade = group == GachaGroup.Weapon ? GachaWeaponMerge.GetMaxGrade(rarity) : 4;
            if (grade < 1 || grade > maxGrade) Debug.LogWarning($"Invalid grade: {grade}");
            if (weight <= 0) Debug.LogWarning("Weight must be positive");
            if (itemIcon == null) Debug.LogWarning("Item icon is missing");
            if (group == GachaGroup.Weapon && string.IsNullOrWhiteSpace(linkedWeaponId))
                Debug.LogWarning("LinkedWeaponId is missing");
        }
#endif
    }
}
