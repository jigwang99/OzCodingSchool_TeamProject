using System;
using UnityEngine;

// 애셋 UnitController.Start의 무작위 검 설정이 끝난 뒤 저장된 무기를 적용한다.
[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(UnitAttack))]
public class PlayerWeaponEquipment : MonoBehaviour
{
    [SerializeField] private PlayerWeaponCatalog catalog;
    [SerializeField] private SpriteRenderer swordRenderer;
    [SerializeField] private string defaultWeaponId = "swords_0";

    private UnitAttack unitAttack;
    private UpgradeManager subscribedUpgradeManager;
    private bool started;

    public PlayerWeaponCatalog Catalog => catalog;
    public PlayerWeaponCatalog.Weapon CurrentWeapon { get; private set; }
    public event Action<PlayerWeaponCatalog.Weapon> OnWeaponChanged;

    // 가차 결과 지급/인스펙터 테스트의 공통 진입점. 획득과 장착은 별도 동작이다.
    public bool AcquireWeapon(string weaponId, int count = 1)
    {
        PlayerWeaponCatalog.Weapon weapon = catalog != null ? catalog.Find(weaponId) : null;
        if (weapon == null || weapon.GetSprite() == null)
            return false;

        PlayerData data = GameManager.instance.PlayerData;
        return data != null && data.AddWeapon(weaponId, count);
    }

    private void OnEnable()
    {
        subscribedUpgradeManager = UpgradeManager.instance;
        if (subscribedUpgradeManager != null)
            subscribedUpgradeManager.OnUpgradePurchased += HandleUpgradePurchased;

        if (started)
            RestoreEquipment();
    }

    private void Start()
    {
        started = true;
        RestoreEquipment();
    }

    private void OnDisable()
    {
        if (subscribedUpgradeManager != null)
            subscribedUpgradeManager.OnUpgradePurchased -= HandleUpgradePurchased;

        subscribedUpgradeManager = null;
    }

    // UI/인벤토리에서 안정적인 ID로 호출한다. 잘못된 ID/프리팹이면 현재 장비를 유지한다.
    public bool EquipWeapon(string weaponId)
    {
        if (catalog == null)
        {
            Debug.LogWarning("[PlayerWeaponEquipment] 무기 카탈로그를 연결하세요.", this);
            return false;
        }

        PlayerWeaponCatalog.Weapon weapon = catalog.Find(weaponId);
        Sprite sprite = weapon != null ? weapon.GetSprite() : null;
        if (sprite == null || !ResolveReferences())
        {
            Debug.LogWarning($"[PlayerWeaponEquipment] 무기를 장착할 수 없습니다: {weaponId}", this);
            return false;
        }

        PlayerData data = GameManager.instance.PlayerData;
        if (data == null || !data.OwnsWeapon(weaponId))
            return false;

        // sword의 Transform/정렬/머티리얼을 유지해 기존 팔과 검 애니메이션을 그대로 사용한다.
        swordRenderer.sprite = sprite;
        CurrentWeapon = weapon;
        unitAttack.SetAttackDamage(weapon.GetDamage(data.GetWeaponLevel(weaponId)));
        data.TryEquipOwnedWeapon(weapon.id);
        OnWeaponChanged?.Invoke(weapon);
        return true;
    }

    public void RestoreEquipment()
    {
        PlayerData data = GameManager.instance.PlayerData;
        if (data == null)
            return;

        data.InitializeWeapons(defaultWeaponId);
        string savedId = data.equippedWeaponId;
        if (string.IsNullOrEmpty(savedId))
        {
            EquipWeapon(defaultWeaponId);
            return;
        }

        if (!EquipWeapon(savedId) && savedId != defaultWeaponId)
            EquipWeapon(defaultWeaponId);
    }

    public void RefreshAttackDamage()
    {
        if (CurrentWeapon == null || unitAttack == null)
            return;

        PlayerData data = GameManager.instance.PlayerData;
        if (data != null)
            unitAttack.SetAttackDamage(CurrentWeapon.GetDamage(data.GetWeaponLevel(CurrentWeapon.id)));
    }

    private bool ResolveReferences()
    {
        if (unitAttack == null)
            unitAttack = GetComponent<UnitAttack>();

        if (swordRenderer == null)
        {
            Transform sword = transform.Find("cat19/armL/sword");
            if (sword != null)
                swordRenderer = sword.GetComponent<SpriteRenderer>();
        }

        return unitAttack != null && swordRenderer != null;
    }

    private void HandleUpgradePurchased(UpgradeData data, int level)
    {
        if (data != null && data.type == UpgradeType.WeaponPower)
            RefreshAttackDamage();
    }
}
