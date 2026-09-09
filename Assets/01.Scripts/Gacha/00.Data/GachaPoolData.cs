using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 하나의 가챠 풀 (무기/가구/레시피)에 들어갈 모든 아이템 관리
    /// 천장 설정(GachaPityConfig) 포함
    /// ScriptableObject로 Unity Inspector에서 직접 생성 가능
    /// </summary>
    [CreateAssetMenu(fileName = "GachaPool_", menuName = "Gacha/GachaPool")]
    public class GachaPoolData : ScriptableObject
    {
        [SerializeField]
        private string poolName = "WeaponGachaPool";

        [SerializeField]
        private GachaGroup group = GachaGroup.Weapon;

        [SerializeField]
        private List<GachaItem> items = new List<GachaItem>();

        [SerializeField]
        private GachaPityConfig pityConfig;

        public string PoolName => poolName;
        public GachaGroup Group => group;
        public List<GachaItem> Items => items;
        public GachaPityConfig PityConfig => pityConfig;

        /// <summary>
        /// 특정 레어리티의 아이템 목록 반환
        /// </summary>
        /// <param name="rarity">레어리티</param>
        /// <returns>해당 레어리티의 아이템 리스트</returns>
        public List<GachaItem> GetItemsByRarity(GachaRarity rarity)
        {
            return items.FindAll(item => item.Rarity == rarity);
        }

        /// <summary>
        /// 특정 레벨의 특정 레어리티 가중치 조회
        /// </summary>
        /// <param name="pityLevel">천장 레벨</param>
        /// <param name="rarity">레어리티</param>
        /// <returns>가중치</returns>
        public int GetWeight(int pityLevel, GachaRarity rarity)
        {
            if (pityConfig == null)
            {
                Debug.LogError($"[{poolName}] pityConfig가 할당되지 않았습니다!");
                return 0;
            }

            return pityConfig.GetWeight(pityLevel, rarity);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 풀 데이터 검증
        /// </summary>
        [ContextMenu("Validate Pool")]
        private void ValidatePool()
        {
            if (string.IsNullOrEmpty(poolName))
            {
                Debug.LogError("[가챠 풀] poolName이 비어있습니다!", this);
                return;
            }

            if (items.Count == 0)
            {
                Debug.LogWarning($"[{poolName}] 아이템이 하나도 없습니다!", this);
                return;
            }

            if (pityConfig == null)
            {
                Debug.LogError($"[{poolName}] pityConfig가 할당되지 않았습니다!", this);
                return;
            }

            // 아이템 ID 중복 체크
            var itemIds = new HashSet<string>();
            foreach (var item in items)
            {
                if (itemIds.Contains(item.ItemId))
                {
                    Debug.LogError($"[{poolName}] 중복된 itemId: {item.ItemId}", this);
                }
                itemIds.Add(item.ItemId);

                // 그룹 일치성 확인
                if (item.Group != group)
                {
                    Debug.LogWarning($"[{poolName}] 아이템 {item.ItemName}의 그룹이 다릅니다!", this);
                }
            }

            // 레어리티별 아이템 개수 확인
            int commonCount = GetItemsByRarity(GachaRarity.Common).Count;
            int rareCount = GetItemsByRarity(GachaRarity.Rare).Count;
            int uniqueCount = GetItemsByRarity(GachaRarity.Unique).Count;
            int epicCount = GetItemsByRarity(GachaRarity.Epic).Count;

            Debug.Log($"[{poolName}] 검증 완료 - Common: {commonCount}, Rare: {rareCount}, Unique: {uniqueCount}, Epic: {epicCount}", this);
        }
#endif
    }
}