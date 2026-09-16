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
                        Debug.LogError(
                            "[가챠 매니저] GachaManager를 찾을 수 없습니다!"
                        );
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

            Debug.Log($"[가챠] CurrencyManager 확인: {CurrencyManager.instance}");

            if (CurrencyManager.instance != null)
            {
                _currencyProvider = CurrencyManager.instance;
                Debug.Log("[가챠] CurrencyProvider 연결 완료");
            }
            else
            {
                Debug.LogError("[가챠] CurrencyManager.instance가 아직 없습니다!");
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

            Debug.Log("[가챠 매니저] 초기화 완료");
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
                Debug.LogWarning(
                    "[가챠] 지원되지 않는 뽑기 횟수입니다. 지원: 1, 10"
                );

                return results;
            }

            if (_pools == null || !_pools.ContainsKey(group))
            {
                Debug.LogError(
                    $"[가챠] {group} 그룹의 풀을 찾을 수 없습니다."
                );

                return results;
            }

            if (_currencyProvider == null)
            {
                _currencyProvider = CurrencyManager.instance;

                if (_currencyProvider == null)
                {
                    Debug.LogError(
                        "[가챠] CurrencyManager를 찾을 수 없습니다!"
                    );

                    return results;
                }

                Debug.Log("[가챠] CurrencyProvider 자동 연결 완료");
            }

            if (_pitySystem == null)
            {
                Debug.LogError(
                    "[가챠] PitySystem이 설정되지 않았습니다!"
                );

                return results;
            }

            GachaPoolData poolData = _pools[group];

            Debug.Log(
                $"[가챠] {pullCount}회 뽑기 시작 " +
                $"({group}, 비용: {costPerPull} x {pullCount})"
            );

            // =========================
            // Pull
            // =========================

            for (int i = 0; i < pullCount; i++)
            {
                // Step 1
                // BigNumber 기준으로 골드 소비
                if (!_currencyProvider.SpendGold(costPerPull))
                {
                    Debug.Log(
                        $"[가챠] 골드 부족 ({i + 1}회에서 중단)"
                    );

                    break;
                }

                // Step 2
                // 현재 가챠 레벨 확인
                int currentLevel =
                    _pitySystem.GetCurrentPityLevel(group);

                // Step 3
                // 레어리티 결정
                GachaRarity rarity =
                    DrawRarity(
                        group,
                        currentLevel,
                        poolData
                    );

                // Step 4
                // 아이템 결정
                GachaItem item =
                    DrawItem(
                        group,
                        rarity,
                        poolData
                    );

                if (item == null)
                {
                    Debug.LogError(
                        "[가챠] 아이템을 찾을 수 없습니다."
                    );

                    break;
                }

                // Step 5
                // 뽑기 횟수 증가
                _pitySystem.IncreasePullCount(group);

                // Step 6
                // 레벨업 확인
                if (_pitySystem.CheckAndLevelUp(
                    group,
                    poolData.PityConfig))
                {
                    Debug.Log(
                        $"[가챠] {group} 천장 레벨업 → 확률 변경"
                    );
                }

                results.Add(item);
                _lastDrawnItem = item;

                Debug.Log(
                    $"[가챠] {i + 1}회: " +
                    $"{item.GetDisplayName()} 획득 ({rarity})"
                );
            }

            Debug.Log(
    $"[가챠] 뽑기 완료: {results.Count}개 획득"
);

            if (results.Count > 0)
            {
                OnGachaItemsDrawn?.Invoke(results);
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
                Debug.LogError(
                    $"[가챠] {group} Level {pityLevel}의 " +
                    "가중치가 0입니다!"
                );

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
                Debug.LogWarning(
                    $"[가챠] {group} {rarity} 아이템이 없습니다!"
                );

                return null;
            }

            int totalWeight = 0;

            foreach (var item in candidates)
            {
                totalWeight += item.Weight;
            }

            if (totalWeight <= 0)
            {
                Debug.LogError(
                    $"[가챠] {rarity} 아이템의 " +
                    "총 가중치가 0입니다!"
                );

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
                Debug.LogError(
                    $"[가챠] {group} 그룹의 풀을 찾을 수 없습니다."
                );

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