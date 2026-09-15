using UnityEngine;

namespace PixelRestaurant.Gacha
{
    public class GachaTestGold : MonoBehaviour
    {
        [Header("테스트용 골드")]
        [SerializeField] private BigNumber testGold = new BigNumber(1000);

        public void AddTestGold()
        {
            if (CurrencyManager.instance == null)
            {
                Debug.LogError("[가챠 테스트] CurrencyManager를 찾을 수 없습니다.");
                return;
            }

            CurrencyManager.instance.AddGold(testGold);

            Debug.Log($"[가챠 테스트] 골드 {testGold} 추가 완료");
        }

        public void ResetGold()
        {
            if (CurrencyManager.instance == null)
            {
                Debug.LogError("[가챠 테스트] CurrencyManager를 찾을 수 없습니다.");
                return;
            }

            BigNumber currentGold =
                CurrencyManager.instance.GetCurrentGold();

            if (currentGold > new BigNumber(0))
            {
                CurrencyManager.instance.SpendGold(currentGold);
            }

            Debug.Log("[가챠 테스트] 골드 초기화 완료");
        }
    }
}