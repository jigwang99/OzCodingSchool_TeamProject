using System;
using UnityEngine;

// 전투 결과를 '물고기 드롭'으로 변환하는 전투 씬 전담 시스템.
//  - CombatManager.OnEnemyDefeated 구독 → 적 처치 감지
//  - StageDropTable로 획득 확률/등급을 굴림
//  - 결과는 OnFishDropped 이벤트로만 방출 (인벤토리 담당이 구독해서 저장)
public class FishDropSystem : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private StageDropTable defaultDropTable; // 스테이지 테이블 미지정 시 사용

    private float dropChanceMultiplier = 1f; // 드롭률 업그레이드 배수 (성장 담당이 세팅)
    private StageDropTable currentTable;

    // 인벤토리/재화 담당이 구독: 어떤 등급을 몇 개 얻었는지
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

    // 드롭률 업그레이드 반영
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

        if (!TryRollGrade(currentTable, out FishGrade grade))
            return;

        OnFishDropped?.Invoke(new FishDrop(grade, 1, enemy.transform.position));
    }

    // 등급 가중치 기반 랜덤 추첨
    private static bool TryRollGrade(StageDropTable table, out FishGrade grade)
    {
        grade = FishGrade.Common;

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
                grade = gw.grade;
                return true;
            }
        }

        grade = weights[weights.Length - 1].grade;
        return true;
    }
}