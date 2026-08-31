using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private PlayercatController playerCat;
    [SerializeField] private EnemyController enemyPrefab;
    [SerializeField] private Transform enemySpawnPoint;
    [SerializeField, Min(0f)] private float respawnDelay = 1f;
    [SerializeField, Min(0f)] private float defeatDelay = 2f;

    // 전투 진행 중 대기 태스크(리스폰 등)를 묶는 취소 스코프
    private CancellationTokenSource battleCts;
    private EnemyController currentEnemy;
    private bool isBattleOver;

    // 다른 팀원(UI/경영/스테이지)이 구독할 패배 신호
    public event Action OnPlayerDefeated;

    private void Start()
    {
        if (playerCat == null || enemyPrefab == null || enemySpawnPoint == null)
        {
            Debug.LogError("[CombatManager] 참조가 비어 있습니다.");
            return;
        }

        playerCat.Health.OnDied += HandlePlayerDied;
        StartBattle();
    }

    private void OnDestroy()
    {
        if (playerCat != null)
        {
            playerCat.Health.OnDied -= HandlePlayerDied;
        }

        CancelBattleScope();
    }

    // 전투 스코프를 새로 열고 첫 적을 스폰
    private void StartBattle()
    {
        battleCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy());
        isBattleOver = false;
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        currentEnemy = Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);
        currentEnemy.Health.OnDied += HandleEnemyDied;

        playerCat.SetTarget(currentEnemy);
        currentEnemy.SetTarget(playerCat);
    }

    private void HandleEnemyDied()
    {
        if (isBattleOver)
        {
            return;
        }

        currentEnemy.Health.OnDied -= HandleEnemyDied;
        // TODO: 물고기 드롭 이벤트 발행 (재화 담당 연동 지점)
        RespawnAsync(currentEnemy, battleCts.Token).Forget();
    }

    private async UniTaskVoid RespawnAsync(EnemyController dead, CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(respawnDelay),
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (dead != null)
        {
            Destroy(dead.gameObject);
        }

        SpawnEnemy();
    }

    private void HandlePlayerDied()
    {
        if (isBattleOver)
        {
            return;
        }
        isBattleOver = true;

        // 진행 중이던 리스폰 대기 중단
        CancelBattleScope();

        // 현재 적 정리
        if (currentEnemy != null)
        {
            currentEnemy.Health.OnDied -= HandleEnemyDied;
            Destroy(currentEnemy.gameObject);
            currentEnemy = null;
        }

        // 패배 신호 발행 (UI/경영/스테이지가 각자 반응)
        OnPlayerDefeated?.Invoke();

        // TODO: 재도전 토글에 따라 currentStage 조정

        // 재시작 (오브젝트 파괴에만 묶어 딜레이 대기)
        RestartBattleAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid RestartBattleAsync(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(defeatDelay),
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        playerCat.Revive();  // DieState 탈출
        StartBattle();
    }

    private void CancelBattleScope()
    {
        battleCts?.Cancel();
        battleCts?.Dispose();
        battleCts = null;
    }
}