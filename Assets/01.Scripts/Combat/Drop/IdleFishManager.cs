using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IdleFishManager : MonoBehaviour
{
    private const string CombatSceneName = "CombatScene";
    private const double BaseFishPerMinute = 0.5;
    private const double FishDropRateBonusPerLevel = 0.3;
    private const double StageBonusPerStage = 0.1;
    private const float UpdateIntervalSeconds = 1f;
    public const double MaxOfflineSeconds = 8 * 60 * 60;

    public event Action OnPendingRewardsChanged;
    public double PendingSeconds => GameManager.instance.PlayerData.pendingIdleSeconds;
    public long PendingFishCount
    {
        get
        {
            long total = 0;
            for (int grade = 0; grade <= (int)FishGrade.Epic; grade++)
            {
                int[] pending = GameManager.instance.PlayerData.GetPendingIdleFishArray((FishGrade)grade);
                if (pending != null)
                    foreach (int count in pending) total += Math.Max(0, count);
            }
            return total;
        }
    }

    public int GetPendingFish(FishGrade grade, int species)
    {
        int[] pending = GameManager.instance.PlayerData.GetPendingIdleFishArray(grade);
        return pending != null && species >= 0 && species < pending.Length ? pending[species] : 0;
    }

    public bool ClaimPendingRewards()
    {
        if (PendingFishCount <= 0) return false;
        PlayerData data = GameManager.instance.PlayerData;
        int[][] rewards = { data.pendingIdleCommonFish, data.pendingIdleRareFish,
            data.pendingIdleUniqueFish, data.pendingIdleEpicFish };
        // 지급 이벤트가 다시 수령을 호출해도 동일한 보상을 재지급하지 않는다.
        data.pendingIdleCommonFish = new int[8];
        data.pendingIdleRareFish = new int[4];
        data.pendingIdleUniqueFish = new int[2];
        data.pendingIdleEpicFish = new int[1];
        data.pendingIdleSeconds = 0;
        for (int grade = 0; grade < rewards.Length; grade++)
        {
            int[] fish = rewards[grade];
            if (fish == null) continue;
            for (int i = 0; i < fish.Length; i++)
                if (fish[i] > 0) CurrencyManager.instance.AddFish((FishGrade)grade, i, fish[i]);
        }
        SaveManager.instance?.Save();
        OnPendingRewardsChanged?.Invoke();
        return true;
    }

    private float elapsedSinceUpdate;
    private bool sessionInitialized;
    private bool isAway;
    private StageDataList stageDataList;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        if (sessionInitialized && !isAway)
            RefreshCollectionState();
    }

    private void Start()
    {
        // 전투 씬이 로드되기 전에도 전투와 동일한 원본 테이블로 정산한다.
        stageDataList = Resources.Load<StageDataList>("Combat/StageDataList");
        if (stageDataList == null || stageDataList.Count == 0)
        {
            Debug.LogError("[IdleFishManager] 스테이지 보상 테이블을 찾을 수 없습니다.", this);
            enabled = false;
            return;
        }
        PlayerData data = GameManager.instance.PlayerData;
        PrepareLoadedData(data);

        // 시작 씬과 무관하게 미접속 보상을 저장하고, 전투 씬에서 수령한다.
        CollectElapsedFish(true);
        sessionInitialized = true;
        RefreshCollectionState();
        SaveManager.instance?.Save();
    }

    // 자동 저장의 Start보다 먼저, 로드 직후 기존 저장의 미접속 기준 시각을 확정한다.
    public static void PrepareLoadedData(PlayerData data)
    {
        data.InitializePendingIdleFish();
        // 이전 버전의 전투 종료 저장(false)도 마지막 저장 이후의 미접속 시간을 정산한다.
        // 전투 진입 시각을 사용하면 접속 중 전투한 시간까지 지급되므로 저장 시각을 기준으로 한다.
        if (!data.idleFishAccumulationEnabled || data.idleFishLastCollectionUtcTicks <= 0)
        {
            data.idleFishLastCollectionUtcTicks = TryGetLastSaveUtc(data, out DateTime savedUtc)
                ? savedUtc.Ticks : DateTime.UtcNow.Ticks;
            data.idleFishAccumulationEnabled = true;
        }
    }

    private void Update()
    {
        if (!sessionInitialized || isAway || IsCombatScene())
            return;

        elapsedSinceUpdate += Time.unscaledDeltaTime;
        if (elapsedSinceUpdate < UpdateIntervalSeconds)
            return;

        elapsedSinceUpdate = 0f;
        CollectElapsedFish();
    }

    private void OnApplicationPause(bool paused)
    {
        if (!sessionInitialized)
            return;

        if (paused)
        {
            BeginAbsence();
            SaveManager.instance?.Save();
            return;
        }

        if (!isAway)
            return;

        // 전투 씬으로 복귀할 때도 생산 중단 전에 부재 시간을 수령 대기 보상으로 정산한다.
        CollectElapsedFish(true);
        isAway = false;
        elapsedSinceUpdate = 0f;
        RefreshCollectionState();
        SaveManager.instance?.Save();
    }

    private void OnApplicationQuit()
    {
        if (!sessionInitialized)
            return;

        BeginAbsence();
        SaveManager.instance?.Save();
    }

    private void BeginAbsence()
    {
        // Pause 뒤 Quit 또는 중복 Pause가 와도 최초 부재 시작 시각을 유지한다.
        if (isAway)
            return;

        RefreshCollectionState();
        GameManager.instance.PlayerData.idleFishAccumulationEnabled = true;
        isAway = true;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        if (!sessionInitialized || isAway)
            return;

        elapsedSinceUpdate = 0f;
        RefreshCollectionState();
    }

    private void RefreshCollectionState()
    {
        if (GameManager.instance == null || GameManager.instance.PlayerData == null)
            return;

        if (IsCombatScene())
        {
            MarkCollectionStopped();
            return;
        }

        CollectElapsedFish();
    }

    private void CollectElapsedFish(bool offline = false)
    {
        PlayerData data = GameManager.instance.PlayerData;
        DateTime now = DateTime.UtcNow;

        if (data.idleFishLastCollectionUtcTicks <= 0)
        {
            data.idleFishAccumulationEnabled = true;

            if (TryGetLastSaveUtc(data, out DateTime savedUtc))
            {
                data.idleFishLastCollectionUtcTicks = savedUtc.Ticks;
            }
            else
            {
                data.idleFishLastCollectionUtcTicks = now.Ticks;
                return;
            }
        }

        if (!data.idleFishAccumulationEnabled)
        {
            data.idleFishAccumulationEnabled = true;
            data.idleFishLastCollectionUtcTicks = now.Ticks;
            return;
        }

        long elapsedTicks = Math.Max(0L, now.Ticks - data.idleFishLastCollectionUtcTicks);
        data.idleFishLastCollectionUtcTicks = now.Ticks;

        double elapsedMinutes = TimeSpan.FromTicks(elapsedTicks).TotalMinutes;
        if (offline)
        {
            // 정산 시각은 실제 현재 시각으로 이동해 8시간 초과분이 다음 접속에 지급되지 않는다.
            double availableSeconds = Math.Max(0, MaxOfflineSeconds - data.pendingIdleSeconds);
            double rewardedSeconds = Math.Min(TimeSpan.FromTicks(elapsedTicks).TotalSeconds, availableSeconds);
            data.pendingIdleSeconds = Math.Min(MaxOfflineSeconds, data.pendingIdleSeconds + rewardedSeconds);
            elapsedMinutes = rewardedSeconds / 60;
        }
        double totalFish = data.idleFishFraction + elapsedMinutes * GetFishPerMinute(data);

        int wholeFish = (int)Math.Floor(totalFish);
        data.idleFishFraction = (float)(totalFish - wholeFish);

        if (wholeFish > 0)
            AddStageFish(data, wholeFish, offline);
        if (offline) OnPendingRewardsChanged?.Invoke();
    }

    private void MarkCollectionStopped()
    {
        PlayerData data = GameManager.instance.PlayerData;
        data.idleFishAccumulationEnabled = false;
        data.idleFishLastCollectionUtcTicks = DateTime.UtcNow.Ticks;
    }

    private static bool TryGetLastSaveUtc(PlayerData data, out DateTime savedUtc)
    {
        bool parsed = DateTime.TryParseExact(data.lastSaveTime, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime savedTime);
        savedUtc = parsed ? savedTime.ToUniversalTime() : default;
        return parsed;
    }

    private static double GetFishPerMinute(PlayerData data)
    {
        double stageMultiplier = 1 + Mathf.Max(0, data.currentStage - 1) * StageBonusPerStage;
        double upgradeMultiplier = 1 + Mathf.Max(0, data.fishDropRateLevel - 1) * FishDropRateBonusPerLevel;

        return BaseFishPerMinute * stageMultiplier * upgradeMultiplier;
    }

    private void AddStageFish(PlayerData data, int count, bool pendingReward)
    {
        StageDropTable table = stageDataList.GetDropTable(data.currentStage);
        // 생산량은 시간/강화로 결정했으므로 처치당 드롭 확률(0.7)은 다시 적용하지 않는다.
        // 지급은 종별로 합쳐 대량 정산에서도 인벤토리 이벤트를 최대 15번만 발생시킨다.
        int[][] amounts = pendingReward
            ? new[] { data.pendingIdleCommonFish, data.pendingIdleRareFish,
                data.pendingIdleUniqueFish, data.pendingIdleEpicFish }
            : new[] { new int[8], new int[4], new int[2], new int[1] };
        for (int i = 0; i < count; i++)
        {
            if (!FishDropSystem.TryRollFish(table, out FishGrade grade, out int species))
                break;
            int gradeIndex = (int)grade;
            if (gradeIndex >= 0 && gradeIndex < amounts.Length &&
                species >= 0 && species < amounts[gradeIndex].Length)
                amounts[gradeIndex][species]++;
        }
        if (pendingReward) return;
        for (int grade = 0; grade < amounts.Length; grade++)
            for (int species = 0; species < amounts[grade].Length; species++)
                if (amounts[grade][species] > 0)
                    CurrencyManager.instance.AddFish((FishGrade)grade, species, amounts[grade][species]);
    }

    private static bool IsCombatScene()
    {
        return SceneManager.GetActiveScene().name == CombatSceneName;
    }
}
