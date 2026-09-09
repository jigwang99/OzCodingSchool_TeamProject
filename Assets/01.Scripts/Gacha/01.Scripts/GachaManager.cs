using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠 뽑기 시스템의 핵심 로직
    /// 재화 소비, 뽑기 실행, 결과 반환
    /// 1회/10회/30회 통합 처리
    /// </summary>
    public class GachaManager : MonoBehaviour
    {
        private static GachaManager _instance;

        // 가챠 풀 (무기, 가구, 레시피)
        private Dictionary<GachaGroup, GachaPoolData> _pools
            = new Dictionary<GachaGroup, GachaPoolData>();

        // 천장 시스템
        private GachaPitySystem _pitySystem;

        // 인벤토리
        private GachaInventory _inventory;

        // 재화 제공자 (임시: GoldManager)
        private ICurrencyProvider _currencyProvider;

        // 최근 뽑은 아이템 (향후 10연차 중복 방지용)
        private GachaItem _lastDrawnItem;

        public static GachaManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GachaManager>();
                    if (_instance == null)
                    {
                        Debug.LogError("[가챠 매니저] GachaManager를 찾을 수 없습니다!");
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 가챠 매니저 초기화
        /// </summary>
        /// <param name="pools">가챠 풀 딕셔너리</param>
        /// <param name="pitySystem">천장 시스템</param>
        /// <param name="inventory">인벤토리</param>
        /// <param name="currencyProvider">재화 제공자</param>
        public void Initialize(
            Dictionary<GachaGroup, GachaPoolData> pools,
            GachaPitySystem pitySystem,
            GachaInventory inventory,
            ICurrencyProvider currencyProvider)
        {
            _pools = pools;
            _pitySystem = pitySystem;
            _inventory = inventory;
            _currencyProvider = currencyProvider;

            Debug.Log("[가챠 매니저] 초기화 완료");
        }

        /// <summary>
        /// 가챠 실행 (1회/10회/30회 통합)
        /// </summary>
        /// <param name="group">그룹 (Weapon/Furniture/Recipe)</param>
        /// <param name="pullCount">뽑기 횟수 (1, 10, 30)</param>
        /// <param name="costPerPull">1회당 비용</param>
        /// <returns>획득 아이템 리스트</returns>
        public List<GachaItem> DrawGacha(
            GachaGroup group,
            int pullCount,
            int costPerPull)
        {
            var results = new List<GachaItem>();

            // 풀 데이터 확인
            if (!_pools.ContainsKey(group))
            {
                Debug.LogError($"[가챠] {group} 그룹의 풀을 찾을 수 없습니다.");
                return results;
            }

            GachaPoolData poolData = _pools[group];

            Debug.Log($"[가챠] {pullCount}회 뽑기 시작 ({group}, 비용: {costPerPull} x {pullCount})");

            for (int i = 0; i < pullCount; i++)
            {
                // Step 1: 재화 확인/소비
                if (!_currencyProvider.SpendGold(costPerPull))
                {
                    Debug.Log($"[가챠] 골드 부족 ({i + 1}회에서 중단)");
                    break;
                }

                // Step 2: 현재 레벨 확인 (뽑기 전)
                int currentLevel = _pitySystem.GetCurrentPityLevel(group);

                // Step 3: 레어리티 결정 (1차 뽑기)
                GachaRarity rarity = DrawRarity(group, currentLevel, poolData);

                // Step 4: 아이템 선택 (2차 뽑기)
                GachaItem item = DrawItem(group, rarity, poolData);

                if (item == null)
                {
                    Debug.LogError($"[가챠] 아이템을 찾을 수 없습니다.");
                    break;
                }

                // Step 5: 횟수 증가
                _pitySystem.IncreasePullCount(group);

                // Step 6: 레벨업 확인
                if (_pitySystem.CheckAndLevelUp(group, poolData.PityConfig))
                {
                    Debug.Log($"[가챠] {group} 천장 레벨업 → 확률 변경");
                }

                results.Add(item);
                _lastDrawnItem = item;

                Debug.Log($"[가챠] {i + 1}회: {item.GetDisplayName()} 획득 ({rarity})");
            }

            Debug.Log($"[가챠] 뽑기 완료: {results.Count}개 획득");
            return results;
        }

        /// <summary>
        /// 1차 뽑기: 레어리티 결정
        /// 현재 레벨의 가중치를 사용하여 레어리티 선택
        /// </summary>
        /// <param name="group">그룹</param>
        /// <param name="pityLevel">천장 레벨</param>
        /// <param name="poolData">풀 데이터</param>
        /// <returns>선택된 레어리티</returns>
        private GachaRarity DrawRarity(GachaGroup group, int pityLevel, GachaPoolData poolData)
        {
            GachaPityConfig config = poolData.PityConfig;

            // 현재 레벨의 모든 레어리티 가중치
            int commonWeight = config.GetWeight(pityLevel, GachaRarity.Common);
            int rareWeight = config.GetWeight(pityLevel, GachaRarity.Rare);
            int uniqueWeight = config.GetWeight(pityLevel, GachaRarity.Unique);
            int epicWeight = config.GetWeight(pityLevel, GachaRarity.Epic);

            int totalWeight = commonWeight + rareWeight + uniqueWeight + epicWeight;

            if (totalWeight <= 0)
            {
                Debug.LogError($"[가챠] {group} Level {pityLevel}의 가중치가 0입니다!");
                return GachaRarity.Common;
            }

            // 가중치 기반 랜덤
            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;

            // Common
            cumulativeWeight += commonWeight;
            if (randomValue < cumulativeWeight)
                return GachaRarity.Common;

            // Rare
            cumulativeWeight += rareWeight;
            if (randomValue < cumulativeWeight)
                return GachaRarity.Rare;

            // Unique
            cumulativeWeight += uniqueWeight;
            if (randomValue < cumulativeWeight)
                return GachaRarity.Unique;

            // Epic
            return GachaRarity.Epic;
        }

        /// <summary>
        /// 2차 뽑기: 아이템 결정
        /// 선택된 레어리티 내에서 개별 아이템 선택
        /// </summary>
        /// <param name="group">그룹</param>
        /// <param name="rarity">레어리티</param>
        /// <param name="poolData">풀 데이터</param>
        /// <returns>선택된 아이템</returns>
        private GachaItem DrawItem(GachaGroup group, GachaRarity rarity, GachaPoolData poolData)
        {
            var candidates = poolData.GetItemsByRarity(rarity);

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[가챠] {group} {rarity} 아이템이 없습니다!");
                return null;
            }

            // 아이템 가중치 합계
            int totalWeight = 0;
            foreach (var item in candidates)
            {
                totalWeight += item.Weight;
            }

            if (totalWeight <= 0)
            {
                Debug.LogError($"[가챠] {rarity} 아이템의 총 가중치가 0입니다!");
                return candidates[0];
            }

            // 가중치 기반 랜덤
            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;

            foreach (var item in candidates)
            {
                cumulativeWeight += item.Weight;
                if (randomValue < cumulativeWeight)
                    return item;
            }

            return candidates[0];
        }

        /// <summary>
        /// 특정 그룹의 풀 데이터 조회
        /// </summary>
        /// <param name="group">그룹</param>
        /// <returns>풀 데이터</returns>
        public GachaPoolData GetPoolData(GachaGroup group)
        {
            if (!_pools.ContainsKey(group))
            {
                Debug.LogError($"[가챠] {group} 그룹의 풀을 찾을 수 없습니다.");
                return null;
            }

            return _pools[group];
        }

        /// <summary>
        /// 모든 풀 데이터 조회
        /// </summary>
        /// <returns>풀 딕셔너리</returns>
        public Dictionary<GachaGroup, GachaPoolData> GetAllPools()
        {
            return new Dictionary<GachaGroup, GachaPoolData>(_pools);
        }
    }
}