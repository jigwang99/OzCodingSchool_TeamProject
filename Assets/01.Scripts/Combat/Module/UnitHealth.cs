using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1f)] private float maxHp = 100f;

    public float MaxHp => maxHp;
    public float CurrentHp { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<DamageInfo> OnDamaged;
    public event Action OnDied;

    private void Awake()
    {
        ResetHealth();
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (IsDead || damageInfo.Damage <= 0f)
        {
            return;
        }

        CurrentHp = Mathf.Max(0f, CurrentHp - damageInfo.Damage);
        OnDamaged?.Invoke(damageInfo);

        if (CurrentHp <= 0f)
        {
            IsDead = true;
            OnDied?.Invoke();
        }
    }

    public void ResetHealth()
    {
        CurrentHp = maxHp;
        IsDead = false;
    }

    public void SetMaxHp(float value, bool resetCurrentHp = true)
    {
        maxHp = Mathf.Max(1f, value);

        if (resetCurrentHp)
        {
            ResetHealth();
        }
        else
        {
            CurrentHp = Mathf.Min(CurrentHp, maxHp);
        }
    }
}
