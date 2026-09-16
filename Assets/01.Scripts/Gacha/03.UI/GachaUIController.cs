using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;


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

        [Header("Popup")]
        [SerializeField] private GameObject resultPopup;
        [SerializeField] private GameObject infoPopup;
        [SerializeField] private GameObject listPopup;

        [Header("Popup Buttons")]
        [SerializeField] private Button resultOpenButton;
        [SerializeField] private Button resultCloseButton;
        [SerializeField] private Button inventoryOpenButton;
        [SerializeField] private Button listOpenButton;

        private static readonly BigNumber CostPerPull =
    new BigNumber(30);

        private void Start()
        {
            UpdateDisplay();
        }

        private void Update()
        {
            UpdateGoldDisplay();
        }

        // =========================
        // Gacha Open / Close
        // =========================

        public void OpenGachaPopup()
        {
            CloseAllPopups();

            if (gachaPopup != null)
            {
                gachaPopup.SetActive(true);
                UpdateDisplay();
            }
        }
        public void CloseAllPopups()
        {
            if (gachaPopup != null)
                gachaPopup.SetActive(false);

            if (resultPopup != null)
                resultPopup.SetActive(false);

            if (infoPopup != null)
                infoPopup.SetActive(false);

            if (listPopup != null)
                listPopup.SetActive(false);

            if (inventoryUI != null)
                inventoryUI.CloseInventory();
        }

        // =========================
        // Gacha Type
        // =========================
        public void CloseGachaPopup()
        {
            CloseAllPopups();
        }
        public void SelectWeapon()
        {
            SelectGachaType(GachaGroup.Weapon);
        }

        public void SelectFurniture()
        {
            SelectGachaType(GachaGroup.Furniture);
        }

        public void SelectRecipe()
        {
            SelectGachaType(GachaGroup.Recipe);
        }

        private void SelectGachaType(GachaGroup group)
        {
            _currentGachaType = group;

            Debug.Log($"[GachaUI] 가챠 종류 변경: {group}");

            UpdateDisplay();
        }
        public void OpenResultPopup()
        {
            if (resultPopup != null)
                resultPopup.SetActive(true);
        }

        public void CloseResultPopup()
        {
            if (resultPopup != null)
                resultPopup.SetActive(false);
        }

        // =========================
        // Pull
        // =========================

        public void Pull1()
        {
            ExecuteGacha(1);

            if (resultPopup != null)
                resultPopup.SetActive(true);
        }

        public void Pull10()
        {
            ExecuteGacha(10);

            if (resultPopup != null)
                resultPopup.SetActive(true);
        }

        private void ExecuteGacha(int pullCount)
        {
            Debug.Log($"[GachaUI] 가챠 버튼 클릭: {pullCount}회");

            if (GachaManager.Instance == null)
            {
                Debug.LogError("[GachaUI] GachaManager.Instance가 없습니다.");
                return;
            }

            if (CurrencyManager.instance == null)
            {
                Debug.LogError("[GachaUI] CurrencyManager.instance가 없습니다.");
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

            // =====================================
            // 뽑은 아이템 인벤토리에 추가
            // =====================================
            ShowGachaResults(results);

            if (resultPopup != null)
                resultPopup.SetActive(true);

            UpdateDisplay();

            // =====================================
            // 결과 화면 표시
            // =====================================

            ShowGachaResults(results);

            if (resultPopup != null)
                resultPopup.SetActive(true);

            UpdateDisplay();
        }

        // =========================
        // Result
        // =========================

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

            GachaResultHandler resultHandler =
                GachaResultHandler.Instance;

            if (resultHandler == null)
            {
                Debug.LogError("[GachaUI] GachaResultHandler.Instance가 없습니다.");
                return;
            }

            GachaObjectPool objectPool =
                resultHandler.GetComponent<GachaObjectPool>();

            if (objectPool == null)
            {
                Debug.LogError("[GachaUI] GachaObjectPool을 찾을 수 없습니다.");
                return;
            }

            List<GameObject> resultCards = new List<GameObject>();

            foreach (Transform child in resultSpawnPoint)
            {
                resultCards.Add(child.gameObject);
            }

            foreach (GameObject card in resultCards)
            {
                objectPool.ReturnObject(card);
            }
        }
        // =========================
        // Display
        // =========================

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

            if (CurrencyManager.instance == null)
            {
                goldDisplay.text = "0";
                return;
            }

            goldDisplay.text =
                CurrencyManager.instance
                    .GetCurrentGold()
                    .ToString();
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
        public void OpenInventoryPopup()
        {
            CloseAllPopups();

            if (inventoryUI != null)
                inventoryUI.OpenInventory();
        }
        public void OpenListPopup()
        {
            CloseAllPopups();

            if (listPopup != null)
                listPopup.SetActive(true);
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

            text.text = $"{rarity}: \n{weight:0.##}%";
        }
    }
}