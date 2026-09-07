using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerWeaponCatalog", menuName = "Combat/Player Weapon Catalog")]
public class PlayerWeaponCatalog : ScriptableObject
{
    [Serializable]
    public class Weapon
    {
        public string id;
        public GameObject prefab;
        [Min(0f)] public float baseDamage = 10f;
        [Min(0f)] public float damagePerLevel = 5f;

        public float GetDamage(int level)
        {
            return Mathf.Max(0f, baseDamage) + Mathf.Max(0, level - 1) * Mathf.Max(0f, damagePerLevel);
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
