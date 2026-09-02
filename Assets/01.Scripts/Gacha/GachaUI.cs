using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaUI : MonoBehaviour
{
    //[Header("Gacha / Pool")]
    //public GachaPool pool; // inspector에서 링크
    //public PoolManager poolManager; // inspector에 PoolManager를 드래그하거나 자동으로 찾음

    //[Header("Cost")]
    //public int costPerDraw = 10;

    //[Header("UI")]
    //public Button drawButton;
    //public TMP_Text moneyText;
    //public TMP_Text drawButtonText; // 버튼 텍스트에 비용 표시(선택)
    //public GameObject popupPanel; // 팝업 패널 (비활성화 상태에서 사용)
    //public TMP_Text popupTitleText;
    //public TMP_Text popupDetailText;
    //public Transform popupPreviewParent; // 팝업에 표시할 prefab을 부모로 둘 Transform (빈 GameObject)
    //public Button popupCloseButton;

    //private GameObject currentPreviewInstance;

    //void Start()
    //{
    //    if (poolManager == null)
    //    {
    //        poolManager = FindObjectOfType<PoolManager>();
    //        if (poolManager == null) Debug.LogWarning("PoolManager not found in scene. Assign in inspector.");
    //    }

    //    drawButton.onClick.AddListener(OnDrawButton);
    //    if (popupCloseButton != null) popupCloseButton.onClick.AddListener(ClosePopup);
    //    CurrencyManager.Instance.OnMoneyChanged += UpdateMoneyUI;

    //    UpdateMoneyUI(CurrencyManager.Instance.currentMoney);

    //    if (drawButtonText != null)
    //        drawButtonText.text = $"뽑기 ({costPerDraw})";
    //}

    //void OnDestroy()
    //{
    //    if (CurrencyManager.Instance != null)
    //        CurrencyManager.Instance.OnMoneyChanged -= UpdateMoneyUI;
    //}

    //void UpdateMoneyUI(int amount)
    //{
    //    if (moneyText != null)
    //        moneyText.text = $"Money: {amount}";
    //}

    //public void OnDrawButton()
    //{
    //    if (pool == null)
    //    {
    //        Debug.LogWarning("GachaUI: pool is null");
    //        return;
    //    }

    //    if (!CurrencyManager.Instance.CanSpend(costPerDraw))
    //    {
    //        // 부족 UI 처리
    //        Debug.Log("돈 부족");
    //        // TODO: 부족 안내 팝업 등
    //        return;
    //    }

    //    // 지불
    //    CurrencyManager.Instance.Spend(costPerDraw);

    //    // 실제 뽑기
    //    var result = GachaManager.Instance.DrawFromPool(pool);
    //    if (result == null)
    //    {
    //        Debug.LogWarning("GachaUI: draw result is null");
    //        return;
    //    }

    //    // 팝업에 텍스트 셋업
    //    if (popupTitleText != null) popupTitleText.text = $"{result.rarity} 등급!";
    //    if (popupDetailText != null) popupDetailText.text = $"아이템: {result.itemId}\n그룹: {result.groupName}";

    //    // 기존 미리보기 있으면 반환
    //    if (currentPreviewInstance != null)
    //    {
    //        poolManager.ReleaseToPool(currentPreviewInstance);
    //        currentPreviewInstance = null;
    //    }

    //    // 프리팹이 있으면 풀에서 받아와 팝업 미리보기 위치에 붙임
    //    if (result.prefab != null && poolManager != null)
    //    {
    //        currentPreviewInstance = poolManager.GetFromPool(result.prefab);
    //        if (currentPreviewInstance != null)
    //        {
    //            // 부모를 popupPreviewParent로 하고 로컬 변환 초기화
    //            currentPreviewInstance.transform.SetParent(popupPreviewParent, false);
    //            currentPreviewInstance.transform.localPosition = Vector3.zero;
    //            currentPreviewInstance.transform.localRotation = Quaternion.identity;
    //            currentPreviewInstance.transform.localScale = Vector3.one;
    //        }
    //    }

    //    // 팝업 열기
    //    if (popupPanel != null) popupPanel.SetActive(true);
    //}

    //public void ClosePopup()
    //{
    //    if (currentPreviewInstance != null && poolManager != null)
    //    {
    //        poolManager.ReleaseToPool(currentPreviewInstance);
    //        currentPreviewInstance = null;
    //    }

    //    if (popupPanel != null) popupPanel.SetActive(false);
    //}
}