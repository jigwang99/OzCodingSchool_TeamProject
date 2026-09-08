using UnityEngine;

// CombatScene에서 발생한 드롭만 집계한다. 실제 재고 지급은 FishInventoryAdapter가 담당한다.
[DisallowMultipleComponent]
public class CombatRewardTracker : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private FishDropSystem fishDropSystem;

    public long TotalFishCount { get; private set; }

    private void OnEnable()
    {
        if (stageManager == null || fishDropSystem == null)
        {
            Debug.LogError("[CombatRewardTracker] StageManager와 FishDropSystem을 연결하세요.", this);
            return;
        }

        stageManager.OnStageStarted += ResetRewards;
        fishDropSystem.OnFishDropped += HandleFishDropped;
    }

    private void OnDisable()
    {
        if (stageManager != null)
            stageManager.OnStageStarted -= ResetRewards;
        if (fishDropSystem != null)
            fishDropSystem.OnFishDropped -= HandleFishDropped;
    }

    private void HandleFishDropped(FishDrop drop)
    {
        if (drop.Count > 0)
            TotalFishCount += drop.Count;
    }

    private void ResetRewards()
    {
        // 결과/진행도 변경 시에는 유지하고, 다음 전투가 실제 시작될 때만 초기화한다.
        TotalFishCount = 0;
    }
}
