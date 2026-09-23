using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageData
{
    [Header("Info")]
    [SerializeField] private string stageName;
    [Header("Map: 좌 / 중앙 / 우 공중 발판 수 (각 0~2)")]
    [SerializeField] private Vector3Int elevatedFloorCounts = new Vector3Int(1, 0, 2);

    [Header("Enemy Placement (스폰 기준점 기준 오프셋, 길이 = 적 수)")]
    [SerializeField] private Vector2[] spawnOffsets;

    [Tooltip("Spawn Offsets와 같은 순서로 15종의 적 외형을 지정합니다. 비어 있거나 부족한 항목은 Crab_0001입니다.")]
    [SerializeField] private PoolType[] enemyTypes;

    [Header("Enemy Stats (절대값)")]
    [SerializeField] private float enemyMaxHp;
    [SerializeField] private float enemyDamage;

    [Header("Reward")]
    [SerializeField] private StageDropTable dropTable;
    [Tooltip("보스 처치 시 현재 드롭 테이블에서 추가로 확정 지급할 물고기 수. 일반 스테이지는 0입니다.")]
    [SerializeField, Min(0)] private int bossGuaranteedFishCount;

    public string StageName => stageName;
    public Vector3Int ElevatedFloorCounts => new Vector3Int(
        Mathf.Clamp(elevatedFloorCounts.x, 0, 2), Mathf.Clamp(elevatedFloorCounts.y, 0, 2),
        Mathf.Clamp(elevatedFloorCounts.z, 0, 2));
    public Vector2[] SpawnOffsets => spawnOffsets;
    public int EnemyCount => spawnOffsets != null ? spawnOffsets.Length : 0;
    public float EnemyMaxHp => enemyMaxHp;
    public float EnemyDamage => enemyDamage;
    public StageDropTable DropTable => dropTable;
    public int BossGuaranteedFishCount => Mathf.Max(0, bossGuaranteedFishCount);
    public bool SpawnInCamera { get; private set; }

    public StageData(string stageName, Vector2[] spawnOffsets, float enemyMaxHp,
        float enemyDamage, StageDropTable dropTable, PoolType[] enemyTypes = null, Vector3Int? elevatedFloorCounts = null,
        int bossGuaranteedFishCount = 0, bool spawnInCamera = false)
    {
        this.stageName = stageName;
        SpawnInCamera = spawnInCamera;
        this.spawnOffsets = spawnOffsets;
        this.enemyMaxHp = enemyMaxHp;
        this.enemyDamage = enemyDamage;
        this.dropTable = dropTable;
        this.bossGuaranteedFishCount = Mathf.Max(0, bossGuaranteedFishCount);
        this.enemyTypes = enemyTypes;
        this.elevatedFloorCounts = elevatedFloorCounts ?? new Vector3Int(1, 0, 2);
    }

    public PoolType GetEnemyType(int spawnIndex)
    {
        return enemyTypes != null && spawnIndex >= 0 && spawnIndex < enemyTypes.Length
            ? enemyTypes[spawnIndex]
            : PoolType.Crab_0001;
    }

    public StageData Clone(bool spawnInCamera = false)
    {
        // spawnOffsets는 배열(참조형)이라 얕은 복사면 원본과 배열을 공유한다.
        // 복제본에서 위치를 바꿔도 원본이 안전하도록 배열은 새로 만들어 복사.
        Vector2[] clonedOffsets = spawnOffsets != null
            ? (Vector2[])spawnOffsets.Clone()
            : Array.Empty<Vector2>();

        // dropTable은 SO 참조 → 공유가 정상(에셋을 공용으로 참조). 복제하지 않는다.
        PoolType[] clonedTypes = enemyTypes != null
            ? (PoolType[])enemyTypes.Clone()
            : Array.Empty<PoolType>();
        return new StageData(stageName, clonedOffsets, enemyMaxHp, enemyDamage, dropTable, clonedTypes,
            elevatedFloorCounts, bossGuaranteedFishCount, SpawnInCamera || spawnInCamera);
    }
}
[CreateAssetMenu(fileName = "StageDataList", menuName = "Combat/Stage Data List")]
public class StageDataList : ScriptableObject
{
    [SerializeField] private List<StageData> stageList = new List<StageData>();
    [Header("최종 보스 이후 무한 스테이지")]
    [SerializeField, Min(0f)] private float endlessGrowthPercent = 10f;
    [Tooltip("무한 단계마다 최종 보스의 확정 물고기 보상에 추가되는 수량. 확률 드롭은 별도입니다.")]
    [SerializeField, Min(1)] private int endlessBonusFishPerStage = 1;

    public int Count => stageList != null ? stageList.Count : 0;

    public bool IsEndlessStage(int stageNumber) => Count > 0 && stageNumber > Count;
    public int GetEnvironmentStage(int stageNumber) => Mathf.Clamp(stageNumber, 1, Mathf.Max(1, Count));
    // 표시 이름 조회에서는 적 배치 배열을 생성하지 않는다.
    public string GetStageName(int stageNumber) => IsEndlessStage(stageNumber)
        ? $"무한 {stageNumber - Count}" : GetSource(stageNumber)?.StageName;

    public StageDropTable GetDropTable(int stageNumber) => GetSource(stageNumber)?.DropTable;

    // stageNumber: 1부터 시작 (currentStage와 동일 규약).
    // 일반 구간 이후에는 마지막 보스 데이터로 무한 단계를 생성한다.
    // 반환값의 배치 배열은 항상 원본과 분리한다.
    public StageData GetClone(int stageNumber)
    {
        StageData source = GetSource(stageNumber);
        if (source == null) return null;
        if (!IsEndlessStage(stageNumber))
            return source.Clone(stageNumber > 0 && stageNumber % 5 == 0 && source.EnemyCount == 1);
        int endlessLevel = stageNumber - Count;
        double multiplier = Math.Pow(1d + Mathf.Max(0f, endlessGrowthPercent) / 100d, endlessLevel);
        int guaranteedFish = (int)Math.Min(int.MaxValue, (long)source.BossGuaranteedFishCount
            + (long)endlessLevel * Mathf.Max(1, endlessBonusFishPerStage));
        // float 전투 모듈의 범위 안에서 유지해 아주 높은 단계도 Infinity/NaN으로 망가지지 않는다.
        float Scale(float value) => value <= 0f ? 0f : (float)Math.Min(float.MaxValue / 1024d, value * multiplier);
        return new StageData(GetStageName(stageNumber), new[] { Vector2.zero },
            Scale(source.EnemyMaxHp), Scale(source.EnemyDamage), source.DropTable,
            new[] { source.GetEnemyType(0) }, Vector3Int.zero, guaranteedFish, true);
    }

    private StageData GetSource(int stageNumber)
    {
        if (stageList == null || stageList.Count == 0)
            return null;

        int index = Mathf.Clamp(stageNumber - 1, 0, stageList.Count - 1);
        return stageList[index];
    }
}
