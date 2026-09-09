using UnityEngine;

namespace PixelRestaurant.Data
{
    /// <summary>
    /// 가챠 레어리티 (Common, Rare, Unique, Epic)
    /// 숫자값 없음: 확률은 GachaPityConfig에서 관리
    /// </summary>
    public enum GachaRarity
    {
        /// <summary>
        /// 커몬 등급
        /// 기본: 60%, Level 2: 40%, Level 3: 20%
        /// </summary>
        Common,

        /// <summary>
        /// 레어 등급
        /// 기본: 30%, Level 2: 40%, Level 3: 40%
        /// </summary>
        Rare,

        /// <summary>
        /// 유니크 등급
        /// 기본: 9%, Level 2: 15%, Level 3: 30%
        /// </summary>
        Unique,

        /// <summary>
        /// 에픽 등급
        /// 기본: 1%, Level 2: 5%, Level 3: 10%
        /// </summary>
        Epic
    }
}