using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StageData
{
    [Header("Info")]
    [SerializeField] private string stageName;

    [Header("Enemy Placement (스폰 기준점 기준 오프셋, 길이 = 적 수)")]
    [SerializeField] private Vector2[] spawnOffsets;

    [Header("Enemy Stats (절대값)")]
    [SerializeField] private float enemyMaxHp;
    [SerializeField] private float enemyDamage;

    [Header("Reward")]
    [SerializeField] private StageDropTable dropTable;

    public string StageName => stageName;
    public Vector2[] SpawnOffsets => spawnOffsets;
    public int EnemyCount => spawnOffsets != null ? spawnOffsets.Length : 0;
    public float EnemyMaxHp => enemyMaxHp;
    public float EnemyDamage => enemyDamage;
    public StageDropTable DropTable => dropTable;

    public StageData(string stageName, Vector2[] spawnOffsets, float enemyMaxHp,
        float enemyDamage, StageDropTable dropTable)
    {
        this.stageName = stageName;
        this.spawnOffsets = spawnOffsets;
        this.enemyMaxHp = enemyMaxHp;
        this.enemyDamage = enemyDamage;
        this.dropTable = dropTable;
    }

    public StageData Clone()
    {
        // spawnOffsets는 배열(참조형)이라 얕은 복사면 원본과 배열을 공유한다.
        // 복제본에서 위치를 바꿔도 원본이 안전하도록 배열은 새로 만들어 복사.
        Vector2[] clonedOffsets = spawnOffsets != null
            ? (Vector2[])spawnOffsets.Clone()
            : Array.Empty<Vector2>();

        // dropTable은 SO 참조 → 공유가 정상(에셋을 공용으로 참조). 복제하지 않는다.
        return new StageData(stageName, clonedOffsets, enemyMaxHp, enemyDamage, dropTable);
    }
}
[CreateAssetMenu(fileName = "StageDataList", menuName = "Combat/Stage Data List")]
public class StageDataList : ScriptableObject
{
    [SerializeField] private List<StageData> stageList = new List<StageData>();

    public int Count => stageList != null ? stageList.Count : 0;

    // stageNumber: 1부터 시작 (currentStage와 동일 규약).
    // 범위를 벗어나면 마지막 스테이지로 고정 (= 최상위 스테이지 무한 반복 규칙 유지).
    // 원본 오염 방지를 위해 항상 Clone을 반환.
    public StageData GetClone(int stageNumber)
    {
        if (stageList == null || stageList.Count == 0)
            return null;

        int index = Mathf.Clamp(stageNumber - 1, 0, stageList.Count - 1);
        StageData source = stageList[index];
        return source != null ? source.Clone() : null;
    }
}