using System;
using UnityEngine;

namespace PixelRestaurant.Managers
{
    /// <summary>
    /// 골드 관리 시스템
    /// 게임 내 골드 추가/차감/조회
    /// </summary>
    public class GoldManager : MonoBehaviour
    {
        public static GoldManager instance { get; private set; }

        [SerializeField] private int currentGold = 0;

        /// <summary>
        /// 골드가 변경될 때 현재 잔액을 전달합니다.
        /// </summary>
        public event Action<int> OnGoldChanged;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 골드 추가
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("음수는 추가할 수 없습니다.");
                return;
            }

            currentGold += amount;
            Debug.Log($"[골드 추가] +{amount} (현재: {currentGold})");
            NotifyGoldChanged();
        }

        /// <summary>
        /// 골드 차감 (성공 시 true, 실패 시 false)
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("음수는 차감할 수 없습니다.");
                return false;
            }

            if (currentGold < amount)
            {
                Debug.Log($"[골드 부족] 필요: {amount}, 보유: {currentGold}");
                return false;
            }

            currentGold -= amount;
            Debug.Log($"[골드 차감] -{amount} (현재: {currentGold})");
            NotifyGoldChanged();
            return true;
        }

        /// <summary>
        /// 현재 골드 반환
        /// </summary>
        public int GetCurrentGold()
        {
            return currentGold;
        }

        /// <summary>
        /// 골드 직접 설정 (테스트용)
        /// </summary>
        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            Debug.Log($"[골드 설정] {currentGold}");
            NotifyGoldChanged();
        }

        private void NotifyGoldChanged()
        {
            OnGoldChanged?.Invoke(currentGold);
        }
    }
}
