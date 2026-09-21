using UnityEngine;

namespace PixelRestaurant.Gacha
{
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
                }

                return _instance;
            }
        }

        [Header("Card Spawn")]
        [SerializeField]
        private Vector2 spawnPosition = Vector2.zero;

        [Header("Card Layout")]
        [SerializeField]
        private int cardsPerRow = 5;

        [SerializeField]
        private Vector2 cardSpacing =
            new Vector2(120f, 160f);

        [SerializeField]
        private Vector2 layoutCenter = Vector2.zero;

        [Header("Inventory Target")]
        [SerializeField]
        private RectTransform inventoryTarget;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            DontDestroyOnLoad(gameObject);

            _objectPool = GetComponent<GachaObjectPool>();

            if (_objectPool == null)
            {
                Debug.LogError(
                    "GachaObjectPool is missing.",
                    this
                );
            }
        }

        // 기존 호출 방식 유지
        public GameObject HandleGachaResult(
            GachaItem item,
            Transform parent)
        {
            return HandleGachaResult(
                item,
                parent,
                0,
                1
            );
        }

        // 여러 장의 카드 배치 지원
        public GameObject HandleGachaResult(
            GachaItem item,
            Transform parent,
            int cardIndex,
            int totalCards)
        {
            if (item == null ||
                parent == null ||
                _objectPool == null)
            {
                return null;
            }

            RectTransform parentRect =
                parent as RectTransform;

            if (parentRect == null)
            {
                Debug.LogError(
                    "Result parent must be a RectTransform.",
                    this
                );

                return null;
            }

            GameObject resultCard =
                _objectPool.GetObject();

            if (resultCard == null)
                return null;

            RectTransform cardRect =
                resultCard.GetComponent<RectTransform>();

            GachaRevealCard revealCard =
                resultCard.GetComponent<GachaRevealCard>();

            if (cardRect == null || revealCard == null)
            {
                Debug.LogError(
                    "Result card is missing required components.",
                    resultCard
                );

                _objectPool.ReturnObject(resultCard);

                return null;
            }

            // 부모 설정
            cardRect.SetParent(parentRect, false);

            // 풀에서 재사용한 카드의 기본 Transform 복원
            cardRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            cardRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            cardRect.pivot =
                new Vector2(0.5f, 0.5f);

            cardRect.localScale = Vector3.one;
            cardRect.localRotation = Quaternion.identity;

            cardRect.anchoredPosition = spawnPosition;

            resultCard.name =
                $"{item.ItemName}_ResultCard";

            // 재사용 카드 활성화
            resultCard.SetActive(true);

            // 아이템 정보 및 연출 초기화
            revealCard.Setup(
                item,
                inventoryTarget
            );

            // 각 카드의 목표 위치 계산
            Vector2 targetPosition =
                CalculateCardPosition(
                    cardIndex,
                    totalCards
                );

            // Back 카드 등장 연출
            revealCard.PlaySpawnAnimation(
                spawnPosition,
                targetPosition
            );

            return resultCard;
        }

        // 카드별 목표 좌표 계산
        private Vector2 CalculateCardPosition(
            int index,
            int totalCards)
        {
            if (totalCards <= 1)
            {
                return layoutCenter;
            }

            int columns = Mathf.Max(1, cardsPerRow);

            int row = index / columns;
            int column = index % columns;

            int totalRows =
                Mathf.CeilToInt(
                    (float)totalCards / columns
                );

            // 마지막 행에 카드가 적어도 가운데 정렬
            int rowStart = row * columns;

            int cardsInRow = Mathf.Min(
                columns,
                totalCards - rowStart
            );

            float x =
                (column - (cardsInRow - 1) * 0.5f)
                * cardSpacing.x;

            float y =
                ((totalRows - 1) * 0.5f - row)
                * cardSpacing.y;

            return layoutCenter + new Vector2(x, y);
        }

        public void SetObjectPool(
            GachaObjectPool pool)
        {
            _objectPool = pool;
        }

        // 씬에 배치된 인벤토리 버튼을 연결할 때 사용
        public void SetInventoryTarget(
            RectTransform target)
        {
            inventoryTarget = target;
        }
    }
}