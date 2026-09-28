using UnityEngine;

// 플레이어 체력 강화만 연결한다. 스테이지 전환 중 컨트롤러가 꺼져도 구독을 유지한다.
[DisallowMultipleComponent]
[RequireComponent(typeof(UnitHealth))]
public class PlayerHealthUpgradeBinder : MonoBehaviour
{
    [SerializeField] private UpgradeData healthUpgrade;

    private UnitHealth health;
    private UpgradeManager subscribedUpgradeManager;
    private bool started;

    private void Awake() => health = GetComponent<UnitHealth>();

    private void OnEnable()
    {
        subscribedUpgradeManager = UpgradeManager.instance;
        if (subscribedUpgradeManager != null)
            subscribedUpgradeManager.OnUpgradePurchased += HandlePurchased;

        if (started)
            Apply(false);
    }

    private void Start()
    {
        // 모든 Awake(저장 데이터 로드/UnitHealth 초기화)가 끝난 뒤 최초 체력을 채운다.
        started = true;
        Apply(true);
    }

    private void OnDisable()
    {
        if (subscribedUpgradeManager != null)
            subscribedUpgradeManager.OnUpgradePurchased -= HandlePurchased;

        subscribedUpgradeManager = null;
    }

    private void HandlePurchased(UpgradeData data, int level)
    {
        if (started && data == healthUpgrade)
            Apply(false);
    }

    private void Apply(bool resetCurrentHp)
    {
        if (health == null || healthUpgrade == null || healthUpgrade.type != UpgradeType.Health)
            return;

        PlayerData data = GameManager.instance.PlayerData;
        if (data == null)
            return;

        // 구매/재활성화로 회복하거나 부활하지 않는다. 다음 스테이지의 Revive가 체력을 채운다.
        health.SetMaxHp(healthUpgrade.GetMaxHealth(Mathf.Max(1, data.healthLevel)), resetCurrentHp);
    }
}
