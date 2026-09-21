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
                return;
            }

            CurrencyManager.instance.AddGold(testGold);

        }

        public void ResetGold()
        {
            if (CurrencyManager.instance == null)
            {
                return;
            }

            BigNumber currentGold =
                CurrencyManager.instance.GetCurrentGold();

            if (currentGold > new BigNumber(0))
            {
                CurrencyManager.instance.SpendGold(currentGold);
            }

        }
    }
}