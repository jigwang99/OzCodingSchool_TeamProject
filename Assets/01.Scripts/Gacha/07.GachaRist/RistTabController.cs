using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// µµ°¨ÀÇ Weapon / Furniture / Recipe ÅÇ °ü¸®
    /// </summary>
    public class RistTabController : MonoBehaviour
    {
        [SerializeField]
        private RistPopupController ristPopupController;

        /// <summary>
        /// Weapon ÅÇ
        /// </summary>
        public void OnWeaponTab()
        {
            if (ristPopupController == null)
                return;

            ristPopupController.ShowWeapons();
        }

        /// <summary>
        /// Furniture ÅÇ
        /// </summary>
        public void OnFurnitureTab()
        {
            if (ristPopupController == null)
                return;

            ristPopupController.ShowFurniture();
        }

        /// <summary>
        /// Recipe ÅÇ
        /// </summary>
        public void OnRecipeTab()
        {
            if (ristPopupController == null)
                return;

            ristPopupController.ShowRecipes();
        }
    }
}