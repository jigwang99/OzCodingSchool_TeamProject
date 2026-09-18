using System.Collections.Generic;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠 오브젝트 풀 시스템
    /// 가챠 결과 UI 프리팹을 미리 생성해두고 재사용하여 메모리 효율 증대
    /// </summary>
    public class GachaObjectPool : MonoBehaviour
    {
        [SerializeField]
        private string poolName = "GachaObjectPool";

        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private int initialPoolSize = 10;

        [SerializeField]
        private Transform poolParent;

        private Queue<GameObject> _availableObjects = new Queue<GameObject>();
        private List<GameObject> _allObjects = new List<GameObject>();

        public string PoolName => poolName;

        private void Awake()
        {
            InitializePool();
        }

        /// <summary>
        /// 풀 초기화
        /// </summary>
        private void InitializePool()
        {
            if (prefab == null)
            {
                return;
            }

            if (poolParent == null)
                poolParent = transform;

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewObject();
            }

        }

        /// <summary>
        /// 새로운 객체 생성 및 풀에 추가
        /// </summary>
        private void CreateNewObject()
        {
            GameObject obj = Instantiate(prefab, poolParent);
            obj.SetActive(false);
            _availableObjects.Enqueue(obj);
            _allObjects.Add(obj);
        }

        /// <summary>
        /// 풀에서 객체 꺼내기
        /// 풀이 부족하면 자동 생성
        /// </summary>
        /// <returns>재사용할 GameObject</returns>
        public GameObject GetObject()
        {
            GameObject obj;

            if (_availableObjects.Count > 0)
            {
                obj = _availableObjects.Dequeue();
            }
            else
            {
                // 풀이 부족하면 새로 생성
                CreateNewObject();
                obj = _availableObjects.Dequeue();
            }

            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// 객체를 풀에 반환
        /// </summary>
        /// <param name="obj">반환할 객체</param>
        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(poolParent);
            _availableObjects.Enqueue(obj);
        }

        /// <summary>
        /// 풀의 모든 객체 초기화
        /// </summary>
        public void ClearPool()
        {
            foreach (var obj in _allObjects)
            {
                Destroy(obj);
            }

            _availableObjects.Clear();
            _allObjects.Clear();
        }

    }
}
