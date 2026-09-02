using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Managers
{
    /// <summary>
    /// 가챠 시스템 매니저
    /// 1차: 레어리티를 가중치로 뽑기
    /// 2차: 해당 레어리티의 아이템을 가중치로 뽑기
    /// 골드 결제와 UI는 담당하지 않습니다.
    /// </summary>
    public class GachaManager : MonoBehaviour
    {
        public static GachaManager instance { get; private set; }

        [SerializeField] private GachaPool gachaPool;
        [SerializeField] private GachaConfig gachaConfig;

        public GameObject DefaultResultPrefab => gachaPool != null ? gachaPool.DefaultResultPrefab : null;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (gachaConfig == null)
            {
                gachaConfig = new GachaConfig();
                Debug.LogWarning("[가챠] GachaConfig가 할당되지 않았습니다. 기본값을 사용합니다.");
            }
        }

        /// <summary>
        /// 뽑기 횟수에 따른 비용 반환 (결제는 하지 않음)
        /// </summary>
        public int GetPullCost(int pullCount)
        {
            return pullCount switch
            {
                1 => gachaConfig.pull1Cost,
                5 => gachaConfig.pull5Cost,
                30 => gachaConfig.pull30Cost,
                _ => gachaConfig.pull1Cost
            };
        }

        /// <summary>
        /// 골드 소비 없이 가챠를 진행하고 결과만 반환
        /// 실패 시 재시행해서 반드시 결과 반환
        /// </summary>
        public GachaItem DrawGachaWithoutCost(GachaGroup group)
        {
            GachaItem selectedItem = null;
            int retryCount = 0;
            const int maxRetry = 100;

            while (selectedItem == null && retryCount < maxRetry)
            {
                GachaRarity selectedRarity = Draw1stRarity();
                selectedItem = Draw2ndItem(group, selectedRarity);

                if (selectedItem == null)
                {
                    retryCount++;
                }
            }

            if (selectedItem == null)
            {
                Debug.LogWarning($"[경고] {group} 그룹에서 유효한 아이템을 찾을 수 없습니다.");
            }

            return selectedItem;
        }

        /// <summary>
        /// 1차 뽑기: 레어리티를 가중치로 선택
        /// Common(60) : Rare(30) : Unique(9) : Epic(1)
        /// </summary>
        private GachaRarity Draw1stRarity()
        {
            int totalWeight = (int)GachaRarity.Common + (int)GachaRarity.Rare +
                             (int)GachaRarity.Unique + (int)GachaRarity.Epic;

            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;

            if (randomValue < cumulativeWeight + (int)GachaRarity.Common)
                return GachaRarity.Common;
            cumulativeWeight += (int)GachaRarity.Common;

            if (randomValue < cumulativeWeight + (int)GachaRarity.Rare)
                return GachaRarity.Rare;
            cumulativeWeight += (int)GachaRarity.Rare;

            if (randomValue < cumulativeWeight + (int)GachaRarity.Unique)
                return GachaRarity.Unique;
            cumulativeWeight += (int)GachaRarity.Unique;

            return GachaRarity.Epic;
        }

        /// <summary>
        /// 2차 뽑기: 해당 레어리티 + 그룹의 아이템을 가중치로 선택
        /// </summary>
        private GachaItem Draw2ndItem(GachaGroup group, GachaRarity rarity)
        {
            if (gachaPool == null)
            {
                Debug.LogError("[가챠] GachaPool이 할당되지 않았습니다.");
                return null;
            }

            List<GachaItem> candidates = gachaPool.GetItemsByGroupAndRarity(group, rarity);

            if (candidates.Count == 0)
            {
                return null;
            }

            int totalWeight = 0;
            foreach (var item in candidates)
            {
                totalWeight += item.weight;
            }

            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;

            foreach (var item in candidates)
            {
                cumulativeWeight += item.weight;
                if (randomValue < cumulativeWeight)
                {
                    return item;
                }
            }

            return candidates[0];
        }

        internal GachaItem DrawGacha(GachaGroup currentGachaType, int v)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 가챠 비용 설정 (직렬화)
    /// </summary>
    [System.Serializable]
    public class GachaConfig
    {
        [SerializeField] public int pull1Cost = 1;
        [SerializeField] public int pull5Cost = 5;
        [SerializeField] public int pull30Cost = 30;
    }
}
