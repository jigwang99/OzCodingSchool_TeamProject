public readonly struct DamageInfo
{
    public readonly float Damage;
    public readonly int AttackerId;
    public readonly bool IsCritical;

    public DamageInfo(float damage, int attackerId, bool isCritical = false)
    {
        Damage = damage;
        AttackerId = attackerId;
        IsCritical = isCritical;
    }
}
