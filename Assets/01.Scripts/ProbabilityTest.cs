using System.Collections.Generic;
using UnityEngine;

public class ProbabilityTest : MonoBehaviour
{
    public enum Rarity
    {
        Common,
        Rare,
        Unique,
        Epic
    }

    void Start()
    {
        List<ProbabilityItem<Rarity>> probabilities =
            new List<ProbabilityItem<Rarity>>
        {
        new ProbabilityItem<Rarity>
        {
            item = Rarity.Common,
            value = 60f
        },

        new ProbabilityItem<Rarity>
        {
            item = Rarity.Rare,
            value = 30f
        },

        new ProbabilityItem<Rarity>
        {
            item = Rarity.Unique,
            value = 9f
        },

        new ProbabilityItem<Rarity>
        {
            item = Rarity.Epic,
            value = 1f
        }
        };

        int common = 0;
        int rare = 0;
        int unique = 0;
        int epic = 0;

        int testCount = 10000;

        for (int i = 0; i < testCount; i++)
        {
            Rarity result =
                ProbabilityRandom.GetRandomEnum(probabilities);

            switch (result)
            {
                case Rarity.Common:
                    common++;
                    break;

                case Rarity.Rare:
                    rare++;
                    break;

                case Rarity.Unique:
                    unique++;
                    break;

                case Rarity.Epic:
                    epic++;
                    break;
            }
        }

        Debug.Log("===== °¡Ã­ Å×½ºÆ® °á°ú =====");
        Debug.Log("ÃÑ È½¼ö : " + testCount);

        Debug.Log(
            "Common : " + common +
            "È¸ / " + ((float)common / testCount * 100f) + "%"
        );

        Debug.Log(
            "Rare : " + rare +
            "È¸ / " + ((float)rare / testCount * 100f) + "%"
        );

        Debug.Log(
            "Unique : " + unique +
            "È¸ / " + ((float)unique / testCount * 100f) + "%"
        );

        Debug.Log(
            "Epic : " + epic +
            "È¸ / " + ((float)epic / testCount * 100f) + "%"
        );
    }
}