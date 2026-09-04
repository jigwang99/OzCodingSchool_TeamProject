using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

// 전투 씬 밖에서만 고정 비율로 물고기를 생산한다.
// 플레이 중에는 매 초, 앱을 다시 열었을 때는 마지막 저장 시각부터의 경과 시간으로 정산한다.
public class IdleFishManager : MonoBehaviour
{
    private const string CombatSceneName = "CombatScene";
    private const float BaseFishPerMinute = 1f;
    private const float FishDropRateBonusPerLevel = 0.3f;
    private const float StageBonusPerStage = 0.1f;
    private const float UpdateIntervalSeconds = 1f;

    private float elapsedSinceUpdate;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        RefreshCollectionState();
    }

    private void Start()
    {
        // GameManager Awake에서 추가되는 컴포넌트도 첫 프레임에 오프라인 보상을 정산한다.
        RefreshCollectionState();
    }

    private void Update()
    {
        if (IsCombatScene())
            return;

        elapsedSinceUpdate += Time.unscaledDeltaTime;
        if (elapsedSinceUpdate < UpdateIntervalSeconds)
            return;

        elapsedSinceUpdate = 0f;
        CollectElapsedFish();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            RefreshCollectionState();
            SaveManager.instance?.Save();
        }
    }

    private void OnApplicationQuit()
    {
        RefreshCollectionState();
        SaveManager.instance?.Save();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        elapsedSinceUpdate = 0f;
        RefreshCollectionState();
    }

    private void RefreshCollectionState()
    {
        if (GameManager.instance == null || GameManager.instance.PlayerData == null)
            return;

        if (IsCombatScene())
        {
            // 전투 중이던 시간은 다음 씬이나 다음 실행 때 보상으로 환산하지 않는다.
            MarkCollectionStopped();
            return;
        }

        CollectElapsedFish();
    }

    private void CollectElapsedFish()
    {
        PlayerData data = GameManager.instance.PlayerData;
        DateTime now = DateTime.UtcNow;

        // 기존 저장 데이터는 lastSaveTime을 기준으로 한 번 정산해 마이그레이션한다.
        if (data.idleFishLastCollectionUtcTicks <= 0)
        {
            data.idleFishAccumulationEnabled = true;
            if (DateTime.TryParseExact(data.lastSaveTime, "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime legacySaveTime))
                data.idleFishLastCollectionUtcTicks = legacySaveTime.ToUniversalTime().Ticks;
            else
            {
                data.idleFishLastCollectionUtcTicks = now.Ticks;
                return;
            }
        }

        // 전투 중에 종료된 저장은 생산 중단 상태로 저장되므로 보상 없이 기준 시각만 재설정한다.
        if (!data.idleFishAccumulationEnabled)
        {
            data.idleFishAccumulationEnabled = true;
            data.idleFishLastCollectionUtcTicks = now.Ticks;
            return;
        }

        long elapsedTicks = Math.Max(0L, now.Ticks - data.idleFishLastCollectionUtcTicks);
        data.idleFishLastCollectionUtcTicks = now.Ticks;

        double elapsedMinutes = TimeSpan.FromTicks(elapsedTicks).TotalMinutes;
        float totalFish = data.idleFishFraction + (float)(elapsedMinutes * GetFishPerMinute(data));
        int wholeFish = Mathf.FloorToInt(totalFish);
        data.idleFishFraction = totalFish - wholeFish;

        if (wholeFish > 0)
            AddCommonFishEvenly(data, wholeFish);
    }

    private void MarkCollectionStopped()
    {
        PlayerData data = GameManager.instance.PlayerData;
        data.idleFishAccumulationEnabled = false;
        data.idleFishLastCollectionUtcTicks = DateTime.UtcNow.Ticks;
    }

    // 기본 분당 1마리. 스테이지와 기존 드롭률 업그레이드 레벨이 생산량을 함께 높인다.
    private static float GetFishPerMinute(PlayerData data)
    {
        float stageMultiplier = 1f + Mathf.Max(0, data.currentStage - 1) * StageBonusPerStage;
        float upgradeMultiplier = 1f + Mathf.Max(0, data.fishDropRateLevel - 1) * FishDropRateBonusPerLevel;
        return BaseFishPerMinute * stageMultiplier * upgradeMultiplier;
    }

    // Common 1~8에 순환 방식으로 균등 지급한다.
    // 예: 한 마리씩 생산되어도 1 → 2 → ... → 8 순서로 쌓인다.
    private static void AddCommonFishEvenly(PlayerData data, int count)
    {
        int speciesCount = data.commonFish != null ? data.commonFish.Length : 0;
        if (speciesCount == 0)
            return;

        int firstSpecies = Mathf.Clamp(data.idleFishNextCommonSpecies, 0, speciesCount - 1);
        int fishPerSpecies = count / speciesCount;
        int remainder = count % speciesCount;

        for (int offset = 0; offset < speciesCount; offset++)
        {
            int amount = fishPerSpecies + (offset < remainder ? 1 : 0);
            if (amount > 0)
                CurrencyManager.instance.AddFish(FishGrade.Common, (firstSpecies + offset) % speciesCount, amount);
        }

        data.idleFishNextCommonSpecies = (firstSpecies + remainder) % speciesCount;
    }

    private static bool IsCombatScene()
    {
        return SceneManager.GetActiveScene().name == CombatSceneName;
    }
}
