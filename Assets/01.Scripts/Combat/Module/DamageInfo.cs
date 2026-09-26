public readonly struct DamageInfo
{
    public readonly float Damage;
    public readonly int AttackerId;
    public readonly bool IsCritical;
    public readonly bool IsSkill;

    public DamageInfo(float damage, int attackerId, bool isCritical = false, bool isSkill = false)
    {
        Damage = damage;
        AttackerId = attackerId;
        IsCritical = isCritical;
        IsSkill = isSkill;
    }
}
