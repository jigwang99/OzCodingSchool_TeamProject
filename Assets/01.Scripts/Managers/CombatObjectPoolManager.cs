using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CombatObjectPoolManager : Singleton<CombatObjectPoolManager>
{
    [SerializeField] private List<GameObject> objList;
    private readonly Dictionary<PoolType, GameObject> prefabs = new();
    private readonly Dictionary<PoolType, Pool> pools = new();
    private readonly Dictionary<PoolType, int> required = new();
    private readonly List<PoolType> obsolete = new();

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
        if (!CanPrepare(data)) throw new InvalidOperationException("스테이지의 적 프리팹이 풀에 등록되지 않았습니다.");
        required.Clear();
        for (int i = 0; i < data.EnemyCount; i++)
        {
            PoolType type = data.GetEnemyType(i);
            required.TryGetValue(type, out int count);
            required[type] = count + 1;
        }
        obsolete.Clear();
        foreach (var pair in pools)
            if (!required.ContainsKey(pair.Key)) obsolete.Add(pair.Key);
        foreach (PoolType type in obsolete)
        {
            pools[type].Dispose();
            pools.Remove(type);
        }
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
            pool.Resize(pair.Value);
        }
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
