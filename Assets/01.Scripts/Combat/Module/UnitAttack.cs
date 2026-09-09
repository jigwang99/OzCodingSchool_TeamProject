using System;
using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    [SerializeField, Min(0f)] private float attackDamage = 10f;
    [SerializeField, Min(0.01f)] private float attackRange = 2f;
    [SerializeField, Min(0.01f)] private float attackInterval = 1f;
    [Header("타격 시점")]
    [SerializeField] private bool useAnimationEvents;
    [SerializeField, Min(0f)] private float hitDelay = 0.2f;

    public float AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    public float AttackInterval => Mathf.Max(0.01f, attackInterval);
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

    private void Awake() => owner = GetComponent<BaseUnitController>();
    private void OnDisable() => ResetAttack();

    public int BeginAttack(BaseUnitController target, int animationIndex)
    {
        if (CooldownRemaining > 0f || !IsValidTarget(target))
            return 0;
        pendingTarget = target;
        targetLifeVersion = target.Health.LifeVersion;
        pendingAnimationIndex = animationIndex;
        pendingDamage = new DamageInfo(attackDamage, gameObject.GetInstanceID());
        nextAttackTime = Time.time + AttackInterval;
        return window.Begin();
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
            && !target.Health.IsDead && IsInAttackRange(target.transform);
    }

    public event Action<IDamageable, DamageInfo> OnAttackHit;

    public bool IsInAttackRange(Transform target)
    {
        return target != null && Mathf.Abs(transform.position.x - target.position.x) <= attackRange;
    }

    public bool Attack(IDamageable target, bool isCritical = false)
    {
        if (target == null || target.IsDead)
        {
            return false;
        }
        var info = new DamageInfo(attackDamage, gameObject.GetInstanceID(), isCritical);
        target.TakeDamage(info);
        OnAttackHit?.Invoke(target, info);
        Debug.Log($"{this.gameObject.name}이 공격");
        return true;
    }

    public void SetAttackDamage(float value)
    {
        attackDamage = Mathf.Max(0f, value);
    }
}
