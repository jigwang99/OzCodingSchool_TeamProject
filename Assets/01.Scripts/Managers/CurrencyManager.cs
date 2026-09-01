using UnityEngine;

public class CurrencyManager : Singleton<CurrencyManager>
{
    //°ñµå È¹µæ
    public void AddGold(int amount)
    {
        GameManager.instance.PlayerData.gold += amount;
        Debug.Log($"[CurrencyManager] °ñµå È¹µæ: +{amount} / ÇöÀç °ñµå: {GameManager.instance.PlayerData.gold}");
    }

    //°ñµå ¼Òºñ
    public bool SpendGold(int amount)
    {
        PlayerData data = GameManager.instance.PlayerData;
        if (data.gold >= amount)
        {
            data.gold -= amount;
            Debug.Log($"[CurrencyManager] °ñµå ¼Òºñ: -{amount} / ÀÜ¿© °ñµå: {data.gold}");
            return true;
        }

        Debug.Log("[CurrencyManager] °ñµå°¡ ºÎÁ·ÇÕ´Ï´Ù!");
        return false;
    }

    //Ä¿¸Õ ¹°°í±â È¹µæ
    public void AddCommonFish(int amount)
    {
        GameManager.instance.PlayerData.commonFish += amount;
        Debug.Log($"[CurrencyManager] Ä¿¸Õ ¹°°í±â È¹µæ: +{amount} / ÇöÀç ¹°°í±â: {GameManager.instance.PlayerData.commonFish}");
    }
    
    //Ä¿¸Õ ¹°°í±â ¼Òºñ
    public bool SpendCommonFish(int amount)
    {
        PlayerData data = GameManager.instance.PlayerData;
        if (data.commonFish >= amount)
        {
            data.commonFish -= amount;
            Debug.Log($"[CurrencyManager] Ä¿¸Õ ¹°°í±â ¼Òºñ: -{amount} / ÀÜ¿© ¹°°í±â: {data.commonFish}");
            return true;
        }

        Debug.Log("[CurrencyManager] Ä¿¸Õ ¹°°í±â°¡ ºÎÁ·ÇÕ´Ï´Ù!");
        return false;
    }
}
