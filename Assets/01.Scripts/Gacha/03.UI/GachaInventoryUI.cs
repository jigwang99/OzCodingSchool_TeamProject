
using System.Collections.Generic;
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

        private GachaGroup _currentTab =
            GachaGroup.Weapon;

        // =========================================================
        // 인벤토리 카드
        // =========================================================

        [Header("인벤토리 카드")]
        [SerializeField] private Transform inventoryGrid;

        [SerializeField] private GameObject itemCardPrefab;

        // =========================================================
        // Unity
        // =========================================================

        private void Start()
        {
            RegisterTabEvents();
            RegisterTestEvents();

            

        }
        private void RegisterTestEvents()
        {
            if (clearGachaInventoryButton == null)
                return;

            clearGachaInventoryButton.onClick.RemoveAllListeners();

            clearGachaInventoryButton.onClick.AddListener(
                ClearGachaInventory
            );
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

        private void SelectTab(
            GachaGroup group)
        {
            _currentTab = group;

            RefreshInventoryDisplay();

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

            if (itemIds.Count == 0)
            {

                return;
            }

            // -----------------------------------------------------
            // 카드 생성
            // -----------------------------------------------------

            foreach (string itemId in itemIds)
            {
                CreateItemCard(
                    itemId,
                    inventory,
                    poolData
                );
            }

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
        // =========================================================
        // 인벤토리 초기화 버튼 이벤트 등록 (테스트용)
        // =========================================================
        private void ClearGachaInventory()
        {
            GachaInventory inventory =
                GachaInventory.Instance;

            if (inventory == null)
            {

                return;
            }

         
            // 현재 화면도 즉시 갱신
            RefreshInventoryDisplay();

            // 테스트용 저장 데이터까지 즉시 반영
            if (SaveManager.instance != null)
            {
                SaveManager.instance.Save();
            }

        }
    }

}