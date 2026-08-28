using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public enum Rarity
{
    Common,
    Rare,
    Unique,
    Epic
}
public class Gacha : MonoBehaviour
{
    void Start()
    {
        List<ProbabilityItem<Rarity>> probabilities = new List<ProbabilityItem<Rarity>>
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

        Rarity result = ProbabilityRandom.GetRandomEnum(probabilities);

        Debug.Log("ªÃ¿∫ µÓ±ﬁ : " + result);
    }
}