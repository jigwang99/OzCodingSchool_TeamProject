using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 각 가챠 그룹별 진행도 관리 시스템
    /// 뽑기 횟수 증가, 레벨업 판정, 천장 정보 조회
    /// GachaPityConfig는 참조만 함 (설정 데이터 아님)
    /// </summary>
    public class GachaPitySystem : MonoBehaviour
    {
        private static GachaPitySystem _instance;

        // 그룹별 진행도 (각 그룹마다 독립적)
        private Dictionary<GachaGroup, GachaPityProgress> _progressPerGroup
            = new Dictionary<GachaGroup, GachaPityProgress>();

        // GameManager가 저장 데이터를 불러온 뒤 처음 접근할 때 복원한다.
        private PlayerData _loadedPlayerData;

        private void EnsureLoaded()
        {
            PlayerData data = GameManager.instance != null ? GameManager.instance.PlayerData : null;
            if (data == null || ReferenceEquals(_loadedPlayerData, data)) return;

            _loadedPlayerData = data;
            InitializeProgress();
            if (data.gachaPity == null) data.gachaPity = new GachaPityData();
            if (data.gachaPity.groups == null)
                data.gachaPity.groups = new List<GachaPityGroupData>();

            foreach (GachaPityGroupData saved in data.gachaPity.groups)
            {
                if (saved == null) continue;
                GachaPityProgress progress;
                if (_progressPerGroup.TryGetValue(saved.group, out progress))
                    progress.Restore(saved.totalPullCount, saved.currentLevelPullCount, saved.currentPityLevel);
            }
        }

        private void SyncToPlayerData()
        {
            PlayerData data = GameManager.instance != null ? GameManager.instance.PlayerData : null;
            if (data == null) return;
            if (data.gachaPity == null) data.gachaPity = new GachaPityData();
            if (data.gachaPity.groups == null)
                data.gachaPity.groups = new List<GachaPityGroupData>();
            data.gachaPity.groups.Clear();
            foreach (var entry in _progressPerGroup)
            {
                data.gachaPity.groups.Add(new GachaPityGroupData
                {
                    group = entry.Key,
                    totalPullCount = entry.Value.TotalPullCount,
                    currentLevelPullCount = entry.Value.CurrentLevelPullCount,
                    currentPityLevel = entry.Value.CurrentPityLevel
                });
            }
        }

        public static GachaPitySystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GachaPitySystem>();
                    if (_instance == null)
                    {
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeProgress();
        }

        /// <summary>
        /// 각 그룹의 진행도 초기화
        /// </summary>
        private void InitializeProgress()
        {
            foreach (GachaGroup group in System.Enum.GetValues(typeof(GachaGroup)))
            {
                if (!_progressPerGroup.ContainsKey(group))
                {
                    _progressPerGroup[group] = new GachaPityProgress();
                }
            }
        }

        /// <summary>
        /// 뽑기 횟수 증가
        /// </summary>
        /// <param name="group">그룹</param>
        public void IncreasePullCount(GachaGroup group)
        {
            EnsureLoaded();
            if (!_progressPerGroup.ContainsKey(group))
            {
                _progressPerGroup[group] = new GachaPityProgress();
            }

            _progressPerGroup[group].IncreasePullCount();
            SyncToPlayerData();
        }

        /// <summary>
        /// 현재 진행도 조회
        /// </summary>
        /// <param name="group">그룹</param>
        /// <returns>진행도 객체</returns>
        public GachaPityProgress GetProgress(GachaGroup group)
        {
            EnsureLoaded();
            if (!_progressPerGroup.ContainsKey(group))
            {
                _progressPerGroup[group] = new GachaPityProgress();
            }

            return _progressPerGroup[group];
        }

        /// <summary>
        /// UI용 카운터 조회 (현재 레벨 진행도)
        /// </summary>
        /// <param name="group">그룹</param>
        /// <returns>현재 레벨 내 뽑기 횟수</returns>
        public int GetCurrentUICount(GachaGroup group)
        {
            return GetProgress(group).CurrentLevelPullCount;
        }

        /// <summary>
        /// 현재 천장 레벨 조회
        /// </summary>
        /// <param name="group">그룹</param>
        /// <returns>현재 레벨 (1, 2, 3, ...)</returns>
        public int GetCurrentPityLevel(GachaGroup group)
        {
            return GetProgress(group).CurrentPityLevel;
        }

        /// <summary>
        /// 누적 뽑기 횟수 조회
        /// </summary>
        /// <param name="group">그룹</param>
        /// <returns>전체 누적 횟수</returns>
        public int GetTotalPullCount(GachaGroup group)
        {
            return GetProgress(group).TotalPullCount;
        }

        /// <summary>
        /// 레벨업 여부 확인 및 실행
        /// </summary>
        /// <param name="group">그룹</param>
        /// <param name="config">천장 설정</param>
        /// <returns>레벨업 여부</returns>
        public bool CheckAndLevelUp(GachaGroup group, GachaPityConfig config)
        {
            var progress = GetProgress(group);
            int currentLevel = progress.CurrentPityLevel;

            // 현재 레벨에서 다음 레벨까지 필요한 횟수
            int requiredCount = config.GetRequiredCountForNextLevel(currentLevel);

            // 현재 레벨 내 뽑기 횟수가 요구 횟수 이상인지 확인
            if (progress.CurrentLevelPullCount >= requiredCount)
            {
                progress.LevelUp();
                SyncToPlayerData();
                return true;
            }

            return false;
        }

        /// <summary>
        /// UI에 표시할 "N/M" 텍스트
        /// </summary>
        /// <param name="group">그룹</param>
        /// <param name="config">천장 설정</param>
        /// <returns>"3/20" 형식의 문자열</returns>
        public string GetPityDisplayText(GachaGroup group, GachaPityConfig config)
        {
            var progress = GetProgress(group);
            int currentLevel = progress.CurrentPityLevel;
            int requiredCount = config.GetRequiredCountForNextLevel(currentLevel);

            if (requiredCount == int.MaxValue)
            {
                // 마지막 레벨
                return $"{progress.CurrentLevelPullCount}/(최대)";
            }

            return $"{progress.CurrentLevelPullCount}/{requiredCount}";
        }

        /// <summary>
        /// 진행도 바 채우기 비율 (0~1)
        /// </summary>
        /// <param name="group">그룹</param>
        /// <param name="config">천장 설정</param>
        /// <returns>0 ~ 1 사이의 값</returns>
        public float GetPityProgressFillAmount(GachaGroup group, GachaPityConfig config)
        {
            var progress = GetProgress(group);
            int currentLevel = progress.CurrentPityLevel;
            int requiredCount = config.GetRequiredCountForNextLevel(currentLevel);

            if (requiredCount == int.MaxValue)
                return 1f;

            return Mathf.Clamp01((float)progress.CurrentLevelPullCount / requiredCount);
        }

        /// <summary>
        /// 특정 레벨의 특정 레어리티 가중치 조회
        /// </summary>
        /// <param name="group">그룹</param>
        /// <param name="config">천장 설정</param>
        /// <param name="rarity">레어리티</param>
        /// <returns>가중치</returns>
        public int GetRarityWeight(GachaGroup group, GachaPityConfig config, GachaRarity rarity)
        {
            int currentLevel = GetCurrentPityLevel(group);
            return config.GetWeight(currentLevel, rarity);
        }



    }
}
