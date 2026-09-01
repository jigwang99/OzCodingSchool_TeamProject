using UnityEngine;

// FishDropSystem(전투) → FishInventoryManager(인벤토리) 연결 지점.
// 전투 씬은 '등급'까지만 방출하고, '어느 슬롯에 넣을지'는 여기서 인벤토리 규칙에 맞춰 번역한다.
// 인벤토리 구현이 바뀌면 이 어댑터만 수정하면 됨.
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

        // 권장 방식: 인벤토리에 AddFish 메서드를 두고 호출 (아래 B안 참고)
        //inventory.AddFish(drop.Grade, drop.Count);
    }
}