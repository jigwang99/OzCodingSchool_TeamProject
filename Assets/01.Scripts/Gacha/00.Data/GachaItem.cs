using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 개별 가챠 아이템 정보
    /// 하나의 이름 = 하나의 레어리티 = 하나의 등급
    /// 중복은 GachaInventory에서 관리
    /// </summary>
    [System.Serializable]
    public class GachaItem
    {
        [SerializeField]
        private string itemId;

        [SerializeField]
        private string itemName;

        [SerializeField]
        private GachaGroup group;

        [SerializeField]
        private GachaRarity rarity;

        [SerializeField]
        private int grade;

        [SerializeField]
        private int weight;

        [SerializeField]
        private GameObject displayPrefab;

        public string ItemId => itemId;
        public string ItemName => itemName;
        public GachaGroup Group => group;
        public GachaRarity Rarity => rarity;
        public int Grade => grade;
        public int Weight => weight;
        public GameObject DisplayPrefab => displayPrefab;

        /// <summary>
        /// 디스플레이 이름 반환 (예: "참치 검 LV.1")
        /// </summary>
        public string GetDisplayName()
        {
            return $"{itemName} LV.{grade}";
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 데이터 검증
        /// </summary>
        [ContextMenu("Validate Item ID")]
        private void ValidateItemId()
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogError($"[{itemName}] itemId는 필수입니다!");
            }

            if (grade < 1 || grade > 4)
            {
                Debug.LogError($"[{itemName}] grade는 1~4 사이여야 합니다!");
            }

            if (weight <= 0)
            {
                Debug.LogWarning($"[{itemName}] weight는 0보다 커야 합니다!");
            }
        }
#endif
    }
}
