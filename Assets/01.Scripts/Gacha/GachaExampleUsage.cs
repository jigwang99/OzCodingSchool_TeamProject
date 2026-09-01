using UnityEngine;

public class GachaExampleUsage : MonoBehaviour
{
    public GachaPool pool; // inspector에서 직접 에셋을 드래그하세요

    void Start()
    {
        if (pool == null)
        {
            Debug.LogWarning("GachaExampleUsage: pool is null in inspector");
            return;
        }

        // 직접 풀 전달 오버로드 사용
        var result = GachaManager.Instance.DrawFromPool(pool);
        if (result != null)
        {
            Debug.Log($"뽑힌 아이템: {result.itemId}, 등급: {result.rarity}, 그룹: {result.groupName}");
        }

        var multi = new System.Collections.Generic.List<GachaResult>();
        for (int i = 0; i < 10; i++)
        {
            var r = GachaManager.Instance.DrawFromPool(pool);
            if (r != null) multi.Add(r);
        }
        Debug.Log($"10연속 결과 개수: {multi.Count}");
    }
}