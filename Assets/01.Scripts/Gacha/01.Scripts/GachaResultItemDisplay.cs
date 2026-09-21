using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 뽑기 결과 카드에 아이템 정보 표시
    /// NEW 배지, 아이템명, 레어리티 색상, 등급, 개수 표시
    /// </summary>
    public class GachaResultItemDisplay : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI itemNameText;

        [SerializeField]
        private TextMeshProUGUI rarityText;

        [SerializeField]
        private TextMeshProUGUI gradeText;

        [SerializeField]
        private Image newBadge;

        [SerializeField]
        private TextMeshProUGUI countText;

        // ========== 레어리티 색상 ==========
        private Color _commonColor = Color.white;                    // 하양
        private Color _rareColor = Color.green;                      // 초록
        private Color _uniqueColor = Color.yellow;                   // 노랑
        private Color _epicColor = new Color(0.5f, 0, 1);           // 보라

        private void Start()
        {
            // 폰트 크기 설정
            if (itemNameText != null)
                itemNameText.fontSize = 7;

            if (rarityText != null)
                rarityText.fontSize = 7;

            if (gradeText != null)
                gradeText.fontSize = 7;

            if (countText != null)
                countText.fontSize = 6;
        }

        /// <summary>
        /// 아이템 정보를 카드에 표시
        /// </summary>
        /// <param name="item">표시할 아이템</param>
        /// <param name="isNew">NEW 배지 여부</param>
        public void SetItemInfo(GachaItem item, bool isNew)
        {
            if (item == null)
            {
                return;
            }

            // 아이템 이름 표시
            if (itemNameText != null)
                itemNameText.text = item.ItemName;

            // 레어리티 표시 (색상 포함)
            if (rarityText != null)
            {
                rarityText.text = item.Rarity.ToString();
                rarityText.color = GetRarityColor(item.Rarity);
            }

            // 등급 표시
            if (gradeText != null)
                gradeText.text = $"Lv.{item.Grade}";

            // NEW 배지
            if (newBadge != null)
            {
                newBadge.gameObject.SetActive(isNew);
            }

            // 개수는 SetCount()에서 별도 처리
            if (!isNew && countText != null)
            {
                countText.gameObject.SetActive(false);
            }

        }

        /// <summary>
        /// 아이템 개수 표시
        /// </summary>
        /// <param name="count">개수</param>
        public void SetCount(int count)
        {
            if (countText == null)
                return;

            if (count > 1)
            {
                countText.text = $"x{count}";
                countText.gameObject.SetActive(true);
            }
            else
            {
                countText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 레어리티별 색상 반환
        /// </summary>
        /// <param name="rarity">레어리티</param>
        /// <returns>색상</returns>
        private Color GetRarityColor(GachaRarity rarity)
        {
            return rarity switch
            {
                GachaRarity.Common => _commonColor,
                GachaRarity.Rare => _rareColor,
                GachaRarity.Unique => _uniqueColor,
                GachaRarity.Epic => _epicColor,
                _ => _commonColor
            };
        }

        /// <summary>
        /// 카드 리셋 (풀에 반환 전)
        /// </summary>
        public void ResetCard()
        {
            if (itemNameText != null)
                itemNameText.text = "";

            if (rarityText != null)
                rarityText.text = "";

            if (gradeText != null)
                gradeText.text = "";

            if (newBadge != null)
                newBadge.gameObject.SetActive(false);

            if (countText != null)
                countText.gameObject.SetActive(false);
        }
    }
}
