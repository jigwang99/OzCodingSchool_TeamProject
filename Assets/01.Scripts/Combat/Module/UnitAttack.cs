using System;
using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    [SerializeField, Min(0f)] private float attackDamage = 10f;
    [SerializeField, Min(0.01f)] private float attackRange = 2f;
    [SerializeField, Min(0.01f)] private float attackInterval = 1f;
    [Header("크리티컬")]
    [SerializeField, Range(0f, 1f)] private float criticalChance = 0.05f;
    [SerializeField, Min(1f)] private float criticalDamageMultiplier = 1.5f;
    [Header("타격 시점")]
    [SerializeField] private bool useAnimationEvents;
    [SerializeField, Min(0f)] private float hitDelay = 0.2f;

    public float AttackDamage => attackDamage * buffDamageMultiplier;
    public float AttackRange => attackRange;
    public float AttackInterval => Mathf.Max(0.01f, attackInterval * weaponIntervalMultiplier / buffAttackSpeedMultiplier);
    public float AttackAnimationSpeed => buffAttackSpeedMultiplier / weaponIntervalMultiplier;
    public float CriticalChance => Mathf.Clamp01(criticalChance + weaponCriticalBonus + buffCriticalBonus);
    public float CriticalDamageMultiplier => Mathf.Max(1f, criticalDamageMultiplier);
    public bool UseAnimationEvents => useAnimationEvents;
    public float HitDelay => Mathf.Clamp(hitDelay, 0f, AttackInterval);
    public float CooldownRemaining => Mathf.Max(0f, nextAttackTime - Time.time);
    public bool HasPendingHit => window.IsPending;
    public bool IsPendingTargetValid => window.IsPending && IsValidTarget(pendingTarget)
        && pendingTarget.Health.LifeVersion == targetLifeVersion;

    private readonly AttackWindow window = new AttackWindow();
    private BaseUnitController owner;
    private BaseUnitController pendingTarget;
    private DamageInfo pendingDamage;
    private int targetLifeVersion;
    private int pendingAnimationIndex;
    private float nextAttackTime;
    private float weaponIntervalMultiplier = 1f;
    private float weaponCriticalBonus;
    private float buffDamageMultiplier = 1f;
    private float buffAttackSpeedMultiplier = 1f;
    private float buffCriticalBonus;
    private bool hasSkillBuff;
    public event Action OnStatsChanged;
    private Vector3 DisplayStats => new Vector3(AttackDamage, CriticalChance, AttackInterval);

    public void ConfigureWeaponTraits(float criticalBonus, float intervalMultiplier)
    {
        Vector3 previous = DisplayStats;
        // 장비 교체로 이전 공격을 새 특성으로 처리하지 않으며, 기존 쿨다운은 유지한다.
        CancelPendingHit();
        weaponCriticalBonus = Mathf.Clamp01(criticalBonus);
        weaponIntervalMultiplier = Mathf.Clamp(intervalMultiplier, 0.1f, 1f);
        if (!previous.Equals(DisplayStats)) OnStatsChanged?.Invoke();
    }

    public void SetSkillBuff(float damageBonus, float criticalBonus, float attackSpeedBonus)
    {
        Vector3 previous = DisplayStats;
        buffDamageMultiplier = 1f + Mathf.Max(0f, damageBonus);
        buffCriticalBonus = Mathf.Clamp01(criticalBonus);
        buffAttackSpeedMultiplier = 1f + Mathf.Max(0f, attackSpeedBonus);
        hasSkillBuff = damageBonus > 0f || criticalBonus > 0f || attackSpeedBonus > 0f;
        if (!previous.Equals(DisplayStats)) OnStatsChanged?.Invoke();
    }

    private void Awake() => owner = GetComponent<BaseUnitController>();
    private void OnDisable() => ResetAttack();

    public int BeginAttack(BaseUnitController target, int animationIndex)
    {
        if (CooldownRemaining > 0f || !IsValidTarget(target))
            return 0;
        pendingTarget = target;
        targetLifeVersion = target.Health.LifeVersion;
        pendingAnimationIndex = animationIndex;
        // 공격 시작 시 한 번만 판정하여 타격 이벤트가 중복되어도 결과를 유지한다.
        bool isCritical = CriticalChance >= 1f || UnityEngine.Random.value < CriticalChance;
        float damage = AttackDamage * (isCritical ? CriticalDamageMultiplier : 1f);
        pendingDamage = new DamageInfo(damage, gameObject.GetInstanceID(), isCritical, hasSkillBuff);
        nextAttackTime = Time.time + AttackInterval;
        int attackId = window.Begin();
        owner.Move.CaptureAttackDirection(target.transform.position.x - transform.position.x);
        OnAttackStarted?.Invoke();
        return attackId;
    }

    public bool ResolveAnimationHit(int animationIndex)
    {
        return useAnimationEvents && animationIndex == pendingAnimationIndex && TryResolveHit(window.Id);
    }

    public bool TryResolveHit(int attackId)
    {
        if (attackId != window.Id || !window.IsPending)
            return false;
        bool valid = IsPendingTargetValid;
        // 피해 이벤트 안에서 사망/리타겟이 발생하기 전에 소비한다.
        window.TryConsume(attackId);
        BaseUnitController target = pendingTarget;
        DamageInfo damage = pendingDamage;
        pendingTarget = null;
        if (!valid || damage.Damage <= 0f)
            return false;
        UnitHealth targetHealth = target.Health;
        targetHealth.TakeDamage(damage);
        OnAttackHit?.Invoke(targetHealth, damage);
        return true;
    }

    public void CancelPendingHit(int attackId)
    {
        if (attackId != window.Id) return;
        window.Cancel(attackId);
        pendingTarget = null;
    }

    public void CancelPendingHit()
    {
        window.Cancel();
        pendingTarget = null;
    }

    public void ResetAttack()
    {
        CancelPendingHit();
        nextAttackTime = 0f;
    }

    private bool IsValidTarget(BaseUnitController target)
    {
        return isActiveAndEnabled && owner != null && owner.isActiveAndEnabled
            && owner.Health != null && !owner.Health.IsDead && owner.Target == target
            && target != null && target.isActiveAndEnabled && target.Health != null
            && !target.Health.IsDead && IsUnitInAttackRange(target);
    }

    public event Action<IDamageable, DamageInfo> OnAttackHit;
    public event Action OnAttackStarted;

    // 이미 알고 있는 컨트롤러를 전달해 반복 GetComponent와 층 판정을 줄인다.
    public bool IsUnitInAttackRange(BaseUnitController target) =>
        target != null && owner != null &&
        Mathf.Abs(transform.position.x - target.transform.position.x) <= attackRange && owner.CanEngage(target);

    public void SetAttackDamage(float value)
    {
        float previous = AttackDamage;
        attackDamage = Mathf.Max(0f, value);
        if (previous != AttackDamage) OnStatsChanged?.Invoke();
    }
}
