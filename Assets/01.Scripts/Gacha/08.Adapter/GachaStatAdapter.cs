using System.Collections.Generic;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// GachaManager의 결과 이벤트를 받아
    /// GachaStatManager에 전달한다.
    /// </summary>
    public class GachaStatAdapter : MonoBehaviour
    {
        [SerializeField]
        private GachaManager gachaManager;

        [SerializeField]
        private GachaStatManager statManager;

        private void Awake()
        {
            if (gachaManager == null)
                gachaManager = GachaManager.Instance;

            if (statManager == null)
                statManager = GachaStatManager.Instance;
        }

        private void Start()
        {

            // GachaManager 확인
            if (gachaManager != null)
            {
            }
           
            // GachaStatManager 확인
            if (statManager != null)
            {
            }
          

            // GachaInventory 확인
            if (GachaInventory.Instance != null)
            {
            }
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {

                UnitAttack unitAttack = GetComponent<UnitAttack>();
                PlayerWeaponEquipment weaponEquipment =
                    GetComponent<PlayerWeaponEquipment>();

                if (unitAttack == null)
                {
                    return;
                }

                if (weaponEquipment == null)
                {
                    return;
                }

                // 현재 실제 공격력
            

                // 현재 장착 무기
                if (weaponEquipment.CurrentWeapon != null)
                {
                    
                }
               

                // 가챠 인벤토리 확인
                if (GachaInventory.Instance == null)
                {
                    return;
                }


            

            }
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
            if (statManager == null)
            {
                
                return;
            }

            statManager.ApplyGachaResults(results);
        }
    }
}
