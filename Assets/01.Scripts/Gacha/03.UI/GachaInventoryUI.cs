using PixelRestaurant.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelRestaurant.Gacha
{
    public class GachaInventoryUI : MonoBehaviour
    {
        [Header("인벤토리 팝업")]
        [SerializeField] private GameObject inventoryPopup;

        [Header("가챠 UI")]
        [SerializeField] private GachaUIController gachaUI;

        [Header("인벤토리 탭")]
        [SerializeField] private Button weaponTab;
        [SerializeField] private Button furnitureTab;
        [SerializeField] private Button recipeTab;

        [Header("전체 합치기")]
        [SerializeField] private Button mergeAllButton;
        [SerializeField] private PlayerWeaponCatalog weaponCatalog;
        [SerializeField] private TextMeshProUGUI mergeResultText;

        [Header("인벤토리 카드")]
        [SerializeField] private Transform inventoryGrid;
        [SerializeField] private GachaInventoryObjectPool inventoryCardPool;

        [Header("페이지")]
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private TextMeshProUGUI pageText;
        [SerializeField] private int itemsPerPage = 20;

        private GachaGroup _currentTab = GachaGroup.Weapon;
        private bool _merging;
        private int currentPage;
        private int totalPages = 1;

        private void Start()
        {
            RegisterTabEvents();

            if (mergeAllButton != null)
                mergeAllButton.onClick.AddListener(MergeAllWeapons);

            RefreshInventoryDisplay();
        }

        private void OnDestroy()
        {
            if (mergeAllButton != null)
                mergeAllButton.onClick.RemoveListener(MergeAllWeapons);

            if (weaponTab != null)
                weaponTab.onClick.RemoveAllListeners();

            if (furnitureTab != null)
                furnitureTab.onClick.RemoveAllListeners();

            if (recipeTab != null)
                recipeTab.onClick.RemoveAllListeners();
        }

        private void RegisterTabEvents()
        {
            if (weaponTab != null)
                weaponTab.onClick.AddListener(SelectWeaponTab);

            if (furnitureTab != null)
                furnitureTab.onClick.AddListener(SelectFurnitureTab);

            if (recipeTab != null)
                recipeTab.onClick.AddListener(SelectRecipeTab);
        }

        public void MergeAllWeapons()
        {
            if (_merging ||
                GachaInventory.Instance == null ||
                GachaManager.Instance == null)
            {
                return;
            }

            GachaPoolData pool =
                GachaManager.Instance.GetPoolData(GachaGroup.Weapon);

            if (pool == null || weaponCatalog == null)
            {
                if (mergeResultText != null)
                    mergeResultText.text = "무기 풀/카탈로그 연결을 확인하세요.";

                return;
            }

            _merging = true;

            try
            {
                int merged =
                    GachaInventory.Instance.TryMergeAllWeapons(
                        pool,
                        weaponCatalog
                    );

                if (mergeResultText != null)
                {
                    mergeResultText.text =
                        merged > 0
                            ? $"무기 {merged}회 합치기 완료"
                            : "합칠 수 있는 무기가 없습니다.";
                }
            }
            finally
            {
                _merging = false;
                RefreshInventoryDisplay();
            }
        }

        private void UpdateMergeAllButton()
        {
            if (mergeAllButton == null)
                return;

            bool weaponTabSelected =
                _currentTab == GachaGroup.Weapon;

            mergeAllButton.gameObject.SetActive(weaponTabSelected);

            if (!weaponTabSelected)
                return;

            GachaPoolData pool =
                GachaManager.Instance != null
                    ? GachaManager.Instance.GetPoolData(GachaGroup.Weapon)
                    : null;

            bool hasMergeMaterials = false;

            if (pool != null &&
                pool.Items != null &&
                GachaInventory.Instance != null)
            {
                foreach (GachaItem item in pool.Items)
                {
                    if (item == null ||
                        item.Group != GachaGroup.Weapon)
                    {
                        continue;
                    }

                    if (GachaInventory.Instance.GetItemCount(item.ItemId) <
                        GachaWeaponMerge.MinimumOwnedCount)
                    {
                        continue;
                    }

                    hasMergeMaterials = true;
                    break;
                }
            }

            mergeAllButton.interactable =
                !_merging &&
                weaponCatalog != null &&
                hasMergeMaterials;
        }

        public void OpenInventory()
        {
            if (inventoryPopup == null)
                return;

            inventoryPopup.SetActive(true);
            RefreshInventoryDisplay();
        }

        public void CloseInventory()
        {
            if (inventoryPopup == null)
                return;

            inventoryPopup.SetActive(false);
        }

        public bool IsInventoryOpen()
        {
            return inventoryPopup != null &&
                   inventoryPopup.activeSelf;
        }

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

        public void RefreshInventoryDisplay()
        {
            GachaInventory inventory =
                GachaInventory.Instance;

            if (inventory == null ||
                GachaManager.Instance == null)
            {
                return;
            }

            GachaPoolData poolData =
                GachaManager.Instance.GetPoolData(_currentTab);

            if (poolData == null ||
                poolData.Items == null)
            {
                return;
            }

            ClearInventoryCards();

            List<string> itemIds =
                inventory.GetItemsInGroup(
                    _currentTab,
                    poolData
                );

            itemIds.Sort((idA, idB) =>
            {
                GachaItem itemA =
                    poolData.Items.Find(
                        item => item != null &&
                                item.ItemId == idA
                    );

                GachaItem itemB =
                    poolData.Items.Find(
                        item => item != null &&
                                item.ItemId == idB
                    );

                if (itemA == null && itemB == null)
                    return 0;

                if (itemA == null)
                    return 1;

                if (itemB == null)
                    return -1;

                int rarityCompare =
                    GetRarityOrder(itemA.Rarity)
                        .CompareTo(GetRarityOrder(itemB.Rarity));

                if (rarityCompare != 0)
                    return rarityCompare;

                int gradeCompare =
                    itemA.Grade.CompareTo(itemB.Grade);

                if (gradeCompare != 0)
                    return gradeCompare;

                return string.Compare(
                    itemA.ItemName,
                    itemB.ItemName,
                    System.StringComparison.Ordinal
                );
            });

            int pageSize = Mathf.Max(1, itemsPerPage);

            totalPages =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        itemIds.Count / (float)pageSize
                    )
                );

            currentPage =
                Mathf.Clamp(
                    currentPage,
                    0,
                    totalPages - 1
                );

            int startIndex =
                currentPage * pageSize;

            int endIndex =
                Mathf.Min(
                    startIndex + pageSize,
                    itemIds.Count
                );

            for (int i = startIndex; i < endIndex; i++)
            {
                CreateItemCard(
                    itemIds[i],
                    inventory,
                    poolData
                );
            }

            UpdatePageUI();
            UpdateMergeAllButton();
        }

        private void ClearInventoryCards()
        {
            if (inventoryGrid == null ||
                inventoryCardPool == null)
            {
                return;
            }

            inventoryCardPool.ReturnAll(inventoryGrid);
        }

        private void CreateItemCard(
            string itemId,
            GachaInventory inventory,
            GachaPoolData poolData)
        {
            if (inventoryCardPool == null ||
                inventoryGrid == null ||
                inventory == null ||
                poolData == null ||
                poolData.Items == null)
            {
                return;
            }

            GachaItem item =
                poolData.Items.Find(
                    data => data != null &&
                            data.ItemId == itemId
                );

            if (item == null)
                return;

            GameObject card =
                inventoryCardPool.GetObject(inventoryGrid);

            if (card == null)
                return;

            card.name =
                $"{item.ItemName}_InventoryCard";

            GachaResultItemDisplay display =
                card.GetComponent<GachaResultItemDisplay>();

            if (display == null)
            {
                inventoryCardPool.ReturnObject(card);
                return;
            }

            int count =
                inventory.GetItemCount(itemId);

            display.SetItemInfo(item, false);
            display.SetCount(count);
        }

        public int GetItemCountInGroup(GachaGroup group)
        {
            GachaInventory inventory =
                GachaInventory.Instance;

            if (inventory == null ||
                GachaManager.Instance == null)
            {
                return 0;
            }

            GachaPoolData poolData =
                GachaManager.Instance.GetPoolData(group);

            if (poolData == null)
                return 0;

            return inventory
                .GetItemsInGroup(
                    group,
                    poolData
                )
                .Count;
        }

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
                pageText.text =
                    $"{currentPage + 1} / {totalPages}";

            if (prevPageButton != null)
                prevPageButton.interactable =
                    currentPage > 0;

            if (nextPageButton != null)
                nextPageButton.interactable =
                    currentPage < totalPages - 1;
        }
    }
}
