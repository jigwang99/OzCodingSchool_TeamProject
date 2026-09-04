using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (!paused)
            return;

        RefreshCollectionState();
        SaveManager.instance?.Save();
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
            MarkCollectionStopped();
            return;
        }

        CollectElapsedFish();
    }

    private void CollectElapsedFish()
    {
        PlayerData data = GameManager.instance.PlayerData;
        DateTime now = DateTime.UtcNow;

        if (data.idleFishLastCollectionUtcTicks <= 0)
        {
            data.idleFishAccumulationEnabled = true;

            if (DateTime.TryParseExact(
                    data.lastSaveTime,
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out DateTime legacySaveTime))
            {
                data.idleFishLastCollectionUtcTicks = legacySaveTime.ToUniversalTime().Ticks;
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
        float totalFish = data.idleFishFraction +
                          (float)(elapsedMinutes * GetFishPerMinute(data));

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

    private static float GetFishPerMinute(PlayerData data)
    {
        float stageMultiplier = 1f + Mathf.Max(0, data.currentStage - 1) * StageBonusPerStage;
        float upgradeMultiplier = 1f + Mathf.Max(0, data.fishDropRateLevel - 1) * FishDropRateBonusPerLevel;

        return BaseFishPerMinute * stageMultiplier * upgradeMultiplier;
    }

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
                CurrencyManager.instance.AddFish(
                    FishGrade.Common,
                    (firstSpecies + offset) % speciesCount,
                    amount);
        }

        data.idleFishNextCommonSpecies = (firstSpecies + remainder) % speciesCount;
    }

    private static bool IsCombatScene()
    {
        return SceneManager.GetActiveScene().name == CombatSceneName;
    }
}