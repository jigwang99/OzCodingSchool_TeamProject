using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static bool _isQuitting;
    public static T instance
    {
        get
        {
            if (_isQuitting) return null;

            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();

                    Singleton<T> singleton = _instance as Singleton<T>;
                    if (singleton != null && singleton.isDontDestroy)
                    {
                        DontDestroyOnLoad(obj);
                    }
                }
            }
            else
            {
                Singleton<T> singleton = _instance as Singleton<T>;
                if (singleton != null && singleton.isDontDestroy)
                {
                    DontDestroyOnLoad(singleton.gameObject);
                }
            }

            return _instance;
        }
    }

    [SerializeField] protected bool isDontDestroy = false;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            if (isDontDestroy)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}
