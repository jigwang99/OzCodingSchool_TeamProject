using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelRestaurant.Gacha
{
    public class InfoPopupHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("INFO Popup")]
        [SerializeField] private GameObject infoPopup;

        [Header("INFO Popup이 켜질 때 끌 이미지")]
        [SerializeField] private GameObject imageToHide;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (infoPopup != null)
                infoPopup.SetActive(true);

            if (imageToHide != null)
                imageToHide.SetActive(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (infoPopup != null)
                infoPopup.SetActive(false);

            if (imageToHide != null)
                imageToHide.SetActive(true);
        }
    }
}