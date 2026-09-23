using System;
using UnityEngine;

// 일반 초기화가 끝난 뒤 저장된 무기를 적용한다. 에셋의 무작위 장비 초기화는 CatUnitView에서 막는다.
[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(UnitAttack))]
public class PlayerWeaponEquipment : MonoBehaviour
{
    [SerializeField] private PlayerWeaponCatalog catalog;
    [SerializeField] private SpriteRenderer swordRenderer;
    [SerializeField] private string defaultWeaponId = "swords_0";

    private UnitAttack unitAttack;
    private BaseUnitController unit;
    private float skillEndsAt;
    private float skillReadyAt;
    private bool skillActive;
    private UpgradeManager subscribedUpgradeManager;
    private bool started;

    public PlayerWeaponCatalog Catalog => catalog;
    public PlayerWeaponCatalog.Weapon CurrentWeapon { get; private set; }
    public event Action<PlayerWeaponCatalog.Weapon> OnWeaponChanged;
    public event Action OnSkillChanged;
    private float notifiedSkillRemaining;
    private float notifiedCooldownRemaining;
    private bool notifiedSkillActive;
    public bool HasSkill => CurrentWeapon != null && CurrentWeapon.HasTrait(WeaponTraits.Skill);
    public float SkillRemaining => skillActive ? Mathf.Max(0f, skillEndsAt - Time.time) : 0f;
    public float SkillCooldownRemaining => Mathf.Max(0f, skillReadyAt - Time.time);
    public bool CanUseSkill => isActiveAndEnabled && HasSkill && !skillActive
        && SkillCooldownRemaining <= 0f && unit != null && unit.isActiveAndEnabled && !unit.Health.IsDead
        && Time.timeScale > 0f;

    private void Awake()
    {
        unitAttack = GetComponent<UnitAttack>();
        unit = GetComponent<BaseUnitController>();
    }

    public bool TryUseSkill()
    {
        if (!CanUseSkill || catalog == null) return false;
        PlayerWeaponCatalog.SkillSettings skill = catalog.GetSkillSettings(CurrentWeapon);
        if (skill == null) return false;
        skillActive = true;
        skillEndsAt = Time.time + Mathf.Max(0.1f, skill.duration);
        // 쿨타임은 사용 순간부터 진행되며 무기 교체로 초기화하지 않는다.
        skillReadyAt = Time.time + Mathf.Max(0.1f, skill.cooldown);
        unitAttack.SetSkillBuff(skill.damageBonus, skill.criticalChanceBonus, skill.attackSpeedBonus);
        unit.Move.SetSkillSpeedBonus(skill.moveSpeedBonus);
        NotifySkillChanged();
        return true;
    }

    private void Update()
    {
        if (skillActive && (Time.time >= skillEndsAt || unit == null || !unit.isActiveAndEnabled || unit.Health.IsDead))
            EndSkill();
        if (CanUseSkill && unit.IsTargetInAttackRange)
            TryUseSkill();
        NotifySkillChanged();
    }

    public void ResetSkillCooldown()
    {
        skillReadyAt = 0f;
        NotifySkillChanged();
    }

    // 시간은 기존 스킬 처리에서 흘러가며, 실제 남은 시간/상태가 바뀔 때만 알린다.
    private void NotifySkillChanged()
    {
        float remaining = SkillRemaining;
        float cooldown = SkillCooldownRemaining;
        if (remaining == notifiedSkillRemaining && cooldown == notifiedCooldownRemaining
            && skillActive == notifiedSkillActive) return;
        notifiedSkillRemaining = remaining;
        notifiedCooldownRemaining = cooldown;
        notifiedSkillActive = skillActive;
        OnSkillChanged?.Invoke();
    }

    private void EndSkill()
    {
        skillActive = false;
        skillEndsAt = 0f;
        if (unitAttack != null) unitAttack.SetSkillBuff(0f, 0f, 0f);
        if (unit != null && unit.Move != null) unit.Move.SetSkillSpeedBonus(0f);
        NotifySkillChanged();
    }

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
        if (GetComponent<WeaponSkillUI>() == null) gameObject.AddComponent<WeaponSkillUI>();
    }

    private void OnDisable()
    {
        EndSkill();
        if (subscribedUpgradeManager != null)
            subscribedUpgradeManager.OnUpgradePurchased -= HandleUpgradePurchased;

        subscribedUpgradeManager = null;
    }

    // UI/인벤토리에서 안정적인 ID로 호출한다. 잘못된 ID/프리팹이면 현재 장비를 유지한다.
    public bool EquipWeapon(string weaponId)
    {
        if (catalog == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[PlayerWeaponEquipment] 무기 카탈로그를 연결하세요.", this);
#endif
            return false;
        }

        PlayerWeaponCatalog.Weapon weapon = catalog.Find(weaponId);
        Sprite sprite = weapon != null ? weapon.GetSprite() : null;
        if (sprite == null || !ResolveReferences())
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[PlayerWeaponEquipment] 무기를 장착할 수 없습니다: {weaponId}", this);
#endif
            return false;
        }

        PlayerData data = GameManager.instance.PlayerData;
        if (data == null || !data.OwnsWeapon(weaponId))
            return false;

        // sword의 Transform/정렬/머티리얼을 유지해 기존 팔과 검 애니메이션을 그대로 사용한다.
        EndSkill();
        swordRenderer.sprite = sprite;
        CurrentWeapon = weapon;
        unitAttack.ConfigureWeaponTraits(
            weapon.HasTrait(WeaponTraits.HighCriticalChance) ? weapon.criticalChanceBonus : 0f,
            weapon.HasTrait(WeaponTraits.FastAttack) ? weapon.fastAttackIntervalMultiplier : 1f);
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
