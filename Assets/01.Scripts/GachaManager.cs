using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }

    public List<GachaPool> pools = new List<GachaPool>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 디버그: 등록된 풀 목록 출력
        Debug.Log($"GachaManager Awake - pools count: {pools?.Count ?? 0}");
        if (pools != null)
        {
            for (int i = 0; i < pools.Count; i++)
            {
                var p = pools[i];
                Debug.Log($"Pool[{i}] name={p?.name ?? "null"} poolId={(p != null ? p.poolId : "null")}");
            }
        }
    }

    public GachaResult DrawFromPool(string poolId)
    {
        var pool = pools.Find(p => p != null && p.poolId == poolId);
        if (pool == null)
        {
            Debug.LogWarning($"GachaPool not found: {poolId}");
            return null;
        }

        var group = ProbabilityRandom.GetRandomByWeight(pool.groups, g => g.weight);
        if (group == null || group.entries == null || group.entries.Count == 0)
        {
            Debug.LogWarning($"Invalid group or empty entries in pool {poolId}");
            return null;
        }

        var entry = ProbabilityRandom.GetRandomByWeight(group.entries, e => e.weight);
        if (entry == null)
        {
            Debug.LogWarning($"Failed to pick entry in pool {poolId}");
            return null;
        }

        return new GachaResult
        {
            itemId = entry.id,
            prefab = entry.prefab,
            rarity = group.rarity,
            groupName = group.groupName
        };
    }

    public List<GachaResult> DrawMultiple(string poolId, int count)
    {
        List<GachaResult> results = new List<GachaResult>(count);
        for (int i = 0; i < count; i++)
        {
            var r = DrawFromPool(poolId);
            if (r != null) results.Add(r);
        }
        return results;
    }

    // (선택) 풀 직접 전달용 오버로드
    public GachaResult DrawFromPool(GachaPool pool)
    {
        if (pool == null)
        {
            Debug.LogWarning("DrawFromPool called with null pool");
            return null;
        }

        var group = ProbabilityRandom.GetRandomByWeight(pool.groups, g => g.weight);
        if (group == null || group.entries == null || group.entries.Count == 0)
        {
            Debug.LogWarning($"Invalid group or empty entries in pool {pool.poolId}");
            return null;
        }

        var entry = ProbabilityRandom.GetRandomByWeight(group.entries, e => e.weight);
        if (entry == null)
        {
            Debug.LogWarning($"Failed to pick entry in pool {pool.poolId}");
            return null;
        }

        return new GachaResult
        {
            itemId = entry.id,
            prefab = entry.prefab,
            rarity = group.rarity,
            groupName = group.groupName
        };
    }
}