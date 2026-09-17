using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// GachaInventory의 변경 내용을
    /// PlayerData의 저장 데이터로 연결한다.
    /// </summary>
    public class GachaInventorySaveAdapter : MonoBehaviour
    {
        [SerializeField]
        private GachaInventory gachaInventory;

        private void Awake()
        {
            if (gachaInventory == null)
                gachaInventory = GachaInventory.Instance;
        }

        private void Start()
        {
            if (gachaInventory != null)
            {
                gachaInventory.LoadFromPlayerData();
            }
        }

        private void OnEnable()
        {
            if (gachaInventory != null)
            {
                gachaInventory.OnInventoryChanged
                    += SyncInventoryToPlayerData;
            }
        }

        private void OnDisable()
        {
            if (gachaInventory != null)
            {
                gachaInventory.OnInventoryChanged
                    -= SyncInventoryToPlayerData;
            }
        }

        private void SyncInventoryToPlayerData()
        {
            if (GameManager.instance == null)
            {
                Debug.LogError(
                    "[가챠 저장 Adapter] GameManager가 없습니다."
                );
                return;
            }

            PlayerData playerData =
                GameManager.instance.PlayerData;

            if (playerData == null)
            {
                Debug.LogError(
                    "[가챠 저장 Adapter] PlayerData가 없습니다."
                );
                return;
            }

            if (playerData.gachaInventory == null)
            {
                playerData.gachaInventory =
                    new GachaInventoryData();
            }

            playerData.gachaInventory.items.Clear();

            foreach (var pair in
                     gachaInventory.GetAllItems())
            {
                playerData.gachaInventory.items.Add(
                    new GachaOwnedItemData
                    {
                        itemId = pair.Key,
                        count = pair.Value,
                        isNew =
                            gachaInventory.IsNewItem(pair.Key)
                    }
                );
            }

            Debug.Log(
                "[가챠 저장 Adapter] " +
                "PlayerData에 인벤토리 동기화 완료"
            );

            // 테스트 단계에서는 즉시 저장
            //if (SaveManager.Instance != null)
            //{
            //    SaveManager.Instance.Save();
            //}
        }
    }
}