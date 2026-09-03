using System.Collections.Generic;
using UnityEngine;

// 스테이지의 각 스폰 위치에 적을 한 번에 배치 (CombatObjectPoolManager 사용).
// 위치는 StageData의 오프셋 + 기준점(origin)으로 계산하고,
// 스테이지별 적 스탯(HP/데미지)은 스폰 직후 여기서 덮어쓴다.
public class EnemySpawner : MonoBehaviour
{
    private readonly List<EnemyController> active = new(); // 현재 스테이지 활성 적
    public IReadOnlyList<EnemyController> Spawned => active;

    // origin 기준으로 StageData.SpawnOffsets 위치마다 적을 스폰. 각 적의 타겟은 플레이어.
    public IReadOnlyList<EnemyController> SpawnStage(StageData data, Vector3 origin, BaseUnitController targetForEnemies)
    {
        Clear();

        if (data == null || data.SpawnOffsets == null || data.SpawnOffsets.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] StageData가 비었거나 스폰 오프셋이 없습니다.");
            return active;
        }

        foreach (Vector2 offset in data.SpawnOffsets)
        {
            EnemyController enemy = CombatObjectPoolManager.instance.GetObject<EnemyController>(PoolType.Enemy);
            if (enemy == null)
            {
                Debug.LogWarning("[EnemySpawner] Enemy 풀에서 오브젝트를 가져오지 못했습니다. " +
                                 "(CombatObjectPoolManager objList에 Enemy 프리팹 등록 확인)");
                continue;
            }

            enemy.transform.SetPositionAndRotation(origin + (Vector3)offset, Quaternion.identity);

            // 순서 주의: GetObject 내부 Init→Revive→ResetHealth가 '프리팹 기본값'으로 되돌린 뒤에
            // 이 스테이지 값으로 덮어써야 한다. 그래서 스폰 직후 적용.
            enemy.Health.SetMaxHp(data.EnemyMaxHp);        // resetCurrentHp=true → 새 최대치로 꽉 채움
            enemy.Attack.SetAttackDamage(data.EnemyDamage);

            enemy.SetTarget(targetForEnemies);             // 적은 플레이어를 노림
            active.Add(enemy);
        }

        return active;
    }

    // 현재 스테이지의 적을 모두 풀로 반납 (ReturnObject 내부에서 ReturnToPool()이 정리)
    public void Clear()
    {
        foreach (EnemyController enemy in active)
        {
            if (enemy == null)
                continue;

            CombatObjectPoolManager.instance.ReturnObject(PoolType.Enemy, enemy.gameObject);
        }
        active.Clear();
    }

    // 주어진 위치에서 가장 가까운 살아있는 적 반환 (없으면 null)
    public EnemyController GetNearestAlive(Vector3 from)
    {
        EnemyController nearest = null;
        float bestSqr = float.MaxValue;

        foreach (EnemyController enemy in active)
        {
            if (enemy == null || enemy.Health.IsDead)
                continue;

            float sqr = ((Vector2)(enemy.transform.position - from)).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                nearest = enemy;
            }
        }

        return nearest;
    }
}