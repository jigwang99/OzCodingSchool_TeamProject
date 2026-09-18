
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠 결과를 UI 카드로 표시한다.
    ///
    /// 역할:
    /// 1. ObjectPool에서 결과 카드 가져오기
    /// 2. 결과 위치 설정
    /// 3. 아이템 정보 전달
    /// 4. 카드 반환
    ///
    /// 주의:
    /// 인벤토리에 아이템을 추가하지 않는다.
    /// 인벤토리 추가는 GachaUIController에서 한 번만 담당한다.
    /// </summary>
    public class GachaResultHandler : MonoBehaviour
    {
        private static GachaResultHandler _instance;

        private GachaObjectPool _objectPool;

        public static GachaResultHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance =
                        FindObjectOfType<GachaResultHandler>();

                    if (_instance == null)
                    {
                        
                    }
                }

                return _instance;
            }
        }

        // =========================================================
        // 초기화
        // =========================================================

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            DontDestroyOnLoad(gameObject);

            _objectPool =
                GetComponent<GachaObjectPool>();

            if (_objectPool == null)
            {
               
            }

            
        }

        // =========================================================
        // 결과 처리
        // =========================================================

        /// <summary>
        /// 가챠 결과를 카드로 표시한다.
        /// </summary>
        /// <param name="item">뽑은 아이템</param>
        /// <param name="parent">카드를 생성할 부모</param>
        /// <returns>생성된 결과 카드</returns>
        public GameObject HandleGachaResult(
            GachaItem item,
            Transform parent)
        {
            // -----------------------------------------------------
            // 기본 검증
            // -----------------------------------------------------

            if (item == null)
            {
               

                return null;
            }

            if (_objectPool == null)
            {
               

                return null;
            }

            if (parent == null)
            {
              

                return null;
            }

            // -----------------------------------------------------
            // ObjectPool에서 카드 가져오기
            // -----------------------------------------------------

            GameObject resultCard =
                _objectPool.GetObject();

            if (resultCard == null)
            {
                return null;
            }

            // -----------------------------------------------------
            // 결과 위치 설정
            // -----------------------------------------------------

            RectTransform cardRect =
                resultCard.GetComponent<RectTransform>();

            resultCard.transform.SetParent(
                parent,
                false
            );

            // UI 카드라면 anchoredPosition을 사용하는 것이 안전하다.
            if (cardRect != null)
            {
                cardRect.anchoredPosition =
                    Vector2.zero;

                cardRect.localRotation =
                    Quaternion.identity;

                cardRect.localScale =
                    Vector3.one;
            }
            else
            {
                resultCard.transform.localPosition =
                    Vector3.zero;

                resultCard.transform.localRotation =
                    Quaternion.identity;

                resultCard.transform.localScale =
                    Vector3.one;
            }

            resultCard.name =
                $"{item.ItemName}_ResultCard";

            // -----------------------------------------------------
            // 카드에 아이템 정보 전달
            // -----------------------------------------------------

            GachaResultItemDisplay itemDisplay =
                resultCard.GetComponent<GachaResultItemDisplay>();

            if (itemDisplay == null)
            {
            

                // 잘못된 카드이므로 풀에 반환
                _objectPool.ReturnObject(resultCard);

                return null;
            }

            // -----------------------------------------------------
            // NEW 여부 확인
            // -----------------------------------------------------

            GachaInventory inventory =
                GachaInventory.Instance;

            bool isNew = false;

            if (inventory != null)
            {
                isNew =
                    inventory.IsNewItem(
                        item.ItemId
                    );
            }

            itemDisplay.SetItemInfo(
                item,
                isNew
            );

            // -----------------------------------------------------
            // 카드 공개 연출 설정
            // -----------------------------------------------------

            GachaRevealCard revealCard =
                resultCard.GetComponent<GachaRevealCard>();

            if (revealCard != null)
            {
                revealCard.Setup(
                    item,
                    null
                );
            }

            // -----------------------------------------------------
            // 카드 활성화
            // -----------------------------------------------------

            resultCard.SetActive(true);

            // -----------------------------------------------------
            // 카드 활성화
            // -----------------------------------------------------

            resultCard.SetActive(true);

          
            

            // -----------------------------------------------------
            // 생성된 카드를 UIController에게 반환
            // -----------------------------------------------------

            return resultCard;
        }

        // =========================================================
        // ObjectPool 설정
        // =========================================================

        public void SetObjectPool(
            GachaObjectPool pool)
        {
            _objectPool = pool;

          
        }
    }
}

