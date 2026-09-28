using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    public class GachaResultItemDisplay : MonoBehaviour
    {
        [Header("Item Icon")]
        [SerializeField] private Image itemIconImage;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI gradeText;
        [SerializeField] private TextMeshProUGUI countText;

        [Header("Badge")]
        [SerializeField] private Image newBadge;

        private Color _commonColor = Color.white;
        private Color _rareColor = Color.green;
        private Color _uniqueColor = new Color(0.5f, 0f, 1f);
        private Color _epicColor = Color.yellow;

        public void SetItemInfo(GachaItem item, bool isNew)
        {
            if (item == null)
                return;

            if (itemIconImage != null)
            {
                itemIconImage.sprite = item.ItemIcon;

                itemIconImage.gameObject.SetActive(
                    item.ItemIcon != null
                );

                itemIconImage.preserveAspect = true;
            }

            if (itemNameText != null)
                itemNameText.text = item.ItemName;

            if (rarityText != null)
            {
                rarityText.text =
                    GetRarityName(item.Rarity);

                rarityText.color =
                    GetRarityColor(item.Rarity);
            }

            if (gradeText != null)
                gradeText.text = $"Lv.{item.Grade}";

            if (newBadge != null)
                newBadge.gameObject.SetActive(isNew);

            if (!isNew && countText != null)
                countText.gameObject.SetActive(false);
        }

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
                countText.text = "";
                countText.gameObject.SetActive(false);
            }
        }

        private string GetRarityName(GachaRarity rarity)
        {
            return rarity switch
            {
                GachaRarity.Common => "ÀÏ¹Ý",
                GachaRarity.Rare => "Èñ±Í",
                GachaRarity.Unique => "À¯´ÏÅ©",
                GachaRarity.Epic => "¿¡ÇÈ",
                _ => ""
            };
        }

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

        public void ResetCard()
        {
            if (itemIconImage != null)
            {
                itemIconImage.sprite = null;
                itemIconImage.gameObject.SetActive(false);
            }

            if (itemNameText != null)
                itemNameText.text = "";

            if (rarityText != null)
            {
                rarityText.text = "";
                rarityText.color = _commonColor;
            }

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