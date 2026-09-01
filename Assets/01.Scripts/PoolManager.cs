using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolEntry
{
    public GameObject prefab;
    public ObjectPool poolPrefab; // 에디터에서 미리 만들어둔 ObjectPool 프리팹(또는 빈 GameObject에 ObjectPool 컴포넌트 설정)
    [HideInInspector] public ObjectPool runtimePool;
}

public class PoolManager : MonoBehaviour
{
    public List<PoolEntry> pools = new List<PoolEntry>();

    // 싱글톤(원하면 사용). 씬에 1개 배치
    public static PoolManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        // 런타임 풀 인스턴스 생성
        foreach (var entry in pools)
        {
            if (entry.prefab == null) continue;

            if (entry.poolPrefab != null)
            {
                // poolPrefab을 인스턴스화 해서 사용
                var poolGO = Instantiate(entry.poolPrefab.gameObject, transform);
                entry.runtimePool = poolGO.GetComponent<ObjectPool>();
                entry.runtimePool.prefab = entry.prefab;
            }
            else
            {
                // 빈 GameObject를 만들어 ObjectPool 추가
                var go = new GameObject(entry.prefab.name + "_ObjectPool");
                go.transform.SetParent(transform, false);
                var pool = go.AddComponent<ObjectPool>();
                pool.prefab = entry.prefab;
                entry.runtimePool = pool;
            }
        }
    }

    public GameObject GetFromPool(GameObject prefab)
    {
        if (prefab == null) return null;

        foreach (var entry in pools)
        {
            if (entry.prefab == prefab && entry.runtimePool != null)
            {
                return entry.runtimePool.Get();
            }
        }

        // 등록되지 않은 prefab이면 임시로 풀 생성(선택적)
        Debug.LogWarning($"PoolManager: prefab not registered, instantiating directly: {prefab.name}");
        return Instantiate(prefab);
    }

    public void ReleaseToPool(GameObject instance)
    {
        if (instance == null) return;

        var po = instance.GetComponent<GachaPooled>();
        if (po != null && po.ownerPool != null)
        {
            po.ownerPool.Release(instance);
            return;
        }

        // owner 정보가 없으면 바로 Destroy (혹은 비활성화)
        Destroy(instance);
    }
}