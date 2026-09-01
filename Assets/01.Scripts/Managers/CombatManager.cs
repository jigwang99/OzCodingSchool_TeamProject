using System;
using System.Collections.Generic;
using UnityEngine;

// 승패판정 전담:
//  - 스테이지 내 모든 적이 처치되면 OnStageCleared (승리)
//  - 플레이어 캣이 사망하면 OnStageFailed (패배)
// 스폰/재시작/스테이지 흐름은 관여하지 않는다. StageManager가 BeginBattle로 대상을 넣어준다.
public class CombatManager : MonoBehaviour
{
    private PlayercatController player;

    // 적별 OnDied 핸들러를 보관해 정확히 해제하기 위한 맵
    private readonly Dictionary<EnemyController, Action> enemyDeathHandlers = new();
    private int aliveEnemyCount;
    private bool isJudging;

    // StageManager / UI 등이 구독
    public event Action OnStageCleared;
    public event Action OnStageFailed;
    public event Action<EnemyController> OnEnemyDefeated; // 개별 적 처치(리타게팅 등에서 사용)

    // 이번 스테이지의 판정 시작
    public void BeginBattle(PlayercatController playerCat, IReadOnlyList<EnemyController> stageEnemies)
    {
        StopBattle(); // 이전 스테이지 구독 정리

        player = playerCat;
        player.Health.OnDied += HandlePlayerDied;

        aliveEnemyCount = 0;
        foreach (EnemyController enemy in stageEnemies)
        {
            if (enemy == null)
            {
                continue;
            }

            // 클로저로 어떤 적이 죽었는지 식별 (OnDied는 인자가 없음)
            Action handler = () => HandleEnemyDied(enemy);
            enemyDeathHandlers.Add(enemy, handler);
            enemy.Health.OnDied += handler;
            aliveEnemyCount++;
        }

        if (aliveEnemyCount <= 0)
        {
            Debug.LogWarning("[CombatManager] 스폰된 적이 없어 판정을 시작하지 않습니다. (스폰 위치 확인)");
            return;
        }

        isJudging = true;
    }

    // 판정 중단 및 모든 구독 해제
    public void StopBattle()
    {
        isJudging = false;

        if (player != null)
        {
            player.Health.OnDied -= HandlePlayerDied;
            player = null;
        }

        foreach (KeyValuePair<EnemyController, Action> pair in enemyDeathHandlers)
        {
            if (pair.Key != null)
            {
                pair.Key.Health.OnDied -= pair.Value;
            }
        }
        enemyDeathHandlers.Clear();
        aliveEnemyCount = 0;
    }

    private void HandleEnemyDied(EnemyController enemy)
    {
        if (!isJudging)
        {
            return;
        }

        if (enemyDeathHandlers.TryGetValue(enemy, out Action handler))
        {
            enemy.Health.OnDied -= handler;
            enemyDeathHandlers.Remove(enemy);
        }

        aliveEnemyCount--;
        OnEnemyDefeated?.Invoke(enemy);

        // 스테이지 내 모든 적 처치 → 승리
        if (aliveEnemyCount <= 0)
        {
            isJudging = false;
            OnStageCleared?.Invoke();
        }
    }

    private void HandlePlayerDied()
    {
        if (!isJudging)
        {
            return;
        }

        // 플레이어 캣 사망 → 패배
        isJudging = false;
        OnStageFailed?.Invoke();
    }

    private void OnDestroy()
    {
        StopBattle();
    }
}