using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [Header("업그레이드 데이터")]
    [SerializeField] private UpgradeData upgradeData;

    [Header("UI")]
    [SerializeField] private TMP_Text upgradeText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeButtonText;

    private void Start()
    {
        upgradeButton.onClick.AddListener(OnClickUpgrade);
        RefreshUI();
    }

    private void OnEnable()
    {
        if (UpgradeManager.instance != null)
            UpgradeManager.instance.OnUpgradePurchased += OnUpgradePurchased;
    }

    private void OnDisable()
    {
        if (UpgradeManager.instance != null)
            UpgradeManager.instance.OnUpgradePurchased -= OnUpgradePurchased;
    }

    private void OnDestroy()
    {
        upgradeButton.onClick.RemoveListener(OnClickUpgrade);
    }

    private void OnClickUpgrade()
    {
        PlayerData playerData = GameManager.instance.PlayerData;

        UpgradeManager.instance.TryUpgrade(upgradeData, playerData);

        RefreshUI();
    }

    private void OnUpgradePurchased(UpgradeData data, int level)
    {
        // 내가 담당하는 업그레이드가 아니면 무시
        if (data != upgradeData)
            return;

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (upgradeData == null)
            return;

        if (upgradeButtonText != null)
        {
            upgradeButtonText.text = upgradeData.upgradeName;
        }

        PlayerData playerData = GameManager.instance.PlayerData;

        int currentLevel = UpgradeManager.instance.GetCurrentLevel(upgradeData, playerData);

        if (currentLevel >= upgradeData.maxLevel)
        {
            upgradeText.text = $"Lv. {currentLevel} / MAX";
            upgradeButton.interactable = false;
            return;
        }

        double cost = UpgradeManager.instance.GetUpgradeCost(upgradeData, currentLevel);

        upgradeText.text = $"Lv. {currentLevel} / Price: {cost:N0} G";

        upgradeButton.interactable = true;
    }
}