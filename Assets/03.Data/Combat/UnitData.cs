using System;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    PlayerCat,
    NormalEnemy,
    BossEnemy
}

[Serializable]
public class UnitStat
{
    [Header("Info")]
    [SerializeField] private UnitType unitType;
    [SerializeField] private string unitId;
    [SerializeField] private string displayName;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Sprite icon;

    [Header("Stats")]
    [SerializeField, Min(1)] private int maxHp;
    [SerializeField, Min(0)] private int attackDamage;
    [SerializeField, Min(0f)] private float moveSpeed;
    [SerializeField, Min(0.01f)] private float attackRange;
    [SerializeField, Min(0.01f)] private float attackCooldown;
    [SerializeField, Min(0f)] private float vision;

    public UnitType UnitType => unitType;
    public string UnitId => unitId;
    public string DisplayName => displayName;
    public GameObject Prefab => prefab;
    public Sprite Icon => icon;
    public int MaxHp => maxHp;
    public int AttackDamage => attackDamage;
    public float MoveSpeed => moveSpeed;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float Vision => vision;

    public UnitStat(UnitType unitType, string unitId, string displayName, GameObject prefab, Sprite icon, int maxHp, int attackDamage, float moveSpeed, float attackRange, float attackCooldown, float vision)
    {
        this.unitType = unitType;
        this.unitId = unitId;
        this.displayName = displayName;
        this.prefab = prefab;
        this.icon = icon;
        this.maxHp = maxHp;
        this.attackDamage = attackDamage;
        this.moveSpeed = moveSpeed;
        this.attackRange = attackRange;
        this.attackCooldown = attackCooldown;
        this.vision = vision;
    }
    public UnitStat Clone()
    {
        return new UnitStat(unitType, unitId, displayName, prefab, icon, maxHp, attackDamage, moveSpeed, attackRange, attackCooldown, vision);
    }
}

[CreateAssetMenu(fileName = "UnitData", menuName = "Data/Combat/Unit Data")]
public class UnitData : ScriptableObject
{
    public List<UnitStat> unitList = new List<UnitStat>();
}
