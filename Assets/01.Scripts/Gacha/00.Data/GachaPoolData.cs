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

                return 0;
            }

            return pityConfig.GetWeight(pityLevel, rarity);
        }

    }
}