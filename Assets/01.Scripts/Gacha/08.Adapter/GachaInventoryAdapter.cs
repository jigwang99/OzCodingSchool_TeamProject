using System.Collections.Generic;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// GachaManager의 결과 이벤트를
    /// GachaInventory에 연결하는 Adapter
    /// </summary>
    public class GachaInventoryAdapter : MonoBehaviour
    {
        [SerializeField]
        private GachaManager gachaManager;

        [SerializeField]
        private GachaInventory gachaInventory;

        private void Awake()
        {
            if (gachaManager == null)
                gachaManager = GachaManager.Instance;

            if (gachaInventory == null)
                gachaInventory = GachaInventory.Instance;
        }

        private void OnEnable()
        {
            if (gachaManager != null)
                gachaManager.OnGachaItemsDrawn += HandleGachaItemsDrawn;
        }

        private void OnDisable()
        {
            if (gachaManager != null)
                gachaManager.OnGachaItemsDrawn -= HandleGachaItemsDrawn;
        }

        private void HandleGachaItemsDrawn(
            List<GachaItem> results)
        {
            if (gachaInventory == null)
            {
                Debug.LogError(
                    "[가챠 인벤토리 Adapter] GachaInventory가 없습니다."
                );

                return;
            }

            if (results == null || results.Count == 0)
                return;

            foreach (GachaItem item in results)
            {
                if (item == null)
                    continue;

                gachaInventory.AddItem(item.ItemId);
            }

            Debug.Log(
                $"[가챠 인벤토리 Adapter] " +
                $"{results.Count}개 아이템 인벤토리 반영"
            );
        }
    }
}