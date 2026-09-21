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

    public string StageName => stageName;
    public Vector3Int ElevatedFloorCounts => new Vector3Int(
        Mathf.Clamp(elevatedFloorCounts.x, 0, 2), Mathf.Clamp(elevatedFloorCounts.y, 0, 2),
        Mathf.Clamp(elevatedFloorCounts.z, 0, 2));
    public Vector2[] SpawnOffsets => spawnOffsets;
    public int EnemyCount => spawnOffsets != null ? spawnOffsets.Length : 0;
    public float EnemyMaxHp => enemyMaxHp;
    public float EnemyDamage => enemyDamage;
    public StageDropTable DropTable => dropTable;

    public StageData(string stageName, Vector2[] spawnOffsets, float enemyMaxHp,
        float enemyDamage, StageDropTable dropTable, PoolType[] enemyTypes = null, Vector3Int? elevatedFloorCounts = null)
    {
        this.stageName = stageName;
        this.spawnOffsets = spawnOffsets;
        this.enemyMaxHp = enemyMaxHp;
        this.enemyDamage = enemyDamage;
        this.dropTable = dropTable;
        this.enemyTypes = enemyTypes;
        this.elevatedFloorCounts = elevatedFloorCounts ?? new Vector3Int(1, 0, 2);
    }

    public PoolType GetEnemyType(int spawnIndex)
    {
        return enemyTypes != null && spawnIndex >= 0 && spawnIndex < enemyTypes.Length
            ? enemyTypes[spawnIndex]
            : PoolType.Crab_0001;
    }

    public StageData Clone()
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
        return new StageData(stageName, clonedOffsets, enemyMaxHp, enemyDamage, dropTable, clonedTypes, elevatedFloorCounts);
    }
}
[CreateAssetMenu(fileName = "StageDataList", menuName = "Combat/Stage Data List")]
public class StageDataList : ScriptableObject
{
    [SerializeField] private List<StageData> stageList = new List<StageData>();

    public int Count => stageList != null ? stageList.Count : 0;

    // 표시용 조회는 원본의 문자열만 반환해 적 배치 배열을 복제하지 않는다.
    public string GetStageName(int stageNumber) => GetSource(stageNumber)?.StageName;

    public StageDropTable GetDropTable(int stageNumber) => GetSource(stageNumber)?.DropTable;

    // stageNumber: 1부터 시작 (currentStage와 동일 규약).
    // 범위를 벗어나면 첫/마지막 스테이지로 고정한다.
    // 원본 오염 방지를 위해 항상 Clone을 반환.
    public StageData GetClone(int stageNumber) => GetSource(stageNumber)?.Clone();

    private StageData GetSource(int stageNumber)
    {
        if (stageList == null || stageList.Count == 0)
            return null;

        int index = Mathf.Clamp(stageNumber - 1, 0, stageList.Count - 1);
        return stageList[index];
    }
}
