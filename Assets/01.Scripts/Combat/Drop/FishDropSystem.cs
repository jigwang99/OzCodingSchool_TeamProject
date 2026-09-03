using System;
using UnityEngine;

// 전투 결과를 '물고기 드롭'으로 변환하는 전투 씬 전담 시스템.
//  - CombatManager.OnEnemyDefeated 구독 → 적 처치 감지
//  - StageDropTable로 획득 확률/등급/종을 굴림
//  - 결과는 OnFishDropped 이벤트로만 방출 (인벤토리 담당이 구독해서 저장)
public class FishDropSystem : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private StageDropTable defaultDropTable; // 스테이지 테이블 미지정 시 사용

    private float dropChanceMultiplier = 1f; // 드롭률 업그레이드 배수 (성장 담당이 세팅)
    private StageDropTable currentTable;

    // 인벤토리/재화 담당이 구독: 어떤 등급의 어떤 종을 몇 개 얻었는지
    public event Action<FishDrop> OnFishDropped;

    private void Awake() => currentTable = defaultDropTable;

    private void OnEnable()
    {
        if (combatManager != null)
            combatManager.OnEnemyDefeated += HandleEnemyDefeated;
    }

    private void OnDisable()
    {
        if (combatManager != null)
            combatManager.OnEnemyDefeated -= HandleEnemyDefeated;
    }

    // StageManager가 스테이지 시작 시 현재 테이블을 지정 (없으면 default 유지)
    public void SetDropTable(StageDropTable table)
    {
        currentTable = table != null ? table : defaultDropTable;
    }

    // 드롭률 업그레이드 반영 (성장 담당이 호출)
    public void SetDropChanceMultiplier(float multiplier)
    {
        dropChanceMultiplier = Mathf.Max(0f, multiplier);
    }

    private void HandleEnemyDefeated(EnemyController enemy)
    {
        if (currentTable == null || enemy == null)
            return;

        float chance = Mathf.Clamp01(currentTable.DropChance * dropChanceMultiplier);
        if (UnityEngine.Random.value > chance)
            return; // 이번엔 드롭 없음

        if (!TryRollGrade(currentTable, out StageDropTable.GradeWeight picked))
            return;

        int species = RollSpecies(picked);
        OnFishDropped?.Invoke(new FishDrop(picked.grade, species, 1, enemy.transform.position));
    }

    // 등급 가중치 기반 랜덤 추첨 → 선택된 등급 구성 전체를 반환
    private static bool TryRollGrade(StageDropTable table, out StageDropTable.GradeWeight picked)
    {
        picked = default;

        var weights = table.Weights;
        if (weights == null || weights.Length == 0)
            return false;

        float total = 0f;
        foreach (var gw in weights)
            total += Mathf.Max(0f, gw.weight);

        if (total <= 0f)
            return false;

        float roll = UnityEngine.Random.value * total;
        foreach (var gw in weights)
        {
            roll -= Mathf.Max(0f, gw.weight);
            if (roll <= 0f)
            {
                picked = gw;
                return true;
            }
        }

        picked = weights[weights.Length - 1];
        return true;
    }

    // 등급 안에서 종 인덱스 추첨.
    // speciesWeights가 speciesCount와 맞으면 가중치 추첨, 아니면 균등 추첨.
    private static int RollSpecies(StageDropTable.GradeWeight gw)
    {
        int count = Mathf.Max(1, gw.speciesCount);
        var weights = gw.speciesWeights;

        if (weights == null || weights.Length != count)
            return UnityEngine.Random.Range(0, count);

        float total = 0f;
        foreach (var w in weights)
            total += Mathf.Max(0f, w);

        if (total <= 0f)
            return UnityEngine.Random.Range(0, count);

        float roll = UnityEngine.Random.value * total;
        for (int i = 0; i < weights.Length; i++)
        {
            roll -= Mathf.Max(0f, weights[i]);
            if (roll <= 0f)
                return i;
        }
        return count - 1;
    }
}