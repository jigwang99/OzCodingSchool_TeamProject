using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 스테이지 흐름 관리:
//  1) 플레이어를 시작 위치로 되돌리고 StageData 기준으로 적 스폰
//  2) CombatManager에 승패판정 위임
//  3) 재도전 토글 상태에 따라 진행/후퇴/반복 처리
public class StageManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayercatController playerCat;
    [SerializeField] private FishDropSystem fishDropSystem;

    // 비워두면 씬에 배치된 플레이어의 최초 위치를 시작 위치로 사용
    [SerializeField] private Transform playerSpawnPoint;

    [Header("스테이지 데이터")]
    [SerializeField] private StageDataList stageDataList;   // SO 에셋 하나
    [SerializeField] private Transform enemySpawnOrigin;    // 비우면 enemySpawner 위치를 기준점으로 사용

    [Header("연출 딜레이(초)")]
    [SerializeField, Min(0f)] private float clearDelay = 2f;
    [SerializeField, Min(0f)] private float failDelay = 2f;

    private int CurrentStage => GameManager.instance.PlayerData.currentStage;
    private bool IsRetry => GameManager.instance.PlayerData.isRetryEnabled;
    private int MaxStage => stageDataList != null ? Mathf.Max(1, stageDataList.Count) : 1;

    private Vector3 playerStartPosition;
    private bool isTransitioning;
    private float resultEndsAt;

    public event Action<StageResult> OnStageResult;
    public event Action OnStageStarted;
    public StageResult? CurrentResult { get; private set; }
    public float ResultRemainingSeconds => isTransitioning
        ? Mathf.Max(0f, resultEndsAt - Time.time)
        : 0f;

    private void Start()
    {
        if (combatManager == null || enemySpawner == null || playerCat == null)
        {
            Debug.LogError("[StageManager] 참조가 비어 있습니다.");
            return;
        }

        playerStartPosition = playerSpawnPoint != null
            ? playerSpawnPoint.position
            : playerCat.transform.position;

        combatManager.OnStageCleared += HandleStageCleared;
        combatManager.OnStageFailed += HandleStageFailed;
        combatManager.OnEnemyDefeated += HandleEnemyDefeated;

        StartStage();
    }

    private void OnDestroy()
    {
        if (combatManager != null)
        {
            combatManager.OnStageCleared -= HandleStageCleared;
            combatManager.OnStageFailed -= HandleStageFailed;
            combatManager.OnEnemyDefeated -= HandleEnemyDefeated;
        }
    }

    // 스테이지를 처음부터 시작
    private void StartStage()
    {
        isTransitioning = false;
        CurrentResult = null;

        StageData data = stageDataList != null ? stageDataList.GetClone(CurrentStage) : null;
        if (data == null)
        {
            Debug.LogError("[StageManager] StageData를 가져오지 못했습니다. (StageDataList 확인)");
            return;
        }

        Vector3 origin = enemySpawnOrigin != null
            ? enemySpawnOrigin.position
            : enemySpawner.transform.position;

        fishDropSystem?.SetDropTable(data.DropTable);

        playerCat.transform.position = playerStartPosition;
        playerCat.Revive();

        IReadOnlyList<EnemyController> enemies = enemySpawner.SpawnStage(data, origin, playerCat);
        RetargetPlayer();
        combatManager.BeginBattle(playerCat, enemies);
        OnStageStarted?.Invoke();

        Debug.Log($"[Stage] {data.StageName} 시작 ({CurrentStage}/{MaxStage})");
    }

    // 적이 하나 죽을 때마다 플레이어 타겟을 가장 가까운 살아있는 적으로 갱신
    private void HandleEnemyDefeated(EnemyController _)
    {
        RetargetPlayer();
    }

    private void RetargetPlayer()
    {
        EnemyController nearest = enemySpawner.GetNearestAlive(playerCat.transform.position);
        if (nearest != null)
            playerCat.SetTarget(nearest);
    }

    // 승리
    private void HandleStageCleared()
    {
        if (isTransitioning) return;
        int completedStage = CurrentStage;
        bool retryWasEnabled = IsRetry;

        if (!IsRetry && CurrentStage < MaxStage)
            GameManager.instance.PlayerData.SetCurrentStage(CurrentStage + 1);

        RestartAfterAsync(CreateResult(true, completedStage, retryWasEnabled, clearDelay)).Forget();
    }

    // 패배
    private void HandleStageFailed()
    {
        if (isTransitioning) return;
        int completedStage = CurrentStage;
        bool retryWasEnabled = IsRetry;

        if (!IsRetry)
        {
            GameManager.instance.PlayerData.SetCurrentStage(CurrentStage - 1); // 세터 내부에서 최하 1 보장
            GameManager.instance.PlayerData.SetRetryEnabled(true);
        }

        RestartAfterAsync(CreateResult(false, completedStage, retryWasEnabled, failDelay)).Forget();
    }

    private StageResult CreateResult(bool isClear, int completedStage, bool retryWasEnabled, float delay)
    {
        string completedName = stageDataList?.GetClone(completedStage)?.StageName;
        string nextName = stageDataList?.GetClone(CurrentStage)?.StageName;
        return new StageResult(isClear, completedStage, CurrentStage,
            string.IsNullOrEmpty(completedName) ? completedStage.ToString() : completedName,
            string.IsNullOrEmpty(nextName) ? CurrentStage.ToString() : nextName,
            retryWasEnabled, Mathf.Max(0f, delay));
    }

    private async UniTaskVoid RestartAfterAsync(StageResult result)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        CurrentResult = result;
        resultEndsAt = Time.time + result.Delay;
        OnStageResult?.Invoke(result);

        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(result.Delay),
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException) { return; }

        StartStage();
    }
}
