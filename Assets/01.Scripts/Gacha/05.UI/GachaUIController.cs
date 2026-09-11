using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;
using PixelRestaurant.Managers;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 뽑기 팝업 화면 관리
    /// 그룹 선택, 뽑기 실행, 결과 표시, 천장 카운터 표시
    /// 1회/5회 뽑기만 가능
    /// </summary>
    public class GachaUIController : MonoBehaviour
    {
        // ========== 팝업 UI ==========
        [SerializeField] private Button gachaOpenButton;
        [SerializeField] private Button gachaCloseButton;
        [SerializeField] private GameObject gachaPopup;
        [SerializeField] private GachaInventoryUI inventoryUI;
 

        // ========== 그룹 선택 ==========
        [SerializeField] private Button weaponButton;
        [SerializeField] private Button furnitureButton;
        [SerializeField] private Button recipeButton;
        private GachaGroup _currentGachaType = GachaGroup.Weapon;

        // ========== 뽑기 버튼 ==========
        [SerializeField] private Button pull1Button;
        [SerializeField] private Button pull10Button; // 변경: 10연 버튼

        // ========== 정보 표시 ==========
        [SerializeField] private TextMeshProUGUI goldDisplay;
        [SerializeField] private TextMeshProUGUI currentGroupDisplay;

        // ========== 천장 표시 ==========
        [SerializeField] private TextMeshProUGUI pityLevelDisplay;
        [SerializeField] private TextMeshProUGUI pityCounterDisplay;
        [SerializeField] private Image pityProgressBar;

        // ========== 레어리티 확률 표시 ==========
        [SerializeField] private TextMeshProUGUI probabilityCommonDisplay;
        [SerializeField] private TextMeshProUGUI probabilityRareDisplay;
        [SerializeField] private TextMeshProUGUI probabilityUniqueDisplay;
        [SerializeField] private TextMeshProUGUI probabilityEpicDisplay;

        // ========== 결과 표시 영역 ==========
        [SerializeField] private Transform resultSpawnPoint;

        // ========== 비용 설정 ==========
        private const int CostPerPull = 100;  // 1회당 100 골드

        //private void Start()
        //{
        //    RegisterButtonEvents();
        //    UpdateDisplay();
        //}

        private void Update()
        {
            // 매 프레임 정보 갱신
            UpdateGoldDisplay();
        }

        /// <summary>
        /// 버튼 이벤트 등록
        /// </summary>
        //private void RegisterButtonEvents()
        //{
        //    if (gachaOpenButton != null) gachaOpenButton.onClick.AddListener(OpenGachaPopup);
        //    if (gachaCloseButton != null) gachaCloseButton.onClick.AddListener(CloseGachaPopup);

        //    // 그룹 선택 (각 버튼에 클릭 사운드 추가)
        //    if (weaponButton != null)
        //    {
        //        weaponButton.onClick.AddListener(() => SelectGachaType(GachaGroup.Weapon));
        //        weaponButton.onClick.AddListener(() => AudioManager.Instance?.PlaySFX("ui_click"));
        //    }

        //    if (furnitureButton != null)
        //    {
        //        furnitureButton.onClick.AddListener(() => SelectGachaType(GachaGroup.Furniture));
        //        furnitureButton.onClick.AddListener(() => AudioManager.Instance?.PlaySFX("ui_click"));
        //    }

        //    if (recipeButton != null)
        //    {
        //        recipeButton.onClick.AddListener(() => SelectGachaType(GachaGroup.Recipe));
        //        recipeButton.onClick.AddListener(() => AudioManager.Instance?.PlaySFX("ui_click"));
        //    }

        //    // 뽑기 실행 (1회, 5회)
        //    if (pull1Button != null)
        //    {
        //        pull1Button.onClick.AddListener(() => ExecuteGacha(1));
        //        pull1Button.onClick.AddListener(() => AudioManager.Instance?.PlaySFX("ui_click"));
        //    }

        //    if (pull5Button != null)
        //    {
        //        pull5Button.onClick.AddListener(() => ExecuteGacha(5));
        //        pull5Button.onClick.AddListener(() => AudioManager.Instance?.PlaySFX("ui_click"));
        //    }

        //    Debug.Log("[가챠 UI] 버튼 이벤트 등록 완료");
        //}

       
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
            {
                gachaPopup.SetActive(false);
                Debug.Log("[가챠 UI] 팝업 닫힘");
            }
        }

        private void SelectGachaType(GachaGroup group)
        {
            _currentGachaType = group;
            UpdateDisplay();
            Debug.Log($"[가챠 UI] 그룹 선택: {group}");
        }

        private void ExecuteGacha(int pullCount)
        {
            if (GachaManager.Instance == null)
            {
                Debug.LogError("[가챠 UI] GachaManager를 찾을 수 없습니다.");
                return;
            }

            // Validate allowed counts (redundant with manager but keeps UI safe)
            if (pullCount != 1 && pullCount != 5)
            {
                Debug.LogWarning("[가챠 UI] 지원되지 않는 뽑기 횟수입니다. (지원: 1, 5)");
                return;
            }

            // 뽑기 실행
            List<GachaItem> results = GachaManager.Instance.DrawGacha(
                _currentGachaType,
                pullCount,
                CostPerPull
            );

            if (results.Count == 0)
            {
                Debug.Log("[가챠 UI] 뽑기 실패 (골드 부족 또는 오류)");
                return;
            }

            // 인벤토리에 아이템 추가
            GachaInventory inventory = GachaInventory.Instance;
            foreach (var item in results)
            {
                inventory.AddItem(item.ItemId, 1);
            }

            // 결과 표시
            ShowGachaResults(results);

            // UI 갱신
            UpdateDisplay();

            Debug.Log($"[가챠 UI] {pullCount}회 뽑기 완료: {results.Count}개 획득");
        }

        private void ShowGachaResults(List<GachaItem> items)
        {
            ClearPreviousResults();

            if (resultSpawnPoint == null)
            {
                Debug.LogError("[가챠 UI] resultSpawnPoint가 할당되지 않았습니다.");
                return;
            }

            GachaResultHandler resultHandler = GachaResultHandler.Instance;
            if (resultHandler == null)
            {
                Debug.LogError("[가챠 UI] GachaResultHandler를 찾을 수 없습니다.");
                return;
            }

            foreach (var item in items)
            {
                resultHandler.HandleGachaResult(item, resultSpawnPoint);
            }

            Debug.Log($"[가챠 UI] {items.Count}개 결과 표시");
        }

        private void ClearPreviousResults()
        {
            if (resultSpawnPoint == null) return;
            foreach (Transform child in resultSpawnPoint) Destroy(child.gameObject);
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
            if (goldDisplay == null) return;

            // GoldManager 사용 (프로젝트에 CurrencyManager가 없으므로 GoldManager로)
            int gold = PixelRestaurant.Managers.CurrencyManager.Instance.GetCurrentGold();
            goldDisplay.text = $"Gold: {gold}";
        }

        private void UpdateGroupDisplay()
        {
            if (currentGroupDisplay == null) return;
            currentGroupDisplay.text = $"Current: {_currentGachaType}";
        }

        private void UpdatePityDisplay()
        {
            if (GachaPitySystem.Instance == null)
            {
                Debug.LogWarning("[가챠 UI] GachaPitySystem을 찾을 수 없습니다.");
                return;
            }

            GachaPoolData poolData = GachaManager.Instance.GetPoolData(_currentGachaType);
            if (poolData == null)
            {
                Debug.LogWarning($"[가챠 UI] {_currentGachaType} 풀을 찾을 수 없습니다.");
                return;
            }

            int level = GachaPitySystem.Instance.GetCurrentPityLevel(_currentGachaType);
            if (pityLevelDisplay != null) pityLevelDisplay.text = $"Level {level}";

            string counter = GachaPitySystem.Instance.GetPityDisplayText(_currentGachaType, poolData.PityConfig);
            if (pityCounterDisplay != null) pityCounterDisplay.text = counter;

            if (pityProgressBar != null)
            {
                float fillAmount = GachaPitySystem.Instance.GetPityProgressFillAmount(_currentGachaType, poolData.PityConfig);
                pityProgressBar.fillAmount = fillAmount;
            }
        }

        private void UpdateProbabilityDisplay()
        {
            if (GachaPitySystem.Instance == null) return;

            GachaPoolData poolData = GachaManager.Instance.GetPoolData(_currentGachaType);
            if (poolData == null) return;

            int level = GachaPitySystem.Instance.GetCurrentPityLevel(_currentGachaType);

            int commonWeight = poolData.GetWeight(level, GachaRarity.Common);
            int rareWeight = poolData.GetWeight(level, GachaRarity.Rare);
            int uniqueWeight = poolData.GetWeight(level, GachaRarity.Unique);
            int epicWeight = poolData.GetWeight(level, GachaRarity.Epic);

            int total = commonWeight + rareWeight + uniqueWeight + epicWeight;
            if (total <= 0) return;

            if (probabilityCommonDisplay != null) probabilityCommonDisplay.text = $"Common: {(commonWeight * 100 / total)}%";
            if (probabilityRareDisplay != null) probabilityRareDisplay.text = $"Rare: {(rareWeight * 100 / total)}%";
            if (probabilityUniqueDisplay != null) probabilityUniqueDisplay.text = $"Unique: {(uniqueWeight * 100 / total)}%";
            if (probabilityEpicDisplay != null) probabilityEpicDisplay.text = $"Epic: {(epicWeight * 100 / total)}%";
        }
    }
}