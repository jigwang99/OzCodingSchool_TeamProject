using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 뽑기 결과를 UI 카드로 표시
    /// 오브젝트 풀에서 프리팹 꺼내 결과 표시
    /// 데이터 판단(NEW 여부)과 표시를 분리
    /// </summary>
    public class GachaResultHandler : MonoBehaviour
    {
        private static GachaResultHandler _instance;

        private GachaObjectPool _objectPool;
        private GachaInventory _inventory;

        public static GachaResultHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GachaResultHandler>();
                    if (_instance == null)
                    {
                        Debug.LogError("[결과 처리] GachaResultHandler를 찾을 수 없습니다!");
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 의존성 초기화
            _objectPool = GetComponent<GachaObjectPool>();
            _inventory = GachaInventory.Instance;

            if (_objectPool == null)
            {
                Debug.LogError("[결과 처리] GachaObjectPool을 찾을 수 없습니다!");
            }

            Debug.Log("[결과 처리] 초기화 완료");
        }

        /// <summary>
        /// 뽑기 결과 처리 (오브젝트 풀 사용)
        /// </summary>
        /// <param name="item">뽑은 아이템</param>
        /// <param name="parent">결과를 표시할 부모 Transform</param>
        public void HandleGachaResult(GachaItem item, Transform parent)
        {
            if (item == null)
            {
                Debug.LogError("[결과 처리] 아이템이 null입니다.");
                return;
            }

            if (_objectPool == null)
            {
                Debug.LogError("[결과 처리] 오브젝트 풀이 없습니다.");
                return;
            }

            if (parent == null)
            {
                Debug.LogError("[결과 처리] parent가 null입니다.");
                return;
            }

            // Step 1: 오브젝트 풀에서 카드 프리팹 꺼내기
            GameObject resultCard = _objectPool.GetObject();
            if (resultCard == null)
            {
                Debug.LogError("[결과 처리] 오브젝트 풀에서 객체를 가져올 수 없습니다.");
                return;
            }

            // Step 2: 부모 설정
            resultCard.transform.SetParent(parent);
            resultCard.transform.localPosition = Vector3.zero;
            resultCard.name = $"{item.ItemName}(Pool)";

            // Step 3: 카드에 아이템 정보 전달
            GachaResultItemDisplay itemDisplay = resultCard.GetComponent<GachaResultItemDisplay>();
            if (itemDisplay == null)
            {
                Debug.LogWarning($"[결과 처리] {resultCard.name}에 GachaResultItemDisplay 컴포넌트가 없습니다.");
                return;
            }

            // ⭐ NEW 판단은 여기서! (데이터와 표시 분리)
            bool isNew = _inventory.IsNewItem(item.ItemId);
            itemDisplay.SetItemInfo(item, isNew);

            // Step 4: 인벤토리에 아이템 추가
            _inventory.AddItem(item.ItemId, 1);

            // Step 5: 결과 카드 활성화 (이미 풀에서 나올 때 활성화됨)
            resultCard.SetActive(true);

            Debug.Log($"[결과 처리] {item.GetDisplayName()} 결과 표시 (NEW: {isNew})");
        }

        /// <summary>
        /// 오브젝트 풀 설정 (직접 할당 안 할 경우용)
        /// </summary>
        /// <param name="pool">오브젝트 풀</param>
        public void SetObjectPool(GachaObjectPool pool)
        {
            _objectPool = pool;
            Debug.Log("[결과 처리] 오브젝트 풀 설정됨");
        }

        /// <summary>
        /// 인벤토리 설정
        /// </summary>
        /// <param name="inventory">인벤토리</param>
        public void SetInventory(GachaInventory inventory)
        {
            _inventory = inventory;
            Debug.Log("[결과 처리] 인벤토리 설정됨");
        }
    }
}
