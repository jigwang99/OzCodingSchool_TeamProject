using PixelRestaurant.Data;
using System.Collections.Generic;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠에서 무기를 뽑았을 때
    /// 기존 PlayerWeaponEquipment의 획득 시스템으로 전달한다.
    /// </summary>
    public class GachaWeaponAcquisitionAdapter : MonoBehaviour
    {
        [SerializeField]
        private GachaManager gachaManager;

        [SerializeField]
        private PlayerWeaponEquipment weaponEquipment;

        private void Awake()
        {
            if (gachaManager == null)
                gachaManager = GachaManager.Instance;

            if (weaponEquipment == null)
                weaponEquipment =
                    FindObjectOfType<PlayerWeaponEquipment>();
        }

        private void OnEnable()
        {
            if (gachaManager != null)
            {
                gachaManager.OnGachaItemsDrawn
                    += HandleGachaItemsDrawn;
            }
        }

        private void OnDisable()
        {
            if (gachaManager != null)
            {
                gachaManager.OnGachaItemsDrawn
                    -= HandleGachaItemsDrawn;
            }
        }

        private void HandleGachaItemsDrawn(
            List<GachaItem> results)
        {
            if (weaponEquipment == null)
            {
              

                return;
            }

            if (results == null)
                return;

            foreach (GachaItem item in results)
            {
                if (item == null)
                    continue;

                // 무기만 기존 전투 인벤토리에 추가
                if (item.Group != GachaGroup.Weapon)
                    continue;

                bool success =
                    weaponEquipment.AcquireWeapon(
                        item.ItemId
                    );

                if (success)
                {
                   
                }
              
            }
        }
    }
}