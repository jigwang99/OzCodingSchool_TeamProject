using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 스테이지 흐름 관리:
//  1) 플레이어를 시작 위치로 되돌리고 EnemySpawner로 적을 한 번에 스폰
//  2) CombatManager에 승패판정 위임
//  3) 승리/패배 결과에 따라 같은 스테이지를 다시 시작 (재도전 반복)
// 다음/이전 스테이지 이동, 재도전 토글은 PlayerData(재화·저장 담당) 연동이 필요하므로 TODO로 표시.
public class StageManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayercatController playerCat;

    // 비워두면 씬에 배치된 플레이어의 최초 위치를 시작 위치로 사용
    [SerializeField] private Transform playerSpawnPoint;

    [Header("연출 딜레이(초)")]
    [SerializeField, Min(0f)] private float clearDelay = 2f; // 클리어 후 재시작까지
    [SerializeField, Min(0f)] private float failDelay = 2f;  // 패배 후 재시작까지

    [SerializeField] private FishDropSystem fishDropSystem;

    [Header("스테이지 드롭 테이블 (스테이지 번호 순서대로)")]
    [SerializeField] private StageDropTable[] stageDropTables; // index 0 = 1스테이지
    [SerializeField] private StageDropTable fallbackDropTable; // 목록에 없을 때

    private int CurrentStage => GameManager.instance.PlayerData.currentStage;
    private bool IsRetry => GameManager.instance.PlayerData.isRetryEnabled;

    private Vector3 playerStartPosition; // 스테이지 재시작 시 되돌릴 위치
    private bool isTransitioning;        // 재시작 대기 중 중복 진입 방지

    private void Start()
    {
        if (combatManager == null || enemySpawner == null || playerCat == null)
        {
            Debug.LogError("[StageManager] 참조가 비어 있습니다.");
            return;
        }

        // 첫 스테이지 시작 전에 시작 위치 확정
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

        ApplyDropTable(); // ← 추가: 이번 스테이지의 드롭 테이블을 FishDropSystem에 지정

        playerCat.transform.position = playerStartPosition;
        playerCat.Revive();

        IReadOnlyList<EnemyController> enemies = enemySpawner.SpawnStage(playerCat);
        RetargetPlayer();
        combatManager.BeginBattle(playerCat, enemies);
    }

    // 현재 스테이지 번호에 맞는 드롭 테이블을 선택해 전달.
    // 목록 범위를 넘으면 마지막 테이블로 고정(= 최상위 스테이지 규칙 유지), 그래도 없으면 fallback.
    private void ApplyDropTable()
    {
        if (fishDropSystem == null)
            return;

        StageDropTable table = fallbackDropTable;

        if (stageDropTables != null && stageDropTables.Length > 0)
        {
            int index = Mathf.Clamp(CurrentStage - 1, 0, stageDropTables.Length - 1);
            if (stageDropTables[index] != null)
                table = stageDropTables[index];
        }

        fishDropSystem.SetDropTable(table);
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
        {
            playerCat.SetTarget(nearest);
        }
    }

    // 승리 → 재도전 OFF: 다음 스테이지 (마지막 스테이지는 무한 반복) / 재도전 ON: 현재 스테이지 반복
    private void HandleStageCleared()
    {
        if (!IsRetry)
        {
            int maxStage = (stageDropTables != null && stageDropTables.Length > 0) ? stageDropTables.Length : 1;

            if (CurrentStage < maxStage)
            {
                GameManager.instance.PlayerData.currentStage++;
            }
        }

        RestartAfterAsync(clearDelay).Forget();
    }

    // 패배 → 재도전 OFF: 이전 스테이지 후퇴 + 재도전 토글 자동 ON / 재도전 ON: 현재 스테이지 재시작
    private void HandleStageFailed()
    {
        if (!IsRetry)
        {
            // 1. 이전 스테이지로 후퇴 (최하 1스테이지 유지)
            GameManager.instance.PlayerData.currentStage = Mathf.Max(1, CurrentStage - 1);

            // 2. 무한 패배 루프 방지를 위한 재도전 토글 자동 ON
            GameManager.instance.PlayerData.isRetryEnabled = true;
        }

        RestartAfterAsync(failDelay).Forget();
    }

    private async UniTaskVoid RestartAfterAsync(float delay)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        Debug.Log($"[Stage] 재시작 대기 시작: {delay}s (t={Time.time:F2})");
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay),
                cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException) { return; }
        Debug.Log($"[Stage] 재시작 (t={Time.time:F2})");

        StartStage();
    }
}