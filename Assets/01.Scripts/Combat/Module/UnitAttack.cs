using System;
using UnityEngine;
using static UnityEngine.AdaptivePerformance.Provider.AdaptivePerformanceSubsystemDescriptor;

public class UnitAttack : MonoBehaviour
{
    [SerializeField, Min(0f)] private float attackDamage = 10f;
    [SerializeField, Min(0.01f)] private float attackRange = 1f;
    [SerializeField, Min(0.01f)] private float attackInterval = 1f;

    public float AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    public float AttackInterval => attackInterval;

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
