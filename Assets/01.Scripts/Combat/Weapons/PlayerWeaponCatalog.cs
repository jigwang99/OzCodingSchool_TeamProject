using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum WeaponTraits
{
    None = 0,
    Skill = 1,
    StrongDamage = 2,
    HighCriticalChance = 4,
    FastAttack = 8
}

[CreateAssetMenu(fileName = "PlayerWeaponCatalog", menuName = "Combat/Player Weapon Catalog")]
public class PlayerWeaponCatalog : ScriptableObject
{
    [Serializable]
    public class SkillSettings
    {
        [Min(0.1f)] public float duration;
        [Min(0f)] public float damageBonus;
        [Range(0f, 1f)] public float criticalChanceBonus;
        [Min(0f)] public float attackSpeedBonus;
        [Min(0f)] public float moveSpeedBonus;
        [Min(0.1f)] public float cooldown;

        public SkillSettings(float duration, float bonus, float criticalBonus, float cooldown)
        {
            this.duration = duration;
            damageBonus = attackSpeedBonus = moveSpeedBonus = bonus;
            criticalChanceBonus = criticalBonus;
            this.cooldown = cooldown;
        }
    }

    [Header("등급별 자동 스킬")]
    [SerializeField] private SkillSettings commonSkill = new SkillSettings(5f, 0.2f, 0.05f, 20f);
    [SerializeField] private SkillSettings rareSkill = new SkillSettings(7f, 0.3f, 0.1f, 20f);
    [SerializeField] private SkillSettings uniqueSkill = new SkillSettings(9f, 0.4f, 0.15f, 20f);
    [SerializeField] private SkillSettings epicSkill = new SkillSettings(11f, 0.5f, 0.2f, 20f);

    public SkillSettings GetSkillSettings(Weapon weapon)
    {
        switch (weapon.rarity)
        {
            case PixelRestaurant.Data.GachaRarity.Rare: return rareSkill;
            case PixelRestaurant.Data.GachaRarity.Unique: return uniqueSkill;
            case PixelRestaurant.Data.GachaRarity.Epic: return epicSkill;
            default: return commonSkill;
        }
    }

    [Serializable]
    public class Weapon
    {
        public string id;
        public string displayName;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? id : displayName;
        public GameObject prefab;
        public PixelRestaurant.Data.GachaRarity rarity;
        [Min(0f)] public float baseDamage = 10f;
        [Min(0f)] public float damagePerLevel = 5f;
        public WeaponTraits traits;
        [Min(1f)] public float strongDamageMultiplier = 1.3f;
        [Range(0f, 1f)] public float criticalChanceBonus = 0.15f;
        [Range(0.1f, 1f)] public float fastAttackIntervalMultiplier = 0.75f;

        public bool HasTrait(WeaponTraits trait) => (traits & trait) != 0;

        public string GetTraitDescription()
        {
            var descriptions = new List<string>();
            if (HasTrait(WeaponTraits.Skill))
                descriptions.Add("자동 스킬: 피해·치명타·공속·이속 강화");
            if (HasTrait(WeaponTraits.StrongDamage))
                descriptions.Add($"무기 피해 +{(Mathf.Max(1f, strongDamageMultiplier) - 1f) * 100f:0.#}%");
            if (HasTrait(WeaponTraits.HighCriticalChance))
                descriptions.Add($"치명타 확률 +{Mathf.Clamp01(criticalChanceBonus) * 100f:0.#}%p");
            if (HasTrait(WeaponTraits.FastAttack))
                descriptions.Add($"공격 간격 -{(1f - Mathf.Clamp(fastAttackIntervalMultiplier, 0.1f, 1f)) * 100f:0.#}%");
            return string.Join(" · ", descriptions);
        }

        public float GetDamage(int level)
        {
            float damage = Mathf.Max(0f, baseDamage) + Mathf.Max(0, level - 1) * Mathf.Max(0f, damagePerLevel);
            return damage * (HasTrait(WeaponTraits.StrongDamage) ? Mathf.Max(1f, strongDamageMultiplier) : 1f);
        }

        public Sprite GetSprite()
        {
            SpriteRenderer renderer = prefab != null ? prefab.GetComponent<SpriteRenderer>() : null;
            return renderer != null ? renderer.sprite : null;
        }
    }

    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    public IReadOnlyList<Weapon> Weapons => weapons;

    public Weapon Find(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        return weapons.Find(weapon => weapon != null && weapon.id == id);
    }
}
