using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    public class GachaProbabilityListUI : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button weaponTabButton;
        [SerializeField] private Button furnitureTabButton;
        [SerializeField] private Button recipeTabButton;

        [Header("Level Page")]
        [SerializeField] private Transform levelPage;
        [SerializeField] private GachaProbabilityLevelRow levelRowPrefab;

        [Header("Pagination")]
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private TextMeshProUGUI pageText;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI currentPityText;

        private GachaGroup currentGroup = GachaGroup.Weapon;

        private GachaPityConfig currentConfig;

        // 현재 보고 있는 Level의 인덱스
        private int currentPage = 0;

        private void Start()
        {
            ShowWeapon();
        }

        // =========================
        // Tab
        // =========================

        public void ShowWeapon()
        {
            ShowGroup(GachaGroup.Weapon);
        }

        public void ShowFurniture()
        {

            ShowGroup(GachaGroup.Furniture);
        }

        public void ShowRecipe()
        {
            ShowGroup(GachaGroup.Recipe);
        }

        private void ShowGroup(GachaGroup group)
        {
            currentGroup = group;

            // 탭을 바꾸면 첫 페이지로
            currentPage = 0;

            LoadCurrentConfig();
        }

        // =========================
        // Config
        // =========================

        private void LoadCurrentConfig()
        {
            ClearPage();

            if (GachaManager.Instance == null)
            {
              
                return;
            }

            GachaPoolData poolData =
                GachaManager.Instance.GetPoolData(currentGroup);

            if (poolData == null)
            {
               

                return;
            }

            currentConfig = poolData.PityConfig;

            if (currentConfig == null)
            {
              

                return;
            }

            UpdateTitle();
            UpdateCurrentPity();

            ShowCurrentPage();
        }

        // =========================
        // Page
        // =========================

        private void ShowCurrentPage()
        {
            ClearPage();

            if (currentConfig == null)
                return;

            if (currentConfig.Levels == null ||
                currentConfig.Levels.Count == 0)
            {
                UpdatePageUI();
                return;
            }

            currentPage = Mathf.Clamp(
                currentPage,
                0,
                currentConfig.Levels.Count - 1
            );

            GachaPityConfig.PityLevel level =
                currentConfig.Levels[currentPage];

            int startPull;
            int endPull;

            // 현재 레벨이 시작되는 횟수
            if (currentPage == 0)
            {
                startPull = 0;
            }
            else
            {
                startPull =
                    currentConfig.Levels[currentPage]
                        .RequiredPullCount;
            }

            // 다음 레벨 직전까지 현재 확률 적용
            if (currentPage + 1 < currentConfig.Levels.Count)
            {
                endPull =
                    currentConfig.Levels[currentPage + 1]
                        .RequiredPullCount - 1;
            }
            else
            {
                // 마지막 Level
                endPull = -1;
            }

            if (levelRowPrefab != null &&
                levelPage != null)
            {
                GachaProbabilityLevelRow row =
                    Instantiate(
                        levelRowPrefab,
                        levelPage
                    );

                row.Setup(
                    level,
                    startPull,
                    endPull
                );
            }

            UpdatePageUI();
        }

        // =========================
        // Previous / Next
        // =========================

        public void PreviousPage()
        {
            if (currentConfig == null)
                return;

            if (currentPage <= 0)
                return;

            currentPage--;

            ShowCurrentPage();
        }

        public void NextPage()
        {
            if (currentConfig == null)
                return;

            if (currentPage >= currentConfig.Levels.Count - 1)
                return;

            currentPage++;

            ShowCurrentPage();
        }

        // =========================
        // Page UI
        // =========================

        private void UpdatePageUI()
        {
            int totalPages = 0;

            if (currentConfig != null &&
                currentConfig.Levels != null)
            {
                totalPages = currentConfig.Levels.Count;
            }

            if (pageText != null)
            {
                if (totalPages > 0)
                {
                    pageText.text =
                        $"{currentPage + 1} / {totalPages}";
                }
                else
                {
                    pageText.text = "0 / 0";
                }
            }

            if (prevPageButton != null)
            {
                prevPageButton.interactable =
                    totalPages > 0 &&
                    currentPage > 0;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable =
                    totalPages > 0 &&
                    currentPage < totalPages - 1;
            }
        }

        // =========================
        // Current Pity
        // =========================

        private void UpdateCurrentPity()
        {
            if (currentPityText == null)
                return;

            if (GachaPitySystem.Instance == null ||
                currentConfig == null)
            {
                currentPityText.text = "";
                return;
            }

            int currentLevel =
                GachaPitySystem.Instance
                    .GetCurrentPityLevel(currentGroup);

            int currentCount =
                GachaPitySystem.Instance
                    .GetCurrentUICount(currentGroup);

            int requiredCount =
                currentConfig
                    .GetRequiredCountForNextLevel(currentLevel);

            if (requiredCount == int.MaxValue)
            {
                currentPityText.text =
                    $"Current Level : {currentLevel} / MAX";
            }
            else
            {
                currentPityText.text =
                    $"Current Level : {currentLevel}   " +
                    $"Progress : {currentCount}/{requiredCount}";
            }
        }
        // =========================
        // Title
        // =========================
        private void UpdateTitle()
        {
            if (titleText == null)
                return;

            switch (currentGroup)
            {
                case GachaGroup.Weapon:
                    titleText.text = "WEAPON RATES";
                    break;

                case GachaGroup.Furniture:
                    titleText.text = "FURNITURE RATES";
                    break;

                case GachaGroup.Recipe:
                    titleText.text = "RECIPE RATES";
                    break;
            }
        }

        // =========================
        // Clear
        // =========================
    
        private void ClearPage()
        {
            if (levelPage == null)
                return;

            for (int i = levelPage.childCount - 1;
                 i >= 0;
                 i--)
            {
                Destroy(
                    levelPage.GetChild(i).gameObject
                );
            }
        }
    }
}