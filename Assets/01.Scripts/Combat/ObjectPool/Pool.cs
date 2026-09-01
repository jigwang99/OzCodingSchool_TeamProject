using UnityEngine;
using System.Collections.Generic;

public class Pool
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private Transform parent;

    public Pool(GameObject prefab, Transform parent, int size)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < size; i++)
        {
            Create();
        }
    }

    private GameObject Create()
    {
        GameObject go = UnityEngine.Object.Instantiate(prefab, parent);
        go.SetActive(false);
        pool.Enqueue(go);
        return go;
    }

    public T GetObject<T>() where T : Component
    {
        if (pool.Count == 0)
            Create();

        GameObject go = pool.Dequeue();
        go.SetActive(true);

        // 꺼낼 때 초기화 훅
        if (go.TryGetComponent(out IPoolable poolable))
            poolable.Init();

        return go.GetComponent<T>();
    }

    public void ReturnObject(GameObject go)
    {
        // 반납 전 정리 훅
        if (go.TryGetComponent(out IPoolable poolable))
            poolable.ReturnToPool();

        go.SetActive(false);
        pool.Enqueue(go);
    }
}