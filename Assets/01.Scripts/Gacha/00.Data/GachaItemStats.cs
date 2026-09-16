using System;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    [Serializable]
    public class GachaItemStats
    {
        [Header("Weapon")]
        [Tooltip("무기 공격력 +N")]
        [Min(0)]
        public int attackPower = 0;

        [Header("Furniture")]
        [Tooltip("가구 골드 증가 +N")]
        [Min(0)]
        public int goldBonusFlat = 0;

        [Header("Recipe")]
        [Tooltip("레시피 골드 증가 +N%")]
        [Min(0)]
        public float goldBonusRate = 0f;
    }
}