using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIManager : Singleton<UpgradeUIManager>
{
    [Header("업그레이드 데이터")]
    [SerializeField] private UpgradeData[] upgradeDatas;

    [Header("업그레이드 UI")]
    [SerializeField] private GameObject upgradeUIPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject background;

    [Header("풀 설정")]
    [SerializeField] private int poolSize = 5;

    private readonly Queue<GameObject> upgradeUIPool = new();
    private readonly List<GameObject> activeUpgradeUIs = new();

    // 현재 열려 있는 카테고리
    private UpgradeCategory? currentCategory = null;

    protected override void Awake()
    {
        base.Awake();

        if (instance != this)
            return;

        CreatePool();

        background.SetActive(false);
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(upgradeUIPrefab, content);

            obj.SetActive(false);

            upgradeUIPool.Enqueue(obj);
        }
    }

    public void ShowUpgrades(UpgradeCategory category)
    {
        // 같은 메뉴를 다시 누르면 닫기
        if (currentCategory == category)
        {
            ClearUpgrades();
            currentCategory = null;
            background.SetActive(false);
            return;
        }

        // 다른 메뉴를 누르면 기존 UI를 제거하고 새로 표시
        ClearUpgrades();

        currentCategory = category;

        background.SetActive(true);

        int siblingIndex = 0;

        foreach (UpgradeData data in upgradeDatas)
        {
            if (data == null)
                continue;

            if (data.category != category)
                continue;

            GameObject obj = GetUI();

            if (obj == null)
            {
                Debug.LogWarning("UpgradeUI Pool이 부족합니다.");
                break;
            }

            obj.SetActive(true);
            obj.transform.SetParent(content, false);

            // 항상 데이터 배열 순서대로 배치
            obj.transform.SetSiblingIndex(siblingIndex++);

            UpgradeUI ui = obj.GetComponentInChildren<UpgradeUI>(true);

            if (ui == null)
            {
                Debug.LogError("UpgradeUI 컴포넌트를 찾을 수 없습니다.");

                ReturnUI(obj);
                continue;
            }

            ui.Initialize(data);

            activeUpgradeUIs.Add(obj);
        }
    }

    private GameObject GetUI()
    {
        if (upgradeUIPool.Count == 0)
            return null;

        return upgradeUIPool.Dequeue();
    }

    private void ReturnUI(GameObject obj)
    {
        if (obj == null)
            return;

        obj.SetActive(false);

        upgradeUIPool.Enqueue(obj);
    }

    private void ClearUpgrades()
    {
        foreach (GameObject obj in activeUpgradeUIs)
        {
            ReturnUI(obj);
        }

        activeUpgradeUIs.Clear();
    }

    public void ClickCombatMenu()
    {
        ShowUpgrades(UpgradeCategory.Combat);
    }

    public void ClickBusinessMenu()
    {
        ShowUpgrades(UpgradeCategory.Business);
    }
}