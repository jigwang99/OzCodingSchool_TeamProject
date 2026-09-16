

namespace PixelRestaurant.Gacha
{
    public class GachaCurrencyAdapter : ICurrencyProvider
    {
        public bool SpendGold(BigNumber amount)
        {
            return CurrencyManager.instance.SpendGold(amount);
        }

        public void AddGold(BigNumber amount)
        {
            CurrencyManager.instance.AddGold(amount);
        }

        public BigNumber GetCurrentGold()
        {
            return CurrencyManager.instance.GetCurrentGold();
        }
    }
}