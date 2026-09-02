癤using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaUI : MonoBehaviour
{
    [Header("Gacha / Pool")]
    public GachaPool pool; // inspector 留
    public PoolManager poolManager; // inspector PoolManager瑜 洹명嫄곕 쇰 李얠

    [Header("Cost")]
    public int costPerDraw = 10;

    [Header("UI")]
    public Button drawButton;
    public TMP_Text moneyText;
    public TMP_Text drawButtonText; // 踰 ㅽ몄 鍮 ()
    public GameObject popupPanel; //  ⑤ (鍮깊  ъ)
    public TMP_Text popupTitleText;
    public TMP_Text popupDetailText;
    public Transform popupPreviewParent; //   prefab 遺紐⑤  Transform (鍮 GameObject)
    public Button popupCloseButton;

    private GameObject currentPreviewInstance;
    /*
    void Start()
    {
        if (poolManager == null)
        {
            poolManager = FindObjectOfType<PoolManager>();
            if (poolManager == null) Debug.LogWarning("PoolManager not found in scene. Assign in inspector.");
        }

        drawButton.onClick.AddListener(OnDrawButton);
        if (popupCloseButton != null) popupCloseButton.onClick.AddListener(ClosePopup);
        CurrencyManager.Instance.OnMoneyChanged += UpdateMoneyUI;

        UpdateMoneyUI(CurrencyManager.Instance.currentMoney);

        if (drawButtonText != null)
            drawButtonText.text = $"戮湲 ({costPerDraw})";
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnMoneyChanged -= UpdateMoneyUI;
    }

    void UpdateMoneyUI(int amount)
    {
        if (moneyText != null)
            moneyText.text = $"Money: {amount}";
    }

    public void OnDrawButton()
    {
        if (pool == null)
        {
            Debug.LogWarning("GachaUI: pool is null");
            return;
        }

        if (!CurrencyManager.Instance.CanSpend(costPerDraw))
        {
            // 遺議 UI 泥由
            Debug.Log(" 遺議");
            // TODO: 遺議   
            return;
        }

        // 吏遺
        CurrencyManager.Instance.Spend(costPerDraw);

        // ㅼ 戮湲
        var result = GachaManager.Instance.DrawFromPool(pool);
        if (result == null)
        {
            Debug.LogWarning("GachaUI: draw result is null");
            return;
        }

        //  ㅽ 
        if (popupTitleText != null) popupTitleText.text = $"{result.rarity} 깃!";
        if (popupDetailText != null) popupDetailText.text = $"댄: {result.itemId}\n洹몃９: {result.groupName}";

        // 湲곗〈 誘몃━蹂닿린 쇰㈃ 諛
        if (currentPreviewInstance != null)
        {
            poolManager.ReleaseToPool(currentPreviewInstance);
            currentPreviewInstance = null;
        }

        // 由ы뱀 쇰㈃  諛  誘몃━蹂닿린 移 遺
        if (result.prefab != null && poolManager != null)
        {
            currentPreviewInstance = poolManager.GetFromPool(result.prefab);
            if (currentPreviewInstance != null)
            {
                // 遺紐⑤� popupPreviewParent濡 怨 濡而 蹂 珥湲고
                currentPreviewInstance.transform.SetParent(popupPreviewParent, false);
                currentPreviewInstance.transform.localPosition = Vector3.zero;
                currentPreviewInstance.transform.localRotation = Quaternion.identity;
                currentPreviewInstance.transform.localScale = Vector3.one;
            }
        }

        //  닿린
        if (popupPanel != null) popupPanel.SetActive(true);
    }

    public void ClosePopup()
    {
        if (currentPreviewInstance != null && poolManager != null)
        {
            poolManager.ReleaseToPool(currentPreviewInstance);
            currentPreviewInstance = null;
        }

        if (popupPanel != null) popupPanel.SetActive(false);
    }
    */
}