using PixelRestaurant.Gacha;
using UnityEngine;

namespace PixelRestaurant.Managers
{
    /// <summary>
    /// MVP 단계의 임시 재화 관리
    /// ICurrencyProvider 구현
    /// 나중에 CurrencyManager로 교체 가능
    /// </summary>
    public class CurrencyManager : MonoBehaviour, ICurrencyProvider
    {
        private static CurrencyManager _instance;

        [SerializeField]
        private int _currentGold = 1000;  // 테스트용 초기값

        public static CurrencyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CurrencyManager>();
                    if (_instance == null)
                    {
                        Debug.LogError("[골드 관리자] GoldManager를 찾을 수 없습니다!");
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

            Debug.Log($"[골드 관리자] 초기화 완료 (초기 골드: {_currentGold})");
        }

        /// <summary>
        /// 골드 소비 (뽑기 등)
        /// </summary>
        /// <param name="amount">소비할 골드 양</param>
        /// <returns>성공 여부</returns>
        public bool SpendGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[골드] 음수는 사용할 수 없습니다: {amount}");
                return false;
            }

            if (_currentGold < amount)
            {
                Debug.Log($"[골드] 부족 (필요: {amount}, 보유: {_currentGold})");
                return false;
            }

            _currentGold -= amount;
            Debug.Log($"[골드] 소비 -{amount} (현재: {_currentGold})");
            return true;
        }

        /// <summary>
        /// 골드 획득 (경영 등)
        /// </summary>
        /// <param name="amount">획득할 골드 양</param>
        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[골드] 음수는 추가할 수 없습니다: {amount}");
                return;
            }

            _currentGold += amount;
            Debug.Log($"[골드] 획득 +{amount} (현재: {_currentGold})");
        }

        /// <summary>
        /// 아이템 획득 (MVP: 로그만)
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수</param>
        public void AddItem(string itemId, int count)
        {
            Debug.Log($"[아이템] {itemId} x{count} 획득");
        }

        /// <summary>
        /// 현재 골드 조회
        /// </summary>
        /// <returns>현재 골드</returns>
        public int GetCurrentGold()
        {
            return _currentGold;
        }

        /// <summary>
        /// 골드 직접 설정 (테스트용)
        /// </summary>
        /// <param name="amount">설정할 골드</param>
        public void SetGold(int amount)
        {
            _currentGold = Mathf.Max(0, amount);
            Debug.Log($"[골드] 설정됨: {_currentGold}");
        }

        /// <summary>
        /// 모든 골드 제거 (테스트용)
        /// </summary>
        public void ClearGold()
        {
            _currentGold = 0;
            Debug.Log("[골드] 모두 제거됨");
        }
    }
}
