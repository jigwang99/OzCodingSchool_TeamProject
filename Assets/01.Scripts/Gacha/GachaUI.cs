using System.Collections.Generic;
using UnityEngine;

namespace PixelRestaurant.Data
{
    /*
    /// <summary>
    /// 가챠 아이템 정보 (무기, 가구, 요리 등)
    /// </summary>
    [System.Serializable]
    public class GachaItem
    {
        public string itemId;           // 아이템 고유ID
        public string itemName;         // 아이템 이름
        public GachaGroup group;        // 그룹 (무기, 가구, 요리)
        public GachaRarity rarity;      // 레어리티
        public int grade;               // 등급 (1,2,3,4)
        public int weight;              // 개별 아이템 가중치 (기본값 1)
        public GameObject displayPrefab; // 결과 표시 프리팹 (선택)
    }

    /// <summary>
    /// 가챠 그룹 분류
    /// </summary>
    public enum GachaGroup
    {
        Weapon,    // 무기
        Furniture, // 가구
        Recipe     // 요리
    }

    /// <summary>
    /// 레어리티 분류
    /// </summary>
    public enum GachaRarity
    {
        Common = 60,   // 커몬: 가중치 60
        Rare = 30,     // 레어: 가중치 30
        Unique = 9,    // 유니크: 가중치 9
        Epic = 1       // 에픽: 가중치 1
    }

    /// <summary>
    /// 가챠 풀 (하나의 가챠 세트)
    /// </summary>
    [CreateAssetMenu(fileName = "GachaPool_", menuName = "Gacha/GachaPool")]
    public class GachaPool : ScriptableObject
    {
        [SerializeField] private string poolName;
        [SerializeField] private List<GachaItem> items = new List<GachaItem>();
        [SerializeField] private GameObject defaultResultPrefab; // 기본 프리팹

        public string PoolName => poolName;
        public List<GachaItem> Items => items;
        public GameObject DefaultResultPrefab => defaultResultPrefab;

        /// <summary>
        /// 특정 그룹의 아이템만 필터링해서 반환
        /// </summary>
        public List<GachaItem> GetItemsByGroup(GachaGroup group)
        {
            return items.FindAll(item => item.group == group);
        }

        /// <summary>
        /// 특정 그룹과 레어리티의 아이템들 반환
        /// </summary>
        public List<GachaItem> GetItemsByGroupAndRarity(GachaGroup group, GachaRarity rarity)
        {
            return items.FindAll(item => item.group == group && item.rarity == rarity);
        }
    }
    */
}