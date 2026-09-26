using PixelRestaurant.Data;
using TMPro;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    public class GachaStatisticsUI : MonoBehaviour
    {
        [Header("Gacha Records")]
        [SerializeField] private TextMeshProUGUI weaponPullText;
        [SerializeField] private TextMeshProUGUI furniturePullText;
        [SerializeField] private TextMeshProUGUI recipePullText;
        [SerializeField] private TextMeshProUGUI totalPullText;

        [Header("Other Records")]
        [SerializeField] private TextMeshProUGUI goldPerMinuteText;
        [SerializeField] private TextMeshProUGUI totalGoldText;
        [SerializeField] private TextMeshProUGUI deathCountText;

        private void OnEnable()
        {
            Refresh();
        }

        // =========================
        // Refresh
        // =========================

        public void Refresh()
        {
            UpdateGachaRecords();
            UpdateOtherRecords();
        }

        // =========================
        // Gacha Records
        // =========================

        private void UpdateGachaRecords()
        {
            GachaPitySystem pitySystem =
                GachaPitySystem.Instance;

            if (pitySystem == null)
            {
                SetGachaTexts(0, 0, 0);
                return;
            }

            int weapon =
                pitySystem.GetTotalPullCount(
                    GachaGroup.Weapon
                );

            int furniture =
                pitySystem.GetTotalPullCount(
                    GachaGroup.Furniture
                );

            int recipe =
                pitySystem.GetTotalPullCount(
                    GachaGroup.Recipe
                );

            SetGachaTexts(
                weapon,
                furniture,
                recipe
            );
        }

        private void SetGachaTexts(
            int weapon,
            int furniture,
            int recipe)
        {
            long total =
                (long)weapon + furniture + recipe;

            if (weaponPullText != null)
                weaponPullText.text =
                    $"Weapon Pull : {weapon:N0}";

            if (furniturePullText != null)
                furniturePullText.text =
                    $"Furniture Pull : {furniture:N0}";

            if (recipePullText != null)
                recipePullText.text =
                    $"Recipe Pull : {recipe:N0}";

            if (totalPullText != null)
                totalPullText.text =
                    $"Total Pull : {total:N0}";
        }

        // =========================
        // Other Records
        // =========================

        private void UpdateOtherRecords()
        {
            // 골드 및 사망 기록은 추후 연결
            if (goldPerMinuteText != null)
                goldPerMinuteText.text =
                    "Gold / Min : -";

            if (totalGoldText != null)
                totalGoldText.text =
                    "Total Gold : -";

            if (deathCountText != null)
                deathCountText.text =
                    "Death Count : -";
        }
    }
}