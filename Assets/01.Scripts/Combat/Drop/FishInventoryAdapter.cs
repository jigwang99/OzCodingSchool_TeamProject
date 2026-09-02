using UnityEngine;

// FishDropSystem(전투) → FishInventoryManager(임시 인벤토리) 연결 지점.
// 전투 씬은 '등급'까지만 방출하고, 실제 저장/표시는 여기서 물고기숫자확인용 구조에 맞춰 번역한다.
// 정식 인벤토리 API가 생기면 HandleFishDropped 내부만 교체하면 됨.
public class FishInventoryAdapter : MonoBehaviour
{
    [SerializeField] private FishDropSystem fishDropSystem;

    private void OnEnable()
    {
        if (fishDropSystem != null)
            fishDropSystem.OnFishDropped += HandleFishDropped;
    }

    private void OnDisable()
    {
        if (fishDropSystem != null)
            fishDropSystem.OnFishDropped -= HandleFishDropped;
    }

    private void HandleFishDropped(FishDrop drop)
    {
        var inventory = FishInventoryManager.instance;
        if (inventory == null)
            return;

        int[] target = GetGradeArray(inventory, drop.Grade);
        if (target == null || target.Length == 0)
            return;

        // 드롭은 등급까지만 결정 → 등급 안에서 어떤 물고기인지는 임시로 랜덤 지정
        int index = Random.Range(0, target.Length);
        target[index] += Mathf.Max(1, drop.Count);

        RefreshDisplays();
    }

    private static int[] GetGradeArray(FishInventoryManager inventory, FishGrade grade)
    {
        switch (grade)
        {
            case FishGrade.Common: return inventory.commonFishNum;
            case FishGrade.Rare: return inventory.rareFishNum;
            case FishGrade.Unique: return inventory.uniqueFishNum;
            case FishGrade.Epic: return inventory.epicFishNum;
            default: return null;
        }
    }

    // 물고기숫자확인용 표시들을 배열 값 기준으로 갱신 (임시 인벤토리 UI)
    private static void RefreshDisplays()
    {
        var displays = FindObjectsOfType<물고기숫자확인용>();
        foreach (var display in displays)
            display.UpdateText();
    }
}