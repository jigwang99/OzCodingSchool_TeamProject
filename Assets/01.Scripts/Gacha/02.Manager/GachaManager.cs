using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠 시스템의 핵심 로직
    /// 재화 소비, 가챠 실행, 결과 반환
    /// </summary>
    public class GachaManager : MonoBehaviour
    {
        private static GachaManager _instance;

        [Header("가챠 1회 비용")]
        [SerializeField, Min(0)]
        private int weaponGachaCost = 30;

        [SerializeField, Min(0)]
        private int recipeGachaCost = 30;

        [SerializeField, Min(0)]
        private int furnitureGachaCost = 30;

        private Dictionary<GachaGroup, GachaPoolData> _pools
            = new Dictionary<GachaGroup, GachaPoolData>();

        private GachaPitySystem _pitySystem;

        private GachaInventory _inventory;

        // 공용 CurrencyManager를 ICurrencyProvider로 사용
        private ICurrencyProvider _currencyProvider;

        private GachaItem _lastDrawnItem;

        public event Action<List<GachaItem>> OnGachaItemsDrawn;
        public static GachaManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GachaManager>();

                    if (_instance == null)
                    {
                   
                      
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


            if (CurrencyManager.instance != null)
            {
                _currencyProvider = CurrencyManager.instance;
            }
            else
            {
            }
        }
        /// <summary>
        /// 가챠 매니저 초기화
        /// </summary>
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

        }

        /// <summary>
        /// 가챠 종류별 1회 비용 반환
        /// </summary>
        public BigNumber GetGachaCost(GachaGroup group)
        {
            switch (group)
            {
                case GachaGroup.Weapon:
                    return new BigNumber(weaponGachaCost);

                case GachaGroup.Recipe:
                    return new BigNumber(recipeGachaCost);

                case GachaGroup.Furniture:
                    return new BigNumber(furnitureGachaCost);

                default:
                    Debug.LogError(
                        $"[GachaManager] 알 수 없는 가챠 종류: {group}"
                    );

                    return new BigNumber(0);
            }
        }
        /// <summary>
        /// 가챠 실행
        /// 1회 / 10회 지원
        /// </summary>
        public List<GachaItem> DrawGacha(
        GachaGroup group,
        int pullCount,
        BigNumber costPerPull)
        {
            var results = new List<GachaItem>();

            // =========================
            // 기본 검증
            // =========================

            if (pullCount != 1 && pullCount != 10)
            {
        

                return results;
            }

            if (_pools == null || !_pools.ContainsKey(group))
            {
   

                return results;
            }

            if (_currencyProvider == null)
            {
                _currencyProvider = CurrencyManager.instance;

                if (_currencyProvider == null)
                {
                  

                    return results;
                }

            }

            if (_pitySystem == null)
            {
           
               

                return results;
            }

            GachaPoolData poolData = _pools[group];



            // =========================
            // 가챠 종류별 비용 계산
            // =========================

            // 외부에서 전달된 비용 대신
            // 가챠 종류에 설정된 비용을 사용
            costPerPull = GetGachaCost(group);

            // 1회 비용 유효성 확인
            if (costPerPull <= new BigNumber(0))
            {
                Debug.LogError(
                    $"[GachaManager] 가챠 비용 설정 오류: {group}"
                );

                return results;
            }

            // 전체 뽑기 비용 계산
            BigNumber totalCost = new BigNumber(0);

            for (int i = 0; i < pullCount; i++)
            {
                totalCost += costPerPull;
            }

            // 현재 골드 확인
            BigNumber currentGold =
                CurrencyManager.instance.GetCurrentGold();

            // 골드 부족 시 뽑기 중단
            if (currentGold < totalCost)
            {
                Debug.Log(
                    $"[GachaManager] 골드 부족! " +
                    $"필요 골드: {totalCost}, " +
                    $"보유 골드: {currentGold}"
                );

                return results;
            }
            // =========================
            // Pull
            // =========================

            for (int i = 0; i < pullCount; i++)
            {
                // Step 1
                // BigNumber 기준으로 골드 소비
                if (!_currencyProvider.SpendGold(costPerPull))
                {
               

                    // 중간 실패이므로 결과 전체 취소
                    results.Clear();

                    return results;
                }

                // Step 2
                int currentLevel =
                    _pitySystem.GetCurrentPityLevel(group);

                // Step 3
                GachaRarity rarity =
                    DrawRarity(
                        group,
                        currentLevel,
                        poolData
                    );

                // Step 4
                GachaItem item =
                    DrawItem(
                        group,
                        rarity,
                        poolData
                    );

                if (item == null)
                {

                    // 뽑기 실패
                    results.Clear();

                    return results;
                }

                // Step 5
                _pitySystem.IncreasePullCount(group);

                // Step 6
                if (_pitySystem.CheckAndLevelUp(
                    group,
                    poolData.PityConfig))
                {
                    
                }

                results.Add(item);
                _lastDrawnItem = item;

               
            }

            // =========================
            // 최종 결과 검증
            // =========================

            if (results.Count != pullCount)
            {
                

                results.Clear();
                return results;
            }


            if (results.Count > 0)
            {
                // 뽑기 결과를 가챠 인벤토리에 직접 추가
                if (_inventory == null)
                {
                    _inventory = GachaInventory.Instance;
                }

                if (_inventory != null)
                {
                    foreach (GachaItem item in results)
                    {
                        if (item == null ||
                            string.IsNullOrEmpty(item.ItemId))
                        {
                            continue;
                        }

                        _inventory.AddItem(item.ItemId, 1, saveImmediately: false);

                    }
                }
              

                // 결과 표시와 다른 시스템에 알림
                OnGachaItemsDrawn?.Invoke(results);

                // Persist once, after all awards and result subscribers finish.
                if (_inventory != null && SaveManager.instance != null)
                    SaveManager.instance.Save();
            }

            return results;

           
        }

        // =========================
        // Rarity
        // =========================

        private GachaRarity DrawRarity(
            GachaGroup group,
            int pityLevel,
            GachaPoolData poolData)
        {
            GachaPityConfig config = poolData.PityConfig;

            int commonWeight =
                config.GetWeight(
                    pityLevel,
                    GachaRarity.Common
                );

            int rareWeight =
                config.GetWeight(
                    pityLevel,
                    GachaRarity.Rare
                );

            int uniqueWeight =
                config.GetWeight(
                    pityLevel,
                    GachaRarity.Unique
                );

            int epicWeight =
                config.GetWeight(
                    pityLevel,
                    GachaRarity.Epic
                );

            int totalWeight =
                commonWeight
                + rareWeight
                + uniqueWeight
                + epicWeight;

            if (totalWeight <= 0)
            {
              

                return GachaRarity.Common;
            }

            int randomValue =
                Random.Range(0, totalWeight);

            int cumulativeWeight = 0;

            cumulativeWeight += commonWeight;

            if (randomValue < cumulativeWeight)
                return GachaRarity.Common;

            cumulativeWeight += rareWeight;

            if (randomValue < cumulativeWeight)
                return GachaRarity.Rare;

            cumulativeWeight += uniqueWeight;

            if (randomValue < cumulativeWeight)
                return GachaRarity.Unique;

            return GachaRarity.Epic;
        }

        // =========================
        // Item
        // =========================

        private GachaItem DrawItem(
            GachaGroup group,
            GachaRarity rarity,
            GachaPoolData poolData)
        {
            var candidates =
                poolData.GetItemsByRarity(rarity);

            if (candidates.Count == 0)
            {
            
                return null;
            }

            int totalWeight = 0;

            foreach (var item in candidates)
            {
                totalWeight += item.Weight;
            }

            if (totalWeight <= 0)
            {

                return candidates[0];
            }

            int randomValue =
                Random.Range(0, totalWeight);

            int cumulativeWeight = 0;

            foreach (var item in candidates)
            {
                cumulativeWeight += item.Weight;

                if (randomValue < cumulativeWeight)
                    return item;
            }

            return candidates[0];
        }

        // =========================
        // Pool
        // =========================

        public GachaPoolData GetPoolData(
            GachaGroup group)
        {
            if (!_pools.ContainsKey(group))
            {
                return null;
            }

            return _pools[group];
        }

        public Dictionary<GachaGroup, GachaPoolData>
            GetAllPools()
        {
            return new Dictionary<GachaGroup, GachaPoolData>(
                _pools
            );
        }
    }
}