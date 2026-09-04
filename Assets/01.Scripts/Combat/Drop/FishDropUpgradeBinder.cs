using UnityEngine;

// 드롭률 업그레이드 → FishDropSystem 배수 반영 (전투 씬 담당)
// 로드 직후(OnEnable)와 구매 시(OnUpgradePurchased) 모두 여기서 반영한다.
public class FishDropUpgradeBinder : MonoBehaviour
{
    [SerializeField] private FishDropSystem fishDropSystem;
    [SerializeField] private UpgradeData dropRateUpgrade; // type = FishDropRate 에셋

    private void OnEnable()
    {
        if (UpgradeManager.instance != null)
            UpgradeManager.instance.OnUpgradePurchased += HandlePurchased;

        Apply(); // 저장된 레벨을 진입 시 즉시 반영
    }

    private void OnDisable()
    {
        if (UpgradeManager.instance != null)
            UpgradeManager.instance.OnUpgradePurchased -= HandlePurchased;
    }

    private void HandlePurchased(UpgradeData data, int level)
    {
        if (data == dropRateUpgrade) // 드롭률 업그레이드일 때만 반응
            Apply();
    }

    private void Apply()
    {
        if (fishDropSystem == null || dropRateUpgrade == null)
        {
            Debug.LogWarning("[FishDropUpgradeBinder] 참조가 비어 있습니다.");
            return;
        }

        if (GameManager.instance == null || UpgradeManager.instance == null)
            return; // 매니저 초기화 순서 방어

        int level = UpgradeManager.instance.GetCurrentLevel(dropRateUpgrade, GameManager.instance.PlayerData);
        fishDropSystem.SetDropChanceMultiplier(dropRateUpgrade.GetDropChanceMultiplier(level));
    }
}