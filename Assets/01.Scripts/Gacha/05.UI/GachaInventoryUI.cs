using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 인벤토리 팝업 화면 관리
    /// 그룹별 아이템 조회, NEW 배지 표시, 개수 표시
    /// </summary>
    public class GachaInventoryUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject inventoryPopup;

        // ========== 탭 ==========
        [SerializeField]
        private Button weaponTab;

        [SerializeField]
        private Button furnitureTab;

        [SerializeField]
        private Button recipeTab;

        private GachaGroup _currentTab = GachaGroup.Weapon;

        // ========== 그리드 ==========
        [SerializeField]
        private Transform inventoryGrid;

        [SerializeField]
        private GameObject itemCardPrefab;

        private void Start()
        {
            RegisterTabEvents();
            Debug.Log("[인벤토리 UI] 초기화 완료");
        }

        /// <summary>
        /// 탭 이벤트 등록
        /// </summary>
        private void RegisterTabEvents()
        {
            if (weaponTab != null)
                weaponTab.onClick.AddListener(() => SelectTab(GachaGroup.Weapon));

            if (furnitureTab != null)
                furnitureTab.onClick.AddListener(() => SelectTab(GachaGroup.Furniture));

            if (recipeTab != null)
                recipeTab.onClick.AddListener(() => SelectTab(GachaGroup.Recipe));
        }

        /// <summary>
        /// 인벤토리 팝업 열기
        /// </summary>
        public void OpenInventory()
        {
            if (inventoryPopup != null)
            {
                inventoryPopup.SetActive(true);
                RefreshInventoryDisplay();
                Debug.Log("[인벤토리 UI] 팝업 열림");
            }
        }

        /// <summary>
        /// 인벤토리 팝업 닫기
        /// </summary>
        public void CloseInventory()
        {
            if (inventoryPopup != null)
            {
                inventoryPopup.SetActive(false);
                Debug.Log("[인벤토리 UI] 팝업 닫힘");
            }
        }

        /// <summary>
        /// 탭 선택
        /// </summary>
        /// <param name="group">선택한 그룹</param>
        private void SelectTab(GachaGroup group)
        {
            _currentTab = group;
            RefreshInventoryDisplay();
            Debug.Log($"[인벤토리 UI] 탭 선택: {group}");
        }

        /// <summary>
        /// 인벤토리 화면 갱신
        /// </summary>
        private void RefreshInventoryDisplay()
        {
            GachaInventory inventory = GachaInventory.Instance;
            if (inventory == null)
            {
                Debug.LogError("[인벤토리 UI] GachaInventory를 찾을 수 없습니다.");
                return;
            }

            GachaPoolData poolData = GachaManager.Instance.GetPoolData(_currentTab);
            if (poolData == null)
            {
                Debug.LogError($"[인벤토리 UI] {_currentTab} 풀을 찾을 수 없습니다.");
                return;
            }

            // 기존 카드 제거
            if (inventoryGrid != null)
            {
                foreach (Transform child in inventoryGrid)
                {
                    Destroy(child.gameObject);
                }
            }

            // 현재 탭의 아이템 표시
            var itemIds = inventory.GetItemsInGroup(_currentTab, poolData);

            if (itemIds.Count == 0)
            {
                Debug.Log($"[인벤토리 UI] {_currentTab} 아이템이 없습니다.");
                return;
            }

            foreach (var itemId in itemIds)
            {
                CreateItemCard(itemId, inventory, poolData);
            }

            Debug.Log($"[인벤토리 UI] {itemIds.Count}개 아이템 표시");
        }

        /// <summary>
        /// 아이템 카드 생성
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="inventory">인벤토리</param>
        /// <param name="poolData">풀 데이터</param>
        private void CreateItemCard(string itemId, GachaInventory inventory, GachaPoolData poolData)
        {
            if (itemCardPrefab == null || inventoryGrid == null)
            {
                Debug.LogError("[인벤토리 UI] itemCardPrefab 또는 inventoryGrid이 할당되지 않았습니다.");
                return;
            }

            // 카드 생성
            GameObject card = Instantiate(itemCardPrefab, inventoryGrid);

            // 아이템 정보 조회
            var item = poolData.Items.Find(i => i.ItemId == itemId);
            if (item == null)
            {
                Debug.LogError($"[인벤토리 UI] {itemId}를 찾을 수 없습니다.");
                Destroy(card);
                return;
            }

            int count = inventory.GetItemCount(itemId);
            bool isNew = inventory.IsNewItem(itemId);

            // 카드에 정보 전달
            var display = card.GetComponent<GachaResultItemDisplay>();
            if (display != null)
            {
                display.SetItemInfo(item, isNew);
                display.SetCount(count);
            }
            else
            {
                Debug.LogWarning($"[인벤토리 UI] {card.name}에 GachaResultItemDisplay가 없습니다.");
            }

            // NEW 배지 클릭 시 제거
            if (isNew)
            {
                Transform newBadgeTransform = card.transform.Find("NewBadge");
                if (newBadgeTransform != null)
                {
                    Button newBadgeButton = newBadgeTransform.GetComponent<Button>();
                    if (newBadgeButton != null)
                    {
                        newBadgeButton.onClick.AddListener(() =>
                        {
                            inventory.MarkAsViewed(itemId);
                            newBadgeTransform.gameObject.SetActive(false);
                            Debug.Log($"[인벤토리 UI] {item.ItemName} NEW 표시 제거");
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 테스트용: 특정 탭에 아이템이 있는지 확인
        /// </summary>
        /// <param name="group">확인할 그룹</param>
        /// <returns>아이템 개수</returns>
        public int GetItemCountInGroup(GachaGroup group)
        {
            GachaInventory inventory = GachaInventory.Instance;
            GachaPoolData poolData = GachaManager.Instance.GetPoolData(group);

            if (inventory == null || poolData == null)
                return 0;

            return inventory.GetItemsInGroup(group, poolData).Count;
        }
    }
}
