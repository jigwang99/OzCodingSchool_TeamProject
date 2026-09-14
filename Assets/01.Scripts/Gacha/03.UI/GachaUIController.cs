using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;
using PixelRestaurant.Managers;

namespace PixelRestaurant.Gacha
{
    public class GachaUIController : MonoBehaviour
    {
        [Header("Gacha Open / Close")]
        [SerializeField] private Button gachaOpenButton;
        [SerializeField] private Button gachaCloseButton;
        [SerializeField] private GameObject gachaPopup;
        [SerializeField] private GachaInventoryUI inventoryUI;

        [Header("Gacha Type")]
        [SerializeField] private Button weaponButton;
        [SerializeField] private Button furnitureButton;
        [SerializeField] private Button recipeButton;

        private GachaGroup _currentGachaType = GachaGroup.Weapon;

        [Header("Pull Buttons")]
        [SerializeField] private Button pull1Button;
        [SerializeField] private Button pull10Button;

        [Header("Gold")]
        [SerializeField] private TextMeshProUGUI goldDisplay;

        [Header("Current Group")]
        [SerializeField] private TextMeshProUGUI currentGroupDisplay;

        [Header("Pity")]
        [SerializeField] private TextMeshProUGUI pityLevelDisplay;
        [SerializeField] private TextMeshProUGUI pityCounterDisplay;
        [SerializeField] private Image pityProgressBar;

        [Header("Probability")]
        [SerializeField] private TextMeshProUGUI probabilityCommonDisplay;
        [SerializeField] private TextMeshProUGUI probabilityRareDisplay;
        [SerializeField] private TextMeshProUGUI probabilityUniqueDisplay;
        [SerializeField] private TextMeshProUGUI probabilityEpicDisplay;

        [Header("Result")]
        [SerializeField] private Transform resultSpawnPoint;

        private const int CostPerPull = 10;

        private void Start()
        {
            RegisterButtonEvents();
            UpdateDisplay();
        }

        private void Update()
        {
            UpdateGoldDisplay();
        }

        private void RegisterButtonEvents()
        {
            if (gachaOpenButton != null)
                gachaOpenButton.onClick.AddListener(OpenGachaPopup);

            if (gachaCloseButton != null)
                gachaCloseButton.onClick.AddListener(CloseGachaPopup);

            if (weaponButton != null)
                weaponButton.onClick.AddListener(() =>
                    SelectGachaType(GachaGroup.Weapon));

            if (furnitureButton != null)
                furnitureButton.onClick.AddListener(() =>
                    SelectGachaType(GachaGroup.Furniture));

            if (recipeButton != null)
                recipeButton.onClick.AddListener(() =>
                    SelectGachaType(GachaGroup.Recipe));

            if (pull1Button != null)
                pull1Button.onClick.AddListener(() =>
                    ExecuteGacha(1));

            if (pull10Button != null)
                pull10Button.onClick.AddListener(() =>
                    ExecuteGacha(10));
        }

        public void OpenGachaPopup()
        {
            if (inventoryUI != null)
                inventoryUI.CloseInventory();

            if (gachaPopup != null)
            {
                gachaPopup.SetActive(true);
                UpdateDisplay();
            }
        }

        private void CloseGachaPopup()
        {
            if (gachaPopup != null)
                gachaPopup.SetActive(false);
        }

        private void SelectGachaType(GachaGroup group)
        {
            _currentGachaType = group;

            Debug.Log($"[GachaUI] 가챠 종류 변경: {group}");

            UpdateDisplay();
        }

        private void ExecuteGacha(int pullCount)
        {
            Debug.Log($"[GachaUI] 가챠 버튼 클릭: {pullCount}회");

            if (GachaManager.Instance == null)
            {
                Debug.LogError("[GachaUI] GachaManager.Instance가 없습니다.");
                return;
            }

            if (PixelRestaurant.Managers.CurrencyManager.Instance == null)
            {
                Debug.LogError("[GachaUI] CurrencyManager.Instance가 없습니다.");
                return;
            }

            if (pullCount != 1 && pullCount != 10)
            {
                Debug.LogWarning("[GachaUI] 1회 또는 10회 뽑기만 가능합니다.");
                return;
            }

            List<GachaItem> results =
                GachaManager.Instance.DrawGacha(
                    _currentGachaType,
                    pullCount,
                    CostPerPull
                );

            if (results == null || results.Count == 0)
            {
                Debug.LogWarning("[GachaUI] 가챠 결과가 없습니다.");
                return;
            }

            ShowGachaResults(results);

            UpdateDisplay();
        }

        private void ShowGachaResults(List<GachaItem> items)
        {
            ClearPreviousResults();

            if (resultSpawnPoint == null)
            {
                Debug.LogError("[GachaUI] ResultSpawnPoint가 연결되지 않았습니다.");
                return;
            }

            GachaResultHandler resultHandler =
                GachaResultHandler.Instance;

            if (resultHandler == null)
            {
                Debug.LogError("[GachaUI] GachaResultHandler.Instance가 없습니다.");
                return;
            }

            foreach (GachaItem item in items)
            {
                if (item == null)
                    continue;

                resultHandler.HandleGachaResult(
                    item,
                    resultSpawnPoint
                );
            }
        }

        private void ClearPreviousResults()
        {
            if (resultSpawnPoint == null)
                return;

            foreach (Transform child in resultSpawnPoint)
            {
                Destroy(child.gameObject);
            }
        }

        private void UpdateDisplay()
        {
            UpdateGoldDisplay();
            UpdateGroupDisplay();
            UpdatePityDisplay();
            UpdateProbabilityDisplay();
        }

        private void UpdateGoldDisplay()
        {
            if (goldDisplay == null)
                return;

            if (PixelRestaurant.Managers.CurrencyManager.Instance == null)
            {
                goldDisplay.text = "0";
                return;
            }

            goldDisplay.text =
                PixelRestaurant.Managers.CurrencyManager.Instance.GetCurrentGold().ToString();
        }

        private void UpdateGroupDisplay()
        {
            if (currentGroupDisplay == null)
                return;

            switch (_currentGachaType)
            {
                case GachaGroup.Weapon:
                    currentGroupDisplay.text = "무기";
                    break;

                case GachaGroup.Furniture:
                    currentGroupDisplay.text = "가구";
                    break;

                case GachaGroup.Recipe:
                    currentGroupDisplay.text = "레시피";
                    break;
            }
        }

        private void UpdatePityDisplay()
        {
            if (GachaPitySystem.Instance == null)
                return;

            GachaPoolData pool =
                GachaManager.Instance != null
                    ? GachaManager.Instance.GetPoolData(_currentGachaType)
                    : null;

            if (pool == null || pool.PityConfig == null)
                return;

            GachaPityConfig config = pool.PityConfig;

            int level =
                GachaPitySystem.Instance.GetCurrentPityLevel(
                    _currentGachaType
                );

            if (pityLevelDisplay != null)
                pityLevelDisplay.text = $"Lv.{level}";

            if (pityCounterDisplay != null)
            {
                pityCounterDisplay.text =
                    GachaPitySystem.Instance.GetPityDisplayText(
                        _currentGachaType,
                        config
                    );
            }

            if (pityProgressBar != null)
            {
                pityProgressBar.fillAmount =
                    GachaPitySystem.Instance.GetPityProgressFillAmount(
                        _currentGachaType,
                        config
                    );
            }
        }

        private void UpdateProbabilityDisplay()
        {
            if (GachaManager.Instance == null)
                return;

            GachaPoolData pool =
                GachaManager.Instance.GetPoolData(
                    _currentGachaType
                );

            if (pool == null || pool.PityConfig == null)
                return;

            GachaPityConfig config = pool.PityConfig;

            UpdateRarityText(
                probabilityCommonDisplay,
                GachaRarity.Common,
                config
            );

            UpdateRarityText(
                probabilityRareDisplay,
                GachaRarity.Rare,
                config
            );

            UpdateRarityText(
                probabilityUniqueDisplay,
                GachaRarity.Unique,
                config
            );

            UpdateRarityText(
                probabilityEpicDisplay,
                GachaRarity.Epic,
                config
            );
        }

        private void UpdateRarityText(
      TextMeshProUGUI text,
      GachaRarity rarity,
      GachaPityConfig config)
        {
            if (text == null || config == null)
                return;

            float weight =
                GachaPitySystem.Instance.GetRarityWeight(
                    _currentGachaType,
                    config,
                    rarity
                );

            text.text = $"{rarity}: {weight:0.##}%";
        }

        private void OnDestroy()
        {
            if (gachaOpenButton != null)
                gachaOpenButton.onClick.RemoveListener(OpenGachaPopup);

            if (gachaCloseButton != null)
                gachaCloseButton.onClick.RemoveListener(CloseGachaPopup);

            if (weaponButton != null)
                weaponButton.onClick.RemoveAllListeners();

            if (furnitureButton != null)
                furnitureButton.onClick.RemoveAllListeners();

            if (recipeButton != null)
                recipeButton.onClick.RemoveAllListeners();

            if (pull1Button != null)
                pull1Button.onClick.RemoveAllListeners();

            if (pull10Button != null)
                pull10Button.onClick.RemoveAllListeners();
        }
    }
}