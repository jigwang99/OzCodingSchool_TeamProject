using UnityEngine;

// FishDropSystem(전투) → CurrencyManager(재고 게이트) 연결 지점.
// 전투 씬은 '등급 + 종'까지 정해 방출하고,
// 실제 저장은 CurrencyManager.AddFish가, 표시 갱신은 CurrencyManager.OnFishChanged 구독자가 담당한다.
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
        // 재고 반영은 게이트 하나로만. (배열 직접 접근 · 랜덤 칸 · 수동 UI 갱신 제거)
        CurrencyManager.instance.AddFish(drop.Grade, drop.Species, drop.Count);
    }
}