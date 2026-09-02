using System.Collections.Generic;
using UnityEngine;

// 스테이지의 각 스폰 위치에 적을 한 번에 배치 (ObjectPoolManager 사용).
// 상태 초기화/정리는 풀의 Init()/ReturnToPool() 훅이 담당하므로,
// 여기서는 '어디에 두고 누구를 노리게 할지'만 담당한다.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; // 스테이지 내 적 위치들

    private readonly List<EnemyController> active = new(); // 현재 스테이지의 활성 적
    public IReadOnlyList<EnemyController> Spawned => active;

    // 모든 스폰 위치에 적을 풀에서 꺼내 배치. 각 적의 타겟은 플레이어로 지정.
    public IReadOnlyList<EnemyController> SpawnStage(BaseUnitController targetForEnemies)
    {
        Clear();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[EnemySpawner] 스폰 위치가 비어 있습니다.");
            return active;
        }

        foreach (Transform point in spawnPoints)
        {
            if (point == null)
            {
                continue;
            }

            // GetObject 내부에서 IPoolable.Init()이 호출돼 HP/상태가 초기화됨
            EnemyController enemy = CombatObjectPoolManager.instance.GetObject<EnemyController>(PoolType.Enemy);
            if (enemy == null)
            {
                Debug.LogWarning("[EnemySpawner] Enemy 풀에서 오브젝트를 가져오지 못했습니다. " +
                                 "(ObjectPoolManager objList에 Enemy 프리팹 등록 확인)");
                continue;
            }

            enemy.transform.SetPositionAndRotation(point.position, Quaternion.identity);
            enemy.SetTarget(targetForEnemies); // 적은 플레이어를 노림
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
            {
                continue;
            }

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
            {
                continue;
            }

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