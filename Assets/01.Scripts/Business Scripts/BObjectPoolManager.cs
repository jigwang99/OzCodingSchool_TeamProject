using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class BObjectPoolManager : MonoBehaviour
{
    public static BObjectPoolManager instance;

    [SerializeField] private List<GameObject> objList = new();
    private Dictionary<string, Queue<GameObject>> pools = new();  
    
    List<GameObject> activeObjects = new(); //현재 나오는 유닛 받아오기 (게임 종료할때 풀에 다 꺼서 리턴하기위해서)

    int poolSize;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        poolSize = 5;

        foreach (GameObject obj in objList)
        {
            pools[obj.name] = new Queue<GameObject>();
            GameObject parentPool = new GameObject($"{obj.name}_Pool");
            parentPool.transform.SetParent(this.transform);

            for (int i = 0; i < poolSize; i++)
            {
                GameObject go = Instantiate(obj, parentPool.transform);
                go.SetActive(false);
                pools[obj.name].Enqueue(go);
            }
        }
    }
    public void GameEnd()   //ToArray() 쓰는 이유는 foreach 돌면서 activeObjects.Remove()가 일어나면 리스트 변경 오류가 나기 때문.
    {
        foreach (GameObject obj in activeObjects.ToArray())
        {
            ReturnObject(obj.name.Split("(Clone)")[0], obj);
        }
    }

    public GameObject GetObject(string name)
    {
        if (!pools.ContainsKey(name))
        {
            return null;
        }


        if (pools[name].Count > 0)
        {
            GameObject go = pools[name].Dequeue();
            go.SetActive(true);
            activeObjects.Add(go);
            return go;
        }
        else
        {
            Transform parent = transform.Find($"{name}_Pool");
            GameObject go = Instantiate(objList.Find(obj => obj.name == name), parent);
            activeObjects.Add(go);
            return go;
        }
    }

    public void ReturnObject(string name, GameObject go)
    {
        if (!pools.ContainsKey(name))
        {
            Destroy(go);
            return;
        }
        go.SetActive(false);
        Transform parent = transform.Find($"{name}_Pool");
        go.transform.SetParent(parent);
        pools[name].Enqueue(go);

        activeObjects.Remove(go);
    }
}