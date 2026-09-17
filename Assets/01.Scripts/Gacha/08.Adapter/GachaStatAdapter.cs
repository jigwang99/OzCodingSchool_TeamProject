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
            Debug.Log("[가챠 테스트] CombatScene 진입 - 가챠 연결 확인");

            // GachaManager 확인
            if (gachaManager != null)
            {
                Debug.Log("[가챠 테스트] GachaManager 연결 성공");
            }
            else
            {
                Debug.LogError("[가챠 테스트] GachaManager 연결 실패!");
            }

            // GachaStatManager 확인
            if (statManager != null)
            {
                Debug.Log("[가챠 테스트] GachaStatManager 연결 성공");
            }
            else
            {
                Debug.LogError("[가챠 테스트] GachaStatManager 연결 실패!");
            }

            // GachaInventory 확인
            if (GachaInventory.Instance != null)
            {
                Debug.Log("[가챠 테스트] GachaInventory 연결 성공");
            }
            else
            {
                Debug.LogError("[가챠 테스트] GachaInventory 연결 실패!");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                Debug.Log("========== [가챠 전투 연결 테스트] ==========");

                UnitAttack unitAttack = GetComponent<UnitAttack>();
                PlayerWeaponEquipment weaponEquipment =
                    GetComponent<PlayerWeaponEquipment>();

                if (unitAttack == null)
                {
                    Debug.LogError("[가챠 테스트] UnitAttack을 찾을 수 없습니다!");
                    return;
                }

                if (weaponEquipment == null)
                {
                    Debug.LogError("[가챠 테스트] PlayerWeaponEquipment를 찾을 수 없습니다!");
                    return;
                }

                // 현재 실제 공격력
                Debug.Log(
                    $"[가챠 테스트] 현재 실제 공격력 : {unitAttack.AttackDamage}"
                );

                // 현재 장착 무기
                if (weaponEquipment.CurrentWeapon != null)
                {
                    Debug.Log(
                        $"[가챠 테스트] 현재 장착 무기 : " +
                        $"{weaponEquipment.CurrentWeapon.id}"
                    );
                }
                else
                {
                    Debug.LogWarning("[가챠 테스트] 현재 장착 무기가 없습니다.");
                }

                // 가챠 인벤토리 확인
                if (GachaInventory.Instance == null)
                {
                    Debug.LogError("[가챠 테스트] GachaInventory가 없습니다!");
                    return;
                }

                Debug.Log("[가챠 테스트] GachaInventory 연결 성공");

                GachaInventory.Instance.DebugPrintAllItems();

                Debug.Log("==============================================");
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
                Debug.LogError(
                    "[가챠 스탯 Adapter] " +
                    "GachaStatManager가 없습니다."
                );

                return;
            }

            statManager.ApplyGachaResults(results);
        }
    }
}
