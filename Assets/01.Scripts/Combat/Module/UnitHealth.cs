using System;
using UnityEngine;

public class UnitHealth : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1f)] private float maxHp = 100f;

    public float MaxHp => maxHp;
    public float CurrentHp { get; private set; }
    public bool IsDead { get; private set; }
    public int LifeVersion { get; private set; }

    public event Action<DamageInfo> OnDamaged;
    public event Action OnDied;
    public event Action<float, float> OnHealthChanged; // (current, max)
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
        // 콜백에서 추가 피해/리타겟이 발생해도 사망 판정을 중복 실행하지 않는다.
        IsDead = CurrentHp <= 0f;
        OnDamaged?.Invoke(damageInfo);
        OnHealthChanged?.Invoke(CurrentHp, maxHp);

        if (CurrentHp <= 0f)
        {
            OnDied?.Invoke();
        }
    }

    public void ResetHealth()
    {
        LifeVersion++;
        CurrentHp = maxHp;
        IsDead = false;
        OnHealthChanged?.Invoke(CurrentHp, maxHp);
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
            OnHealthChanged?.Invoke(CurrentHp, maxHp);
        }
    }
}
