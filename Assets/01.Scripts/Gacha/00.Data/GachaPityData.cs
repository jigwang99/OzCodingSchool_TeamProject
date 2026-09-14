using PixelRestaurant.Data;
using System;
using System.Collections.Generic;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠 천장 시스템 전체 저장 데이터
    /// </summary>
    [Serializable]
    public class GachaPityData
    {
        public List<GachaPityGroupData> groups = new List<GachaPityGroupData>();
    }

    /// <summary>
    /// 가챠 그룹 하나의 천장 진행도
    /// </summary>
    [Serializable]
    public class GachaPityGroupData
    {
        public GachaGroup group;

        // 누적 뽑기 횟수
        public int totalPullCount;

        // 현재 레벨에서 뽑은 횟수
        public int currentLevelPullCount;

        // 현재 천장 레벨
        public int currentPityLevel = 1;
    }
}

