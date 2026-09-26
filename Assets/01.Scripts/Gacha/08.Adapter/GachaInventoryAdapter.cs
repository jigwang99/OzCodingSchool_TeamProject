using UnityEngine;

namespace PixelRestaurant.Gacha
{
    // Kept so existing scenes and prefabs retain their serialized component.
    // GachaManager now owns awarding results and saving once per draw.
    // Do not subscribe to OnGachaItemsDrawn here: it would award items twice.
    public class GachaInventoryAdapter : MonoBehaviour
    {
        [SerializeField] private GachaManager gachaManager;
        [SerializeField] private GachaInventory gachaInventory;
    }
}