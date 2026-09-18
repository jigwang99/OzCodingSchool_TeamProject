using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 뽑기 결과 카드에 아이템 정보 표시
    /// 아이콘, NEW 배지, 아이템명, 레어리티, 등급, 개수 표시
    /// </summary>
    public class GachaResultItemDisplay : MonoBehaviour
    {
        [Header("Item Icon")]
        [SerializeField]
        private Image itemIconImage;

        [Header("Texts")]
        [SerializeField]
        private TextMeshProUGUI itemNameText;

        [SerializeField]
        private TextMeshProUGUI rarityText;

        [SerializeField]
        private TextMeshProUGUI gradeText;

        [SerializeField]
        private TextMeshProUGUI countText;

        [Header("Badge")]
        [SerializeField]
        private Image newBadge;

        // ========== 레어리티 색상 ==========
        private Color _commonColor = Color.white;
        private Color _rareColor = Color.green;
        private Color _uniqueColor = Color.yellow;
        private Color _epicColor = new Color(0.5f, 0, 1);

        private void Start()
        {
            // 폰트 크기는 2단계에서 수정 예정
            if (itemNameText != null)
                itemNameText.fontSize = 60;

            if (rarityText != null)
                rarityText.fontSize = 20;

            if (gradeText != null)
                gradeText.fontSize = 20;

            if (countText != null)
                countText.fontSize = 20;
        }

        /// <summary>
        /// 아이템 정보를 카드에 표시
        /// </summary>
        public void SetItemInfo(GachaItem item, bool isNew)
        {
            if (item == null)
                return;

            // =========================================
            // 아이콘
            // =========================================

            if (itemIconImage != null)
            {
                itemIconImage.sprite = item.ItemIcon;

                // Sprite가 있을 때만 표시
                itemIconImage.gameObject.SetActive(
                    item.ItemIcon != null
                );

                // 원본 이미지 비율 유지
                itemIconImage.preserveAspect = true;
            }

            // =========================================
            // 아이템 이름
            // =========================================

            if (itemNameText != null)
                itemNameText.text = item.ItemName;

            // =========================================
            // 레어리티
            // =========================================

            if (rarityText != null)
            {
                rarityText.text = item.Rarity.ToString();
                rarityText.color = GetRarityColor(item.Rarity);
            }

            // =========================================
            // 등급
            // =========================================

            if (gradeText != null)
                gradeText.text = $"Lv.{item.Grade}";

            // =========================================
            // NEW 배지
            // =========================================

            if (newBadge != null)
                newBadge.gameObject.SetActive(isNew);

            // =========================================
            // 개수
            // =========================================

            if (!isNew && countText != null)
                countText.gameObject.SetActive(false);
        }

        /// <summary>
        /// 아이템 개수 표시
        /// </summary>
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
        /// 카드 리셋
        /// </summary>
        public void ResetCard()
        {
            // 아이콘 초기화
            if (itemIconImage != null)
            {
                itemIconImage.sprite = null;
                itemIconImage.gameObject.SetActive(false);
            }

            if (itemNameText != null)
                itemNameText.text = "";

            if (rarityText != null)
                rarityText.text = "";

            if (gradeText != null)
                gradeText.text = "";

            if (newBadge != null)
                newBadge.gameObject.SetActive(false);

            if (countText != null)
            {
                countText.text = "";
                countText.gameObject.SetActive(false);
            }
        }
    }
}