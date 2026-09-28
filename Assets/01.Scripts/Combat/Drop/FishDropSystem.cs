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
    private int pendingBossFishCount;

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
    public void SetDropTable(StageDropTable table, int bossGuaranteedFishCount = 0)
    {
        currentTable = table != null ? table : defaultDropTable;
        pendingBossFishCount = Mathf.Max(0, bossGuaranteedFishCount);
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

        Vector3 sourcePosition = enemy.transform.position;
        StageDropTable table = currentTable;
        // 마지막 적의 처치 이벤트는 클리어 판정·저장보다 먼저 발생한다.
        // 일반 드롭과 별도로 지급하며, 이벤트 재진입 시에도 보상을 중복 지급하지 않는다.
        if (pendingBossFishCount > 0 && combatManager != null && combatManager.RemainingEnemyCount == 0)
        {
            int guaranteedCount = pendingBossFishCount;
            pendingBossFishCount = 0;
            for (int i = 0; i < guaranteedCount; i++)
            {
                if (!TryRollFish(table, out FishGrade bossGrade, out int bossSpecies))
                {
                    Debug.LogError("[FishDropSystem] 보스 확정 보상 테이블의 물고기 가중치를 확인하세요.", this);
                    break;
                }
                OnFishDropped?.Invoke(new FishDrop(bossGrade, bossSpecies, 1, sourcePosition));
            }
        }

        // 100%를 넘는 강화분도 보상으로 반영한다.
        // 예: 130% = 1회 확정 + 30% 확률로 1회 추가, 220% = 2회 확정 + 20% 추가.
        int dropCount = CalculateDropCount(table.DropChance * dropChanceMultiplier, UnityEngine.Random.value);

        for (int i = 0; i < dropCount; i++)
        {
            if (!TryRollFish(table, out FishGrade grade, out int species))
                return;

            // 추가 드롭도 별도로 추첨하여 기존 등급/종별 확률을 유지한다.
            OnFishDropped?.Invoke(new FishDrop(grade, species, 1, sourcePosition));
        }
    }

    internal static int CalculateDropCount(float expectedDrops, float roll)
    {
        if (expectedDrops <= 0f || float.IsNaN(expectedDrops) || float.IsInfinity(expectedDrops))
            return 0;

        int guaranteedDrops = Mathf.FloorToInt(expectedDrops);
        float extraChance = expectedDrops - guaranteedDrops;
        // 0%는 난수가 0이어도 지급하지 않고, 정수 배율은 확정 수량만 지급한다.
        return guaranteedDrops + (roll < extraChance ? 1 : 0);
    }

    // 전투 드롭과 방치 생산이 같은 등급/종 가중치를 사용한다. 수량 결정은 호출자가 담당한다.
    internal static bool TryRollFish(StageDropTable table, out FishGrade grade, out int species)
    {
        grade = default;
        species = 0;
        if (table == null || !TryRollGrade(table, out StageDropTable.GradeWeight picked))
            return false;
        grade = picked.grade;
        species = RollSpecies(picked);
        return true;
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
            if (gw.weight <= 0f) continue;
            picked = gw;
            roll -= Mathf.Max(0f, gw.weight);
            if (roll <= 0f)
            {
                picked = gw;
                return true;
            }
        }

        // Random.value가 1이거나 부동소수점 오차가 있어도 마지막 유효 등급을 사용한다.
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
        int lastPositive = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            if (weights[i] <= 0f) continue;
            lastPositive = i;
            roll -= Mathf.Max(0f, weights[i]);
            if (roll <= 0f)
                return i;
        }
        return lastPositive;
    }
}
