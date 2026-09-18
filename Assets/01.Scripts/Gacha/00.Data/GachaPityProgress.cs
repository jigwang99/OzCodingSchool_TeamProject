using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 플레이어의 뽑기 진행도 데이터
    /// 이중 카운터: 누적(totalPullCount) + 현재 레벨(currentLevelPullCount)
    /// SaveManager에서 저장될 데이터
    /// </summary>
    [System.Serializable]
    public class GachaPityProgress
    {
        [SerializeField]
        private int totalPullCount = 0;

        [SerializeField]
        private int currentLevelPullCount = 0;

        [SerializeField]
        private int currentPityLevel = 1;

        public int TotalPullCount => totalPullCount;
        public int CurrentLevelPullCount => currentLevelPullCount;
        public int CurrentPityLevel => currentPityLevel;

        /// <summary>
        /// 뽑기 횟수 증가 (1회 뽑기 후 호출)
        /// totalPullCount와 currentLevelPullCount 모두 증가
        /// </summary>
        public void IncreasePullCount()
        {
            totalPullCount++;
            currentLevelPullCount++;

        }

        /// <summary>
        /// 레벨업 (다음 레벨 도달 시 호출)
        /// currentLevelPullCount는 0으로 리셋, currentPityLevel은 증가
        /// totalPullCount는 절대 리셋하지 않음
        /// </summary>
        public void LevelUp()
        {
            currentLevelPullCount = 0;
            currentPityLevel++;

        }

        /// <summary>
        /// 진행도 리셋 (디버그/테스트용)
        /// </summary>
        public void Reset()
        {
            totalPullCount = 0;
            currentLevelPullCount = 0;
            currentPityLevel = 1;

        }

        /// <summary>
        /// 현재 진행도 정보 출력
        /// </summary>
        public void DebugPrint(string groupName)
        {
            
        }
    }
}
