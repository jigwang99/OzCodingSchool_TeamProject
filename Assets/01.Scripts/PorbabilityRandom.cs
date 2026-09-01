using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가중치 기반 추첨 유틸리티 (제너릭)
/// 사용법: ProbabilityRandom.GetRandomByWeight(list, x => x.weight);
/// </summary>
public static class ProbabilityRandom
{
    public static T GetRandomByWeight<T>(List<T> list, Func<T, float> weightSelector)
    {
        if (list == null || list.Count == 0) return default;

        float total = 0f;
        for (int i = 0; i < list.Count; i++)
        {
            total += Mathf.Max(0f, weightSelector(list[i]));
        }

        if (total <= 0f) return list[0];

        float rnd = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < list.Count; i++)
        {
            cumulative += Mathf.Max(0f, weightSelector(list[i]));
            if (rnd < cumulative)
            {
                return list[i];
            }
        }

        return list[list.Count - 1];
    }
}