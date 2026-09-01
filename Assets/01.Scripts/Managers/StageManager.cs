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

        playerCat.transform.position = playerStartPosition; // 원래 위치로 복귀
        playerCat.Revive();                                 // 풀피 + Idle (이동 정지 포함)

        IReadOnlyList<EnemyController> enemies = enemySpawner.SpawnStage(playerCat);
        RetargetPlayer();  // 시작 위치 기준 가장 가까운 적을 첫 타겟으로
        combatManager.BeginBattle(playerCat, enemies);
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

    // 승리 → 같은 스테이지 재시작
    private void HandleStageCleared()
    {
        // TODO: 재도전 토글 off 시 currentStage++ 로 다음 스테이지 진행 (경영·재화 담당 연동)
        RestartAfterAsync(clearDelay).Forget();
    }

    // 패배 → 같은 스테이지 재시작
    private void HandleStageFailed()
    {
        // TODO: 실패 시 이전 스테이지로 이동 (경영·재화 담당 연동)
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