using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 실제 장착된 플레이어 무기와
    /// 연결된 가챠 무기의 스탯을 적용한다.
    ///
    /// 예:
    /// 실제 장착 무기 ID = swords_0
    ///
    /// 가챠 아이템:
    /// ItemId = Test1
    /// LinkedWeaponId = swords_0
    /// attackPower = 1
    ///
    /// 인벤토리에 Test1이 있다면
    /// 실제 공격력에 +1 적용.
    /// </summary>
    [RequireComponent(typeof(PlayerWeaponEquipment))]
    [RequireComponent(typeof(UnitAttack))]
    public class GachaWeaponStatAdapter : MonoBehaviour
    {
        private PlayerWeaponEquipment weaponEquipment;
        private UnitAttack unitAttack;

        private UpgradeManager subscribedUpgradeManager;

        private void Awake()
        {
            weaponEquipment =
                GetComponent<PlayerWeaponEquipment>();

            unitAttack =
                GetComponent<UnitAttack>();
        }

        private void OnEnable()
        {
            if (weaponEquipment != null)
            {
                weaponEquipment.OnWeaponChanged +=
                    HandleWeaponChanged;
            }

            subscribedUpgradeManager =
                UpgradeManager.instance;

            if (subscribedUpgradeManager != null)
            {
                subscribedUpgradeManager.OnUpgradePurchased +=
                    HandleUpgradePurchased;
            }

            if (GachaInventory.Instance != null)
            {
                GachaInventory.Instance.OnInventoryChanged +=
                    HandleInventoryChanged;
            }
        }

        private void Start()
        {
            RefreshAttackDamage();
        }

        private void OnDisable()
        {
            if (weaponEquipment != null)
            {
                weaponEquipment.OnWeaponChanged -=
                    HandleWeaponChanged;
            }

            if (subscribedUpgradeManager != null)
            {
                subscribedUpgradeManager.OnUpgradePurchased -=
                    HandleUpgradePurchased;
            }

            if (GachaInventory.Instance != null)
            {
                GachaInventory.Instance.OnInventoryChanged -=
                    HandleInventoryChanged;
            }

            subscribedUpgradeManager = null;
        }

        /// <summary>
        /// 실제 장착 무기가 변경되었을 때
        /// 공격력을 다시 계산한다.
        /// </summary>
        private void HandleWeaponChanged(
            PlayerWeaponCatalog.Weapon weapon)
        {
            RefreshAttackDamage();
        }

        /// <summary>
        /// 무기 강화가 되었을 때
        /// 기본 공격력 + 가챠 공격력을 다시 계산한다.
        /// </summary>
        private void HandleUpgradePurchased(
            UpgradeData data,
            int level)
        {
            if (data != null &&
                data.type == UpgradeType.WeaponPower)
            {
                RefreshAttackDamage();
            }
        }

        /// <summary>
        /// 가챠 아이템을 새로 획득했을 때
        /// 현재 장착 무기와 연결된 가챠 스탯을 다시 적용한다.
        /// </summary>
        private void HandleInventoryChanged()
        {
            RefreshAttackDamage();
        }

        /// <summary>
        /// 기본 무기 공격력에
        /// 현재 장착 무기와 연결된 가챠 공격력을 더한다.
        /// </summary>
        public void RefreshAttackDamage()
        {
            if (weaponEquipment == null ||
                unitAttack == null)
            {
                return;
            }

            PlayerWeaponCatalog.Weapon currentWeapon =
                weaponEquipment.CurrentWeapon;

            if (currentWeapon == null)
            {
                return;
            }

            PlayerData playerData =
                GameManager.instance != null
                    ? GameManager.instance.PlayerData
                    : null;

            if (playerData == null)
            {
                return;
            }

            string currentWeaponId =
                currentWeapon.id;

            int weaponLevel =
                playerData.GetWeaponLevel(currentWeaponId);

            // 기존 무기의 기본 공격력
            float baseDamage =
                currentWeapon.GetDamage(weaponLevel);

            // 현재 장착 무기와 연결된 가챠 보너스
            int gachaAttackPower =
                GetGachaAttackPower(currentWeaponId);

            float finalDamage =
                baseDamage + gachaAttackPower;

            // 실제 전투 공격력에 적용
            unitAttack.SetAttackDamage(finalDamage);

            
        }

        /// <summary>
        /// 현재 장착된 실제 무기 ID와 연결된
        /// 가챠 아이템의 공격력을 가져온다.
        /// </summary>
        private int GetGachaAttackPower(
            string currentWeaponId)
        {
            if (string.IsNullOrEmpty(currentWeaponId))
                return 0;

            if (GachaInventory.Instance == null)
                return 0;

            if (GachaManager.Instance == null)
                return 0;

            Dictionary<string, int> inventory =
                GachaInventory.Instance.GetAllItems();

            Dictionary<GachaGroup, GachaPoolData> pools =
                GachaManager.Instance.GetAllPools();

            if (inventory == null ||
                pools == null)
            {
                return 0;
            }

            if (!pools.TryGetValue(
                    GachaGroup.Weapon,
                    out GachaPoolData weaponPool))
            {
                return 0;
            }

            if (weaponPool == null ||
                weaponPool.Items == null)
            {
                return 0;
            }

            int totalAttackPower = 0;

            foreach (var pair in inventory)
            {
                string gachaItemId =
                    pair.Key;

                int ownedCount =
                    pair.Value;

                if (ownedCount <= 0)
                    continue;

                GachaItem item =
                    weaponPool.Items.Find(
                        x =>
                            x != null &&
                            x.ItemId == gachaItemId
                    );

                if (item == null)
                    continue;

                // 실제 장착 무기와 연결된 가챠 아이템인지 확인
                if (item.LinkedWeaponId != currentWeaponId)
                    continue;

                if (item.Stats == null)
                    continue;

                int attackPower =
                    Mathf.Max(
                        0,
                        item.Stats.attackPower
                    );

                /*
                 * 현재는 같은 가챠 무기를 여러 개
                 * 보유하면 수량만큼 적용.
                 *
                 * 예:
                 * Test1 +1
                 * Test1 x3
                 * → +3
                 */
                int bonus =
                    attackPower * ownedCount;

                totalAttackPower += bonus;

           
            }

            return totalAttackPower;
        }
    }
}