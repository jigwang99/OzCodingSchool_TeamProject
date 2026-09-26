using TMPro;
using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    public class GachaProbabilityLevelRow : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI rangeText;
        [SerializeField] private TextMeshProUGUI commonText;
        [SerializeField] private TextMeshProUGUI rareText;
        [SerializeField] private TextMeshProUGUI uniqueText;
        [SerializeField] private TextMeshProUGUI epicText;

        /// <summary>
        /// 확률표 한 줄을 설정한다.
        /// </summary>
        /// <param name="level">현재 천장 레벨 데이터</param>
        /// <param name="startPull">이 확률이 적용되기 시작하는 횟수</param>
        /// <param name="endPull">이 확률이 적용되는 마지막 횟수. -1이면 마지막 단계</param>
        public void Setup(
            GachaPityConfig.PityLevel level,
            int startPull,
            int endPull)
        {
            if (level == null)
                return;

            // -------------------------
            // 횟수 범위
            // -------------------------

            if (rangeText != null)
            {
                if (endPull < 0)
                    rangeText.text = $"Pull Count : {startPull}+";
                else if (startPull == endPull)
                    rangeText.text = $"Pull Count : {startPull}";
                else
                    rangeText.text = $"Pull Count : {startPull} ~ {endPull}";
            }
            // -------------------------
            // Weight 가져오기
            // -------------------------

            int commonWeight = GetWeight(level, GachaRarity.Common);
            int rareWeight = GetWeight(level, GachaRarity.Rare);
            int uniqueWeight = GetWeight(level, GachaRarity.Unique);
            int epicWeight = GetWeight(level, GachaRarity.Epic);

            int totalWeight =
                commonWeight +
                rareWeight +
                uniqueWeight +
                epicWeight;

            // -------------------------
            // 확률 표시
            // -------------------------

            if (totalWeight <= 0)
            {
                SetProbabilityTexts(0f, 0f, 0f, 0f);
                return;
            }

            float commonProbability =
                (float)commonWeight / totalWeight * 100f;

            float rareProbability =
                (float)rareWeight / totalWeight * 100f;

            float uniqueProbability =
                (float)uniqueWeight / totalWeight * 100f;

            float epicProbability =
                (float)epicWeight / totalWeight * 100f;

            SetProbabilityTexts(
                commonProbability,
                rareProbability,
                uniqueProbability,
                epicProbability
            );
        }

        private int GetWeight(
            GachaPityConfig.PityLevel level,
            GachaRarity rarity)
        {
            foreach (GachaPityConfig.RarityWeight rarityWeight in level.Rarities)
            {
                if (rarityWeight.Rarity == rarity)
                    return rarityWeight.Weight;
            }

            return 0;
        }

        private void SetProbabilityTexts(
         float common,
         float rare,
         float unique,
         float epic)
        {
            if (commonText != null)
                commonText.text = $"COMMON\n{common:0.##}%";

            if (rareText != null)
                rareText.text = $"RARE\n{rare:0.##}%";

            if (uniqueText != null)
                uniqueText.text = $"UNIQUE\n{unique:0.##}%";

            if (epicText != null)
                epicText.text = $"EPIC\n{epic:0.##}%";
        }
    }
}