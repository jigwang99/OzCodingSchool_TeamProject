using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 도감 팝업 전체 관리
    /// 현재 GachaManager가 사용하는 가챠 풀을 가져와 도감에 표시한다.
    /// </summary>
    public class RistPopupController : MonoBehaviour
    {
        [Header("Popup")]
        [SerializeField] private GameObject ristPopup;

        [Header("List Content")]
        [SerializeField] private Transform content;

        [Header("Rist Item Card")]
        [SerializeField] private RistItemCard itemCardPrefab;

        private GachaGroup _currentGroup = GachaGroup.Weapon;

        private void Awake()
        {
            if (ristPopup != null)
            {
                ristPopup.SetActive(false);
            }
        }

        private void Update()
        {
            // J 키로 도감 열기 / 닫기
            if (Input.GetKeyDown(KeyCode.J))
            {
                ToggleRist();
            }
        }

        /// <summary>
        /// 도감 열기 / 닫기
        /// </summary>
        public void ToggleRist()
        {
            if (ristPopup == null)
                return;

            if (ristPopup.activeSelf)
            {
                CloseRist();
            }
            else
            {
                OpenRist();
            }
        }

        /// <summary>
        /// 도감 열기
        /// </summary>
        public void OpenRist()
        {
            if (ristPopup == null)
                return;

            ristPopup.SetActive(true);

            // 처음 열었을 때 Weapon 표시
            ShowWeapons();
        }

        /// <summary>
        /// 도감 닫기
        /// </summary>
        public void CloseRist()
        {
            if (ristPopup == null)
                return;

            ristPopup.SetActive(false);
        }

        /// <summary>
        /// Weapon 탭
        /// </summary>
        public void ShowWeapons()
        {
            ShowGroup(GachaGroup.Weapon);
        }

        /// <summary>
        /// Furniture 탭
        /// </summary>
        public void ShowFurniture()
        {
            ShowGroup(GachaGroup.Furniture);
        }

        /// <summary>
        /// Recipe 탭
        /// </summary>
        public void ShowRecipes()
        {
            ShowGroup(GachaGroup.Recipe);
        }

        /// <summary>
        /// 선택한 가챠 그룹의 아이템을 도감에 표시
        /// </summary>
        private void ShowGroup(GachaGroup group)
        {
            _currentGroup = group;

            ClearContent();

            if (GachaManager.Instance == null)
            {

                return;
            }

            GachaPoolData poolData =
                GachaManager.Instance.GetPoolData(group);

            if (poolData == null)
            {

                return;
            }

            // 모든 레어리티의 아이템을 가져온다.
            AddItemsByRarity(poolData, GachaRarity.Common);
            AddItemsByRarity(poolData, GachaRarity.Rare);
            AddItemsByRarity(poolData, GachaRarity.Unique);
            AddItemsByRarity(poolData, GachaRarity.Epic);
        }

        /// <summary>
        /// 특정 레어리티의 아이템을 카드로 생성
        /// </summary>
        private void AddItemsByRarity(
            GachaPoolData poolData,
            GachaRarity rarity)
        {
            List<GachaItem> items =
                poolData.GetItemsByRarity(rarity);

            if (items == null)
                return;

            foreach (GachaItem item in items)
            {
                if (item == null)
                    continue;

                CreateItemCard(item);
            }
        }

        /// <summary>
        /// 도감 아이템 카드 생성
        /// </summary>
        private void CreateItemCard(GachaItem item)
        {
            if (content == null)
            {

                return;
            }

            if (itemCardPrefab == null)
            {
                return;
            }

            RistItemCard card =
                Instantiate(itemCardPrefab, content);

            card.Setup(item);
        }

        /// <summary>
        /// 기존 카드 삭제
        /// </summary>
        private void ClearContent()
        {
            if (content == null)
                return;

            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
    }
}