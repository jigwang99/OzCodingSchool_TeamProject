using System;
using System.Collections.Generic;

[Serializable]
public class ProbabilityItem<T>
{
    public T item;
    public float value;
}

public class ProbabilityRandom
{
    public static T GetRandomEnum<T>(List<ProbabilityItem<T>> probabilities)
    {
        float total = 0f;

        foreach (var entry in probabilities)
        {
            total += entry.value;
        }

        float randomValue = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var entry in probabilities)
        {
            cumulative += entry.value;

            if (randomValue < cumulative)
            {
                return entry.item;
            }
        }

        return probabilities[0].item;
    }
}