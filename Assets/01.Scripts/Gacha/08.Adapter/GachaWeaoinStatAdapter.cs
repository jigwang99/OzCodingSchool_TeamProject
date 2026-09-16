using PixelRestaurant.Data;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠 무기의 AttackPower를
    /// 기존 전투 무기 공격력에 추가한다.
    ///
    /// 기존 PlayerWeaponEquipment / UpgradeManager는 수정하지 않고
    /// 가챠 무기 스탯만 어댑터를 통해 연결한다.
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
                weaponEquipment.OnWeaponChanged
                    += HandleWeaponChanged;
            }

            subscribedUpgradeManager =
                UpgradeManager.instance;

            if (subscribedUpgradeManager != null)
            {
                subscribedUpgradeManager.OnUpgradePurchased
                    += HandleUpgradePurchased;
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
                weaponEquipment.OnWeaponChanged
                    -= HandleWeaponChanged;
            }

            if (subscribedUpgradeManager != null)
            {
                subscribedUpgradeManager.OnUpgradePurchased
                    -= HandleUpgradePurchased;
            }

            subscribedUpgradeManager = null;
        }

        private void HandleWeaponChanged(
            PlayerWeaponCatalog.Weapon weapon)
        {
            RefreshAttackDamage();
        }

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
                return;

            PlayerData playerData =
                GameManager.instance != null
                    ? GameManager.instance.PlayerData
                    : null;

            if (playerData == null)
                return;

            string weaponId =
                currentWeapon.id;

            int weaponLevel =
                playerData.GetWeaponLevel(weaponId);

            // 기존 전투 시스템 공격력
            float baseDamage =
                currentWeapon.GetDamage(weaponLevel);

            // 가챠 무기 보너스
            int gachaAttackPower =
                GetGachaAttackPower(weaponId);

            float finalDamage =
                baseDamage + gachaAttackPower;

            unitAttack.SetAttackDamage(finalDamage);

            Debug.Log(
                $"[가챠 무기 스탯] " +
                $"{weaponId} 공격력 적용: " +
                $"기존 {baseDamage} + " +
                $"가챠 +{gachaAttackPower} = " +
                $"{finalDamage}"
            );
        }

        private int GetGachaAttackPower(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
                return 0;

            if (GachaManager.Instance == null)
                return 0;

            var pools =
                GachaManager.Instance.GetAllPools();

            foreach (var pair in pools)
            {
                GachaPoolData pool =
                    pair.Value;

                if (pool == null ||
                    pair.Key != GachaGroup.Weapon)
                {
                    continue;
                }

                GachaItem item =
                    pool.Items.Find(
                        x => x != null &&
                             x.ItemId == weaponId
                    );

                if (item == null)
                    continue;

                if (item.Stats == null)
                    return 0;

                return Mathf.Max(
                    0,
                    item.Stats.attackPower
                );
            }

            return 0;
        }
    }
}