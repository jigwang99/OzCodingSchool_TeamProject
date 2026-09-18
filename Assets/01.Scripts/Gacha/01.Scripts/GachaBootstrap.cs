using PixelRestaurant.Data;
using PixelRestaurant.Gacha;
using System.Collections.Generic;
using UnityEngine;

public class GachaBootstrap : MonoBehaviour
{
    [SerializeField]
    private List<GachaPoolData> poolAssets =
        new List<GachaPoolData>();

    private void Start()
    {
        // Build dictionary
        var pools =
            new Dictionary<GachaGroup, GachaPoolData>();

        foreach (var p in poolAssets)
        {
            if (p != null)
                pools[p.Group] = p;
        }

        // Find instances
        var pitySystem =
            GachaPitySystem.Instance;

        var inventory =
            GachaInventory.Instance;

        // 공용 CurrencyManager 사용
        ICurrencyProvider currency =
            CurrencyManager.instance;

        if (currency == null)
        {
            

            return;
        }

        GachaManager.Instance.Initialize(
            pools,
            pitySystem,
            inventory,
            currency
        );

       
    }
}