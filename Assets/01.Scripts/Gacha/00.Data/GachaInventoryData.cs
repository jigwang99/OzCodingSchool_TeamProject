using System;
using System.Collections.Generic;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠 인벤토리 저장용 데이터
    /// PlayerData에 포함되어 SaveManager를 통해 저장됩니다.
    /// </summary>
    [Serializable]
    public class GachaInventoryData
    {
        // 보유 아이템 목록
        public List<GachaOwnedItemData> items = new List<GachaOwnedItemData>();
    }

    /// <summary>
    /// 가챠 아이템 하나의 저장 데이터
    /// </summary>
    [Serializable]
    public class GachaOwnedItemData
    {
        public string itemId;
        public int count;

        // 인벤토리에서 NEW 표시 여부
        public bool isNew;
    }
}

