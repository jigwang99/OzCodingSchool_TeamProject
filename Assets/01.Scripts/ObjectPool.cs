using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 10;
    public bool expandIfEmpty = true;

    private Queue<GameObject> queue = new Queue<GameObject>();
    private Transform root;

    void Awake()
    {
        root = new GameObject(prefab.name + "_PoolRoot").transform;
        root.SetParent(transform, false);

        for (int i = 0; i < initialSize; i++)
        {
            var go = CreateNewInstance();
            Release(go);
        }
    }

    GameObject CreateNewInstance()
    {
        var go = Instantiate(prefab, root);
        go.SetActive(false);
        var po = go.GetComponent<GachaPooled>();
        if (po == null) po = go.AddComponent<GachaPooled>();
        po.ownerPool = this;
        return go;
    }

    public GameObject Get()
    {
        GameObject go = null;
        if (queue.Count > 0)
        {
            go = queue.Dequeue();
        }
        else if (expandIfEmpty)
        {
            go = CreateNewInstance();
        }

        if (go != null)
        {
            go.SetActive(true);
            go.transform.SetParent(null); // 사용자가 위치/부모 지정하도록 분리
        }

        return go;
    }

    public void Release(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        go.transform.SetParent(root, false);
        queue.Enqueue(go);
    }
}