
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠 인벤토리 UI 관리
    ///
    /// - 인벤토리 열기 / 닫기
    /// - 가챠 팝업과 상호 배타적으로 열기
    /// - Weapon / Furniture / Recipe 탭
    /// - 아이템 카드 생성
    /// - 아이템 개수 표시
    /// - NEW 표시
    /// </summary>
    public class GachaInventoryUI : MonoBehaviour
    {
        // =========================================================
        // 인벤토리 팝업
        // =========================================================

        [Header("인벤토리 팝업")]
        [SerializeField] private GameObject inventoryPopup;

        // =========================================================
        // 가챠 UI
        // =========================================================

        [Header("가챠 UI")]
        [SerializeField] private GachaUIController gachaUI;

        [Header("테스트")]
        [SerializeField] private Button clearGachaInventoryButton;

        // =========================================================
        // 인벤토리 탭
        // =========================================================

        [Header("인벤토리 탭")]
        [SerializeField] private Button weaponTab;
        [SerializeField] private Button furnitureTab;
        [SerializeField] private Button recipeTab;

        [Header("Merge All (Inventory Popup)")]
        [SerializeField] private Button mergeAllButton;
        [SerializeField] private PlayerWeaponCatalog weaponCatalog;
        [SerializeField] private TextMeshProUGUI mergeResultText;
        private bool _merging;

        private GachaGroup _currentTab =
            GachaGroup.Weapon;

        // =========================================================
        // 인벤토리 카드
        // =========================================================

        [Header("인벤토리 카드")]
        [SerializeField] private Transform inventoryGrid;

        [SerializeField] private GameObject itemCardPrefab;

        [Header("Inventory Pagination")]
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private TMPro.TextMeshProUGUI pageText;

        [SerializeField] private int itemsPerPage = 20;

        private int currentPage = 0;
        private int totalPages = 1;
        // =========================================================
        // Unity
        // =========================================================

        private void Start()
        {


            if (mergeAllButton != null)
                mergeAllButton.onClick.AddListener(MergeAllWeapons);
            RefreshInventoryDisplay();
        }

        private void OnDestroy()
        {
            if (mergeAllButton != null)
                mergeAllButton.onClick.RemoveListener(MergeAllWeapons);
        }

        public void MergeAllWeapons()
        {
            if (_merging || GachaInventory.Instance == null || GachaManager.Instance == null) return;
            GachaPoolData pool = GachaManager.Instance.GetPoolData(GachaGroup.Weapon);
            if (pool == null || weaponCatalog == null)
            {
                if (mergeResultText != null) mergeResultText.text = "무기 풀/카탈로그 연결을 확인하세요.";
                return;
            }
            _merging = true;
            try
            {
                int merged = GachaInventory.Instance.TryMergeAllWeapons(pool, weaponCatalog);
                if (merged == 0)
                if (mergeResultText != null)
                    mergeResultText.text = merged > 0 ? $"무기 {merged}회 합치기 완료" : "합칠 수 있는 무기가 없습니다.";
            }
            finally
            {
                _merging = false;
                RefreshInventoryDisplay();
            }
        }

        private void UpdateMergeAllButton()
        {
            if (mergeAllButton == null) return;
            bool weaponTabSelected = _currentTab == GachaGroup.Weapon;
            mergeAllButton.gameObject.SetActive(weaponTabSelected);
            if (!weaponTabSelected) return;
            GachaPoolData pool = GachaManager.Instance != null
                ? GachaManager.Instance.GetPoolData(GachaGroup.Weapon) : null;
            // Do not disable the whole button because one weapon is unmergeable.
            // The inventory merge routine checks each weapon independently and skips failures.
            bool hasFiveOrMore = false;
            if (pool != null && pool.Items != null && GachaInventory.Instance != null)
            {
                foreach (GachaItem item in pool.Items)
                {
                    if (item == null || item.Group != GachaGroup.Weapon) continue;
                    if (GachaInventory.Instance.GetItemCount(item.ItemId) < GachaWeaponMerge.RequiredCount) continue;
                    hasFiveOrMore = true;
                    break;
                }
            }
            mergeAllButton.interactable = !_merging && weaponCatalog != null && hasFiveOrMore;
        }
        // =========================================================
        // 탭 이벤트
        // =========================================================

        private void RegisterTabEvents()
        {
            if (weaponTab != null)
            {
                weaponTab.onClick.AddListener(
                    () => SelectTab(GachaGroup.Weapon)
                );
            }

            if (furnitureTab != null)
            {
                furnitureTab.onClick.AddListener(
                    () => SelectTab(GachaGroup.Furniture)
                );
            }

            if (recipeTab != null)
            {
                recipeTab.onClick.AddListener(
                    () => SelectTab(GachaGroup.Recipe)
                );
            }

        }

        // =========================================================
        // 인벤토리 열기
        // =========================================================

        public void OpenInventory()
        {
            // -----------------------------------------------------
            // 가챠 팝업이 열려 있다면 닫기
            // -----------------------------------------------------

            if (gachaUI != null)
            {
                gachaUI.OpenGachaPopup();
            }

            // -----------------------------------------------------
            // 인벤토리 열기
            // -----------------------------------------------------

            if (inventoryPopup == null)
            {


                return;
            }

            inventoryPopup.SetActive(true);

            RefreshInventoryDisplay();


        }

        // =========================================================
        // 인벤토리 닫기
        // =========================================================

        public void CloseInventory()
        {
            if (inventoryPopup == null)
                return;

            inventoryPopup.SetActive(false);

        }

        // =========================================================
        // 인벤토리 상태
        // =========================================================

        public bool IsInventoryOpen()
        {
            return inventoryPopup != null &&
                   inventoryPopup.activeSelf;
        }

        // =========================================================
        // 탭 선택
        // =========================================================

        private void SelectTab(GachaGroup group)
        {
            _currentTab = group;

            currentPage = 0;

            RefreshInventoryDisplay();
        }
        public void SelectWeaponTab()
        {
            SelectTab(GachaGroup.Weapon);
        }

        public void SelectFurnitureTab()
        {
            SelectTab(GachaGroup.Furniture);
        }

        public void SelectRecipeTab()
        {
            SelectTab(GachaGroup.Recipe);
        }
        // =========================================================
        // 인벤토리 갱신
        // =========================================================

        public void RefreshInventoryDisplay()
        {
            GachaInventory inventory =
                GachaInventory.Instance;

            if (inventory == null)
            {


                return;
            }

            if (GachaManager.Instance == null)
            {
                return;
            }

            GachaPoolData poolData =
                GachaManager.Instance.GetPoolData(
                    _currentTab
                );

            if (poolData == null)
            {


                return;
            }

            // -----------------------------------------------------
            // 기존 카드 삭제
            // -----------------------------------------------------

            ClearInventoryCards();

            // -----------------------------------------------------
            // 현재 탭의 아이템 조회
            // -----------------------------------------------------

            List<string> itemIds =
                inventory.GetItemsInGroup(
                    _currentTab,
                    poolData
                );
            itemIds.Sort((idA, idB) =>
            {
                GachaItem itemA =
                    poolData.Items.Find(i => i.ItemId == idA);

                GachaItem itemB =
                    poolData.Items.Find(i => i.ItemId == idB);

                if (itemA == null)
                    return 1;

                if (itemB == null)
                    return -1;

                // 1순위: 레어도
                int rarityCompare =
                    GetRarityOrder(itemA.Rarity)
                    .CompareTo(
                        GetRarityOrder(itemB.Rarity)
                    );

                if (rarityCompare != 0)
                    return rarityCompare;

                // 2순위: 등급
                int gradeCompare =
                    itemA.Grade.CompareTo(itemB.Grade);

                if (gradeCompare != 0)
                    return gradeCompare;

                // 3순위: 같은 경우 이름순
                return string.Compare(
                    itemA.ItemName,
                    itemB.ItemName,
                    System.StringComparison.Ordinal
                );
            });


            // -----------------------------------------------------
            // 카드 생성
            // -----------------------------------------------------
            // 전체 페이지 수 계산
            totalPages = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    itemIds.Count / (float)itemsPerPage
                )
            );

            // 현재 페이지가 범위를 벗어나지 않도록 조정
            currentPage = Mathf.Clamp(
                currentPage,
                0,
                totalPages - 1
            );

            // 현재 페이지에 표시할 아이템 범위
            int startIndex = currentPage * itemsPerPage;

            int endIndex = Mathf.Min(
                startIndex + itemsPerPage,
                itemIds.Count
            );

            // 현재 페이지의 아이템만 생성
            for (int i = startIndex; i < endIndex; i++)
            {
                CreateItemCard(
                    itemIds[i],
                    inventory,
                    poolData
                );
                // 페이지 번호 및 버튼 갱신

            }
            UpdatePageUI();
            UpdateMergeAllButton();
        }

        // =========================================================
        // 기존 카드 제거
        // =========================================================

        private void ClearInventoryCards()
        {
            if (inventoryGrid == null)
                return;

            for (int i = inventoryGrid.childCount - 1;
                 i >= 0;
                 i--)
            {
                Transform child =
                    inventoryGrid.GetChild(i);

                Destroy(child.gameObject);
            }
        }

        // =========================================================
        // 아이템 카드 생성
        // =========================================================

        private void CreateItemCard(
            string itemId,
            GachaInventory inventory,
            GachaPoolData poolData)
        {
            if (itemCardPrefab == null)
            {
                return;
            }

            if (inventoryGrid == null)
            {

                return;
            }

            // -----------------------------------------------------
            // Pool 데이터에서 아이템 찾기
            // -----------------------------------------------------

            GachaItem item =
                poolData.Items.Find(
                    i => i.ItemId == itemId
                );

            if (item == null)
            {

                return;
            }

            // -----------------------------------------------------
            // 카드 생성
            // -----------------------------------------------------

            GameObject card =
                Instantiate(
                    itemCardPrefab,
                    inventoryGrid
                );

            if (card == null)
                return;

            card.name =
                $"{item.ItemName}_InventoryCard";


            // -----------------------------------------------------
            // 보유 개수
            // -----------------------------------------------------

            int count =
                inventory.GetItemCount(itemId);

            // -----------------------------------------------------
            // NEW 여부
            // -----------------------------------------------------

            bool isNew =
                inventory.IsNewItem(itemId);

            // -----------------------------------------------------
            // 카드 표시 컴포넌트
            //
            // 현재 프로젝트의 Inventory prefab이
            // GachaResultItemDisplay를 사용하는 구조도
            // 대응할 수 있도록 먼저 찾고,
            // 없으면 에러 출력
            // -----------------------------------------------------

            GachaResultItemDisplay display =
                card.GetComponent<GachaResultItemDisplay>();

            if (display != null)
            {
                display.SetItemInfo(
                    item,
                    isNew
                );

                display.SetCount(
                    count
                );
            }

            // -----------------------------------------------------
            // NEW 버튼
            // -----------------------------------------------------

            SetupNewBadge(
                card,
                item,
                inventory,
                isNew
            );
        }

        // =========================================================
        // NEW 배지 설정
        // =========================================================

        private void SetupNewBadge(
            GameObject card,
            GachaItem item,
            GachaInventory inventory,
            bool isNew)
        {
            if (!isNew)
                return;

            Transform newBadgeTransform =
                card.transform.Find("NewBadge");

            if (newBadgeTransform == null)
                return;

            newBadgeTransform.gameObject
                .SetActive(true);

            Button newBadgeButton =
                newBadgeTransform.GetComponent<Button>();

            if (newBadgeButton == null)
                return;

            // 중복 등록 방지
            newBadgeButton.onClick.RemoveAllListeners();

            newBadgeButton.onClick.AddListener(
                () =>
                {
                    inventory.MarkAsViewed(
                        item.ItemId
                    );

                    newBadgeTransform.gameObject
                        .SetActive(false);

                }
            );
        }

        // =========================================================
        // 특정 그룹의 아이템 종류 개수
        // =========================================================

        public int GetItemCountInGroup(
            GachaGroup group)
        {
            GachaInventory inventory =
                GachaInventory.Instance;

            if (inventory == null)
                return 0;

            if (GachaManager.Instance == null)
                return 0;

            GachaPoolData poolData =
                GachaManager.Instance.GetPoolData(
                    group
                );

            if (poolData == null)
                return 0;

            return inventory
                .GetItemsInGroup(
                    group,
                    poolData
                )
                .Count;
        }

        // =========================================================
        // 외부에서 현재 탭 갱신
        // =========================================================

        public void Refresh()
        {
            RefreshInventoryDisplay();
        }
        private int GetRarityOrder(GachaRarity rarity)
        {
            switch (rarity)
            {
                case GachaRarity.Common:
                    return 0;

                case GachaRarity.Rare:
                    return 1;

                case GachaRarity.Unique:
                    return 2;

                case GachaRarity.Epic:
                    return 3;

                default:
                    return 99;
            }
        }


        public void PreviousPage()
        {
            if (currentPage <= 0)
                return;

            currentPage--;

            RefreshInventoryDisplay();
        }

        public void NextPage()
        {
            if (currentPage >= totalPages - 1)
                return;

            currentPage++;

            RefreshInventoryDisplay();
        }

        private void UpdatePageUI()
        {
            if (pageText != null)
                pageText.text = $"{currentPage + 1} / {totalPages}";

            if (prevPageButton != null)
                prevPageButton.interactable = currentPage > 0;

            if (nextPageButton != null)
                nextPageButton.interactable = currentPage < totalPages - 1;
        }
    }

}