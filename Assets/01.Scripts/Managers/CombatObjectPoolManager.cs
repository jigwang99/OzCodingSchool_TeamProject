using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CombatObjectPoolManager : Singleton<CombatObjectPoolManager>
{
    [SerializeField] private List<GameObject> objList;
    [Header("스테이지 간 풀 보관")]
    [Tooltip("현재 스테이지 필수 수량 외에 보관할 적 인스턴스 총수. 최근 사용한 종류부터 유지하며, 0이면 여분을 모두 제거합니다.")]
    [SerializeField, Min(0)] private int maxRetainedExtraEnemies = 32;
    private readonly Dictionary<PoolType, GameObject> prefabs = new();
    private readonly Dictionary<PoolType, Pool> pools = new();
    private readonly Dictionary<PoolType, int> required = new();
    private readonly Dictionary<PoolType, long> lastUsed = new();
    private readonly List<PoolType> retentionOrder = new();
    private long preparationVersion;

    protected override void Awake()
    {
        base.Awake();
        if (instance != this) return;
        // 씬 진입 시에는 프리팹만 등록한다. 인스턴스는 현재 스테이지만 준비한다.
        if (objList == null) return;
        foreach (GameObject prefab in objList)
            if (prefab != null && prefab.TryGetComponent(out EnemyController enemy))
                prefabs[(PoolType)enemy.PoolKey] = prefab;
    }

    public bool CanPrepare(StageData data)
    {
        if (data == null || data.EnemyCount == 0) return false;
        for (int i = 0; i < data.EnemyCount; i++)
            if (!prefabs.ContainsKey(data.GetEnemyType(i))) return false;
        return true;
    }

    // 이전 적을 모두 반납한 상태에서, 검은 화면 동안 분산 생성한다.
    public async UniTask PrepareStageAsync(StageData data, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!CanPrepare(data)) throw new InvalidOperationException("스테이지의 적 프리팹이 풀에 등록되지 않았습니다.");
        required.Clear();
        for (int i = 0; i < data.EnemyCount; i++)
        {
            PoolType type = data.GetEnemyType(i);
            required.TryGetValue(type, out int count);
            required[type] = count + 1;
        }
        preparationVersion++;
        foreach (PoolType type in required.Keys) lastUsed[type] = preparationVersion;
        TrimRetainedPools();
        foreach (var pair in required)
        {
            token.ThrowIfCancellationRequested();
            if (!pools.TryGetValue(pair.Key, out Pool pool))
            {
                var root = new GameObject($"{prefabs[pair.Key].name}_Pool");
                root.transform.SetParent(transform, false);
                pool = new Pool(prefabs[pair.Key], root.transform, 0);
                pools.Add(pair.Key, pool);
            }
            while (pool.AvailableCount < pair.Value)
            {
                pool.Resize(Mathf.Min(pair.Value, pool.AvailableCount + 2));
                await UniTask.NextFrame(token);
            }
        }
    }

    // 모든 적이 반납된 전환 시점에만 호출한다. 필수 수량은 보호하고 여분만 총 보관 한도로 제한한다.
    private void TrimRetainedPools()
    {
        retentionOrder.Clear();
        retentionOrder.AddRange(pools.Keys);
        retentionOrder.Sort(CompareRetentionPriority);
        int remaining = Mathf.Max(0, maxRetainedExtraEnemies);
        foreach (PoolType type in retentionOrder)
        {
            Pool pool = pools[type];
            required.TryGetValue(type, out int requiredCount);
            int extra = Mathf.Max(0, pool.AvailableCount - requiredCount);
            int retained = Mathf.Min(extra, remaining);
            remaining -= retained;
            if (requiredCount == 0 && retained == 0)
            {
                pool.Dispose();
                pools.Remove(type);
                lastUsed.Remove(type);
            }
            else if (extra > retained)
            {
                pool.Resize(requiredCount + retained);
            }
        }
    }

    private int CompareRetentionPriority(PoolType left, PoolType right)
    {
        lastUsed.TryGetValue(left, out long leftVersion);
        lastUsed.TryGetValue(right, out long rightVersion);
        int comparison = rightVersion.CompareTo(leftVersion);
        return comparison != 0 ? comparison : ((int)left).CompareTo((int)right);
    }

    public T GetObject<T>(PoolType key, Vector3 position) where T : Component
    {
        return pools.TryGetValue(key, out Pool pool)
            ? pool.GetObject<T>(position, Quaternion.identity) : null;
    }

    public void ReturnObject(Enum key, GameObject go)
    {
        if (go == null) return;
        if (key is PoolType type && pools.TryGetValue(type, out Pool pool)) pool.ReturnObject(go);
        else Destroy(go);
    }
}
