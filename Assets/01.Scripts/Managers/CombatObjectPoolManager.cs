using UnityEngine;
using System.Collections.Generic;
using System;

public class CombatObjectPoolManager : Singleton<CombatObjectPoolManager>
{
    [SerializeField] private int poolSize;
    [SerializeField] private List<GameObject> objList;

    private Dictionary<Enum, Pool> pools = new Dictionary<Enum, Pool>();

    protected override void Awake()
    {
        base.Awake();

        // 중복으로 파괴되는 인스턴스면 초기화하지 않음
        if (instance != this)
        {
            return;
        }

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (GameObject go in objList)
        {
            IPoolable poolable = go.GetComponent<IPoolable>();
            if (poolable == null)
                continue;

            GameObject parentObject = new GameObject($"{go.name}_Pool");
            parentObject.transform.SetParent(transform);

            pools.Add(poolable.PoolKey, new Pool(go, parentObject.transform, poolSize));
        }
    }

    public T GetObject<T>(Enum key) where T : Component
    {
        if (!pools.TryGetValue(key, out Pool pool))
            return null;
        return pool.GetObject<T>();
    }

    public void ReturnObject(Enum key, GameObject go)
    {
        if (!pools.TryGetValue(key, out Pool pool))
        {
            Destroy(go);
            return;
        }
        pool.ReturnObject(go);
    }
}