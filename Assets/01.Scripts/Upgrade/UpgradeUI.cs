using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("업그레이드 데이터")]
    [SerializeField] private UpgradeData upgradeData;

    [Header("UI")]
    [SerializeField] private TMP_Text upgradeText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeButtonText;
    [SerializeField] private Image goldFilledImage;

    [Header("연속 강화")]
    [SerializeField] private float repeatDelay = 0.2f;

    private Coroutine upgradeCoroutine;

    private void Start()
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.OnGoldChanged += OnGoldChanged;

        if (UpgradeManager.instance != null)
            UpgradeManager.instance.OnUpgradePurchased += OnUpgradePurchased;
    }

    private void OnDisable()
    {
        StopUpgradeCoroutine();

        if (CurrencyManager.instance != null)
            CurrencyManager.instance.OnGoldChanged -= OnGoldChanged;

        if (UpgradeManager.instance != null)
            UpgradeManager.instance.OnUpgradePurchased -= OnUpgradePurchased;
    }

    // 버튼을 누르기 시작
    public void OnPointerDown(PointerEventData eventData)
    {
        if (upgradeButton == null || !upgradeButton.interactable)
            return;

        // 첫 강화는 즉시 실행
        TryUpgrade();

        // 0.2초 후부터 반복
        upgradeCoroutine = StartCoroutine(RepeatUpgrade());
    }

    // 버튼에서 손을 뗌
    public void OnPointerUp(PointerEventData eventData)
    {
        StopUpgradeCoroutine();
    }

    private IEnumerator RepeatUpgrade()
    {
        yield return new WaitForSeconds(repeatDelay);

        while (true)
        {
            if (upgradeButton == null || !upgradeButton.interactable)
                break;

            TryUpgrade();

            yield return new WaitForSeconds(repeatDelay);
        }

        upgradeCoroutine = null;
    }

    private void StopUpgradeCoroutine()
    {
        if (upgradeCoroutine != null)
        {
            StopCoroutine(upgradeCoroutine);
            upgradeCoroutine = null;
        }
    }

    private void TryUpgrade()
    {
        if (upgradeData == null)
            return;

        if (upgradeButton != null && !upgradeButton.interactable)
            return;

        PlayerData playerData = GameManager.instance.PlayerData;

        UpgradeManager.instance.TryUpgrade(upgradeData, playerData);

        SaveManager.instance.Save();
    }

    private void OnGoldChanged(BigNumber gold)
    {
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
            upgradeButtonText.text = $"[ {upgradeData.upgradeName} ]";
        }

        PlayerData playerData = GameManager.instance.PlayerData;

        int currentLevel = UpgradeManager.instance.GetCurrentLevel(upgradeData, playerData);

        if (currentLevel >= upgradeData.maxLevel)
        {
            upgradeText.text = $"현재 Lv. {currentLevel}";
            upgradeButton.interactable = false;
            return;
        }

        BigNumber cost = UpgradeManager.instance.GetUpgradeCost(upgradeData, currentLevel);

        upgradeText.text = $"현재 Lv. {currentLevel} \n\n필요 골드 {cost} G";

        upgradeButton.interactable = true;

        BigNumber currentGold = playerData.gold;

        double fillAmount = 0.0;

        if (cost > new BigNumber(0))
        {
            BigNumber ratio = currentGold / cost;

            fillAmount = ratio.value * Math.Pow(10, ratio.exponent);

            fillAmount = Math.Max(0.0, Math.Min(1.0, fillAmount));
        }

        if (goldFilledImage != null)
        {
            goldFilledImage.fillAmount = (float)fillAmount;
        }
    }
}
