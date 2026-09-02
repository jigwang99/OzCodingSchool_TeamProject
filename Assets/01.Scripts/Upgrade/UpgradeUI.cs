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

    [SerializeField] private TMP_Text goldText;

    private void Start()
    {
        upgradeButton.onClick.AddListener(OnClickUpgrade);
        RefreshUI();
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

    private void RefreshUI()
    {
        PlayerData playerData = GameManager.instance.PlayerData;

        goldText.text = $"Gold: {playerData.gold:N0}";
        
        int currentLevel = UpgradeManager.instance.GetCurrentLevel(upgradeData, playerData);

        if (currentLevel >= upgradeData.maxLevel)
        {
            upgradeText.text = $"Lv. {currentLevel} / MAX";
            upgradeButton.interactable = false;
            return;
        }

        double cost = UpgradeManager.instance.GetUpgradeCost(upgradeData, currentLevel);

        upgradeText.text = $"Lv. {currentLevel} / Price: {cost:N0} G";
    }
}