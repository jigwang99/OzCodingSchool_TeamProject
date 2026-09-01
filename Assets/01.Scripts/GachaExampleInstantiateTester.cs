using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaExampleInstantiateTester : MonoBehaviour
{
    [Tooltip("Inspector에서 직접 GachaPool 에셋을 드래그하세요.")]
    public GachaPool pool;

    [Tooltip("뽑기 횟수 (예: 1000)")]
    public int testCount = 1000;

    [Tooltip("한 프레임에 처리할 뽑기 수 (0이면 모두 한 프레임에 처리)")]
    public int perFrameBatch = 100;

    [Tooltip("한 줄에 몇 개 배치할지")]
    public int itemsPerRow = 10;

    [Tooltip("생성된 프리팹들의 부모(없으면 씬 루트에 생성)")]
    public Transform spawnParent;

    [Tooltip("프리팹 배치 시작 위치")]
    public Vector3 spawnStart = Vector3.zero;

    [Tooltip("프리팹 간격 (x = 열 간격, y = 줄 간격)")]
    public Vector3 spawnSpacing = new Vector3(1.0f, 1.0f, 0f);

    [Tooltip("라벨 Y 오프셋(프리팹 높이 위에 표시)")]
    public float labelOffset = 0.2f;

    [Tooltip("시드 고정(재현성 테스트 원하면 체크)")]
    public bool useFixedSeed = true;

    [Tooltip("고정 시드 값")]
    public int seed = 42;

    void Start()
    {
        if (pool == null)
        {
            Debug.LogWarning("GachaExampleInstantiateTester: pool is null in inspector");
            return;
        }

        if (useFixedSeed) UnityEngine.Random.InitState(seed);

        // perFrameBatch > 0 일 때 코루틴으로 분할 실행
        if (perFrameBatch > 0)
            StartCoroutine(RunAndInstantiateCoroutine());
        else
            RunAndInstantiateImmediate();
    }

    IEnumerator RunAndInstantiateCoroutine()
    {
        var counts = InitCounts();
        int instantiated = 0;
        int columns = Mathf.Max(1, itemsPerRow); // 한 줄에 배치할 개수 (기본 10)

        for (int i = 0; i < testCount; i++)
        {
            var result = GachaManager.Instance.DrawFromPool(pool);
            if (result != null)
            {
                counts[result.rarity]++;

                if (result.prefab != null)
                {
                    int col = instantiated % columns;
                    int row = instantiated / columns;
                    // 줄마다 위로 쌓이도록 Y축에 row * spawnSpacing.y 적용
                    Vector3 pos = spawnStart + new Vector3(col * spawnSpacing.x,
                                                           row * spawnSpacing.y,
                                                           0f);
                    var go = Instantiate(result.prefab, pos, Quaternion.identity, spawnParent);
                    // 라벨 추가
                    CreateRarityLabel(go, result.rarity);
                    instantiated++;
                }
            }

            // perFrameBatch마다 프레임 양보
            if (perFrameBatch > 0 && i % perFrameBatch == perFrameBatch - 1)
                yield return null;
        }

        LogResults(counts, testCount);
    }

    void RunAndInstantiateImmediate()
    {
        var counts = InitCounts();
        int instantiated = 0;
        int columns = Mathf.Max(1, itemsPerRow);

        for (int i = 0; i < testCount; i++)
        {
            var result = GachaManager.Instance.DrawFromPool(pool);
            if (result != null)
            {
                counts[result.rarity]++;

                if (result.prefab != null)
                {
                    int col = instantiated % columns;
                    int row = instantiated / columns;
                    Vector3 pos = spawnStart + new Vector3(col * spawnSpacing.x,
                                                           row * spawnSpacing.y,
                                                           0f);
                    var go = Instantiate(result.prefab, pos, Quaternion.identity, spawnParent);
                    CreateRarityLabel(go, result.rarity);
                    instantiated++;
                }
            }
        }

        LogResults(counts, testCount);
    }

    Dictionary<Rarity, int> InitCounts()
    {
        var dict = new Dictionary<Rarity, int>();
        foreach (Rarity r in System.Enum.GetValues(typeof(Rarity)))
            dict[r] = 0;
        return dict;
    }

    void LogResults(Dictionary<Rarity, int> counts, int total)
    {
        Debug.Log($"===== Gacha Test Results ({total} draws) =====");
        foreach (var kv in counts)
        {
            float pct = (float)kv.Value / total * 100f;
            Debug.Log($"{kv.Key} : {kv.Value}회 / {pct:0.00}%");
        }
    }

    // ---------------------------------------
    // 라벨 생성 유틸
    // ---------------------------------------
    void CreateRarityLabel(GameObject target, Rarity rarity)
    {
        if (target == null) return;

        // 바운드로 높이 계산
        float height = 0.0f;
        var rends = target.GetComponentsInChildren<Renderer>();
        if (rends != null && rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            height = b.size.y;
            // bounds는 월드 공간이므로, 대략적인 오프셋으로 사용
        }
        if (height <= 0f) height = 1.0f;

        // 라벨용 GameObject 생성
        var labelGO = new GameObject("RarityLabel");
        labelGO.transform.SetParent(target.transform, false);
        // 로컬 포지션: prefab 위에 약간 띄워서 표시
        labelGO.transform.localPosition = new Vector3(0f, height + labelOffset, 0f);
        // TextMesh 추가 (간단한 3D 텍스트)
        var tm = labelGO.AddComponent<TextMesh>();
        tm.text = rarity.ToString();
        tm.fontSize = 48;
        tm.characterSize = 0.02f;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = GetColorForRarity(rarity);
        // 옵션: 회전이 카메라를 향하도록 하려면 Billboard 스크립트 추가 가능
    }

    Color GetColorForRarity(Rarity r)
    {
        switch (r)
        {
            case Rarity.Common: return Color.white;
            case Rarity.Rare: return Color.cyan;
            case Rarity.Unique: return new Color(0.6f, 0.2f, 1f); // 보라
            case Rarity.Epic: return Color.yellow;
            default: return Color.white;
        }
    }
}