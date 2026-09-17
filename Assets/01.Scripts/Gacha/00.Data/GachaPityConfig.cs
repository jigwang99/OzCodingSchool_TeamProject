using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 천장 시스템의 설정 데이터
    /// "몇 번 뽑으면 레벨업, 각 레벨의 확률은 얼마"인지 정의
    /// GachaPoolData 내부에 포함됨
    /// </summary>
    [System.Serializable]
    public class GachaPityConfig
    {
        /// <summary>
        /// 특정 레어리티의 가중치
        /// </summary>
        [System.Serializable]
        public class RarityWeight
        {
            [SerializeField]
            private GachaRarity rarity;

            [SerializeField]
            private int weight;

            public GachaRarity Rarity => rarity;
            public int Weight => weight;

            public RarityWeight(GachaRarity rarity, int weight)
            {
                this.rarity = rarity;
                this.weight = weight;
            }
        }

        /// <summary>
        /// 특정 천장 레벨의 설정
        /// </summary>
        [System.Serializable]
        public class PityLevel
        {
            [SerializeField]
            private int level;

            [SerializeField]
            private int requiredPullCount;

            [SerializeField]
            private List<RarityWeight> rarities = new List<RarityWeight>();

            public int Level => level;
            public int RequiredPullCount => requiredPullCount;
            public List<RarityWeight> Rarities => rarities;

            public PityLevel(int level, int requiredPullCount)
            {
                this.level = level;
                this.requiredPullCount = requiredPullCount;
            }
        }

        [SerializeField]
        private List<PityLevel> levels = new List<PityLevel>();

        public List<PityLevel> Levels => levels;

        /// <summary>
        /// 특정 레벨의 특정 레어리티 가중치 조회
        /// </summary>
        /// <param name="level">천장 레벨</param>
        /// <param name="rarity">레어리티</param>
        /// <returns>가중치 (없으면 0)</returns>
        public int GetWeight(int level, GachaRarity rarity)
        {
            var pityLevel = levels.Find(l => l.Level == level);
            if (pityLevel == null)
            {
                Debug.LogWarning($"[천장] Level {level}을 찾을 수 없습니다.");
                return 0;
            }

            var rarityWeight = pityLevel.Rarities.Find(r => r.Rarity == rarity);
            if (rarityWeight == null)
            {
                Debug.LogWarning($"[천장] Level {level}에서 {rarity}를 찾을 수 없습니다.");
                return 0;
            }

            return rarityWeight.Weight;
        }

        /// <summary>
        /// 다음 레벨까지 필요한 뽑기 횟수 조회
        /// </summary>
        /// <param name="currentLevel">현재 레벨</param>
        /// <returns>다음 레벨까지 필요한 횟수</returns>
        public int GetRequiredCountForNextLevel(int currentLevel)
        {
            var nextLevel = levels.Find(l => l.Level == currentLevel + 1);
            if (nextLevel == null)
            {
                return int.MaxValue;  // 마지막 레벨
            }

            return nextLevel.RequiredPullCount;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 데이터 검증
        /// </summary>
        [ContextMenu("Validate Pity Config")]
        private void ValidatePityConfig()
        {
            if (levels.Count == 0)
            {
                Debug.LogWarning("[천장] Level이 하나도 없습니다!");
                return;
            }

            // Level 1부터 시작하는지 확인
            if (levels[0].Level != 1)
            {
                Debug.LogError("[천장] Level은 1부터 시작해야 합니다!");
            }

            // 각 Level마다 모든 Rarity 가중치 확인
            foreach (var level in levels)
            {
                if (level.Rarities.Count != 4)
                {
                    Debug.LogWarning($"[천장] Level {level.Level}에서 Rarity가 4개가 아닙니다!");
                }
            }

            Debug.Log($"[천장] 검증 완료: {levels.Count}개 레벨");
        }
#endif
    }
}
