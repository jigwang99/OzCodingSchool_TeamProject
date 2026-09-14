using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 도감에서 아이템 하나를 표시하는 카드
    /// </summary>
    public class RistItemCard : MonoBehaviour
    {
        [Header("Item Information")]
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text rarityText;
        [SerializeField] private TMP_Text gradeText;

        [Header("Prefab Check")]
        [SerializeField] private TMP_Text prefabStatusText;

        [Header("Optional")]
        [SerializeField] public Image itemIcon;
        [SerializeField] private TMP_Text descriptionText;

        /// <summary>
        /// GachaItem 정보를 카드에 적용
        /// </summary>
        public void Setup(GachaItem item)
        {
            if (item == null)
                return;

            // 이름
            if (itemNameText != null)
            {
                itemNameText.text =
                    item.GetDisplayName();
            }

            // 레어리티
            if (rarityText != null)
            {
                rarityText.text =
                    item.Rarity.ToString();
            }

            // 등급
            if (gradeText != null)
            {
                gradeText.text =
                    $"Grade {item.Grade}";
            }


            // 설명
            if (descriptionText != null)
            {
                descriptionText.text =
                    "설명이 등록되지 않았습니다.";
            }

            // 현재 GachaItem에는 Icon 데이터가 없으므로
            // 아이콘은 나중에 연결
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
            }
        }
    }
}


 