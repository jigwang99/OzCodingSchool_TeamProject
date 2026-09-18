using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// GachaManager의 결과 이벤트를
    /// GachaInventory에 연결하고 전투 무기 데이터 연동 및 저장을 담당하는 Adapter
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

        private void HandleGachaItemsDrawn(List<GachaItem> results)
        {
            if (gachaInventory == null)
            {

                return;
            }

            if (results == null || results.Count == 0)
                return;

            foreach (GachaItem item in results)
            {
                if (item == null)
                    continue;

                // 1. 가챠 인벤토리에 아이템 추가
                gachaInventory.AddItem(item.ItemId);

                // 2. [추가] 뽑은 아이템이 전투 무기라면 PlayerData(ownedWeapons)에도 연동
                SyncWeaponToPlayerData(item);
            }

            // 3. [추가] 가챠 뽑기가 끝난 직후 확실하게 전체 데이터 저장 실행!
            SaveGameData();

        }

        /// <summary>
        /// 가챠 아이템을 전투씬용 무기 데이터로 변환하여 PlayerData에 추가
        /// </summary>
        private void SyncWeaponToPlayerData(GachaItem item)
        {
            if (GameManager.instance == null || GameManager.instance.PlayerData == null)
                return;

            if (item.Group == GachaGroup.Weapon && !string.IsNullOrEmpty(item.LinkedWeaponId))
            {
                // 전투 무기 데이터에 추가 (중복 시 count 증가)
                GameManager.instance.PlayerData.AddWeapon(item.LinkedWeaponId, 1);
            }
        }

        /// <summary>
        /// 뽑은 직후 즉시 저장 매니저 호출
        /// </summary>
        private void SaveGameData()
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.Save();
            }
         
        }
    }
}