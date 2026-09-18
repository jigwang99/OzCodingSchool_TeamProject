using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// GachaInventory의 변경 내용을
    /// PlayerData의 저장 데이터로 동기화한다.
    /// </summary>
    public class GachaInventorySaveAdapter : MonoBehaviour
    {
        private GachaInventory gachaInventory;
        private bool isSubscribed;

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            // 씬 재입장 시에도 현재 살아 있는 싱글톤을 사용
            Subscribe();

            if (gachaInventory != null)
            {
                gachaInventory.LoadFromPlayerData();
          
            }
        }

        private void Subscribe()
        {
            GachaInventory currentInventory = GachaInventory.Instance;

            if (currentInventory == null)
            {
                return;
            }

            // 이미 같은 인벤토리를 구독하고 있다면 중복 구독 방지
            if (isSubscribed && gachaInventory == currentInventory)
            {
                return;
            }

            // 이전 인벤토리를 구독하고 있었다면 해제
            Unsubscribe();

            gachaInventory = currentInventory;
            gachaInventory.OnInventoryChanged += SyncInventoryToPlayerData;
            isSubscribed = true;


        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (isSubscribed && gachaInventory != null)
            {
                gachaInventory.OnInventoryChanged -= SyncInventoryToPlayerData;
            }

            isSubscribed = false;
            gachaInventory = null;
        }

        private void SyncInventoryToPlayerData()
        {

            if (gachaInventory == null)
            {
            
                return;
            }

            if (GameManager.instance == null)
            {
          
                return;
            }

            PlayerData playerData = GameManager.instance.PlayerData;

            if (playerData == null)
            {
                return;
            }

            if (playerData.gachaInventory == null)
            {
                playerData.gachaInventory =
                    new GachaInventoryData();
            }

            if (playerData.gachaInventory.items == null)
            {
                playerData.gachaInventory.items =
                    new System.Collections.Generic.List<GachaOwnedItemData>();
            }

            playerData.gachaInventory.items.Clear();

            foreach (var pair in gachaInventory.GetAllItems())
            {
                playerData.gachaInventory.items.Add(
                    new GachaOwnedItemData
                    {
                        itemId = pair.Key,
                        count = pair.Value,
                        isNew = gachaInventory.IsNewItem(pair.Key)
                    }
                );
            }

       
        }
    }
}