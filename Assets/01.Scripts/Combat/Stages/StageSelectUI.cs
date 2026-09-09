using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// CombatScene 전용. 열려 있는 동안 전투는 계속되며, 선택 시 StageManager가 새 전투를 시작한다.
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public class StageSelectUI : MonoBehaviour
{
    [Serializable]
    private sealed class StageButton
    {
        [Min(1)] public int stageNumber;
        public Button button;
        public Text label;
    }

    [SerializeField] private StageManager stageManager;
    [Header("씬에 배치된 UI")]
    [SerializeField] private GameObject modal;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button openButton;
    [SerializeField] private Text hint;
    [SerializeField] private List<StageButton> stages = new List<StageButton>();

    [Header("상태별 색상")]
    [SerializeField] private Color normalColor = new Color(0.18f, 0.23f, 0.32f);
    [SerializeField] private Color bossColor = new Color(0.38f, 0.27f, 0.15f);
    [SerializeField] private Color currentColor = new Color(0.13f, 0.46f, 0.48f);
    private const string SelectionHint = "번호를 누르면 해당 스테이지에서 전투를 새로 시작합니다.";
    private bool initialized;

    private void Awake()
    {
        if (stageManager == null || stageManager.StageCount == 0 || modal == null ||
            panel == null || openButton == null || hint == null || stages == null || stages.Count == 0)
        {
            Debug.LogError("[StageSelectUI] 스테이지 데이터와 씬의 UI 참조를 확인하세요.", this);
            enabled = false;
            return;
        }

        var numbers = new HashSet<int>();
        foreach (StageButton stage in stages)
        {
            if (stage == null || stage.button == null || stage.button.image == null || stage.label == null ||
                stage.stageNumber < 1 || stage.stageNumber > stageManager.StageCount || !numbers.Add(stage.stageNumber))
            {
                Debug.LogError("[StageSelectUI] 버튼 참조와 중복되지 않는 스테이지 번호를 확인하세요.", this);
                enabled = false;
                return;
            }
        }

        initialized = true;
        Hide();
    }

    private void OnEnable()
    {
        if (!initialized) return;
        openButton.interactable = true;
        stageManager.OnStageStarted += Refresh;
        stageManager.OnStageResult += HandleResult;
        Refresh();
    }

    private void OnDisable()
    {
        if (stageManager != null)
        {
            stageManager.OnStageStarted -= Refresh;
            stageManager.OnStageResult -= HandleResult;
        }
        if (openButton != null) openButton.interactable = false;
        Hide();
    }

    public void Show()
    {
        if (!initialized || !isActiveAndEnabled) return;
        hint.text = SelectionHint;
        modal.SetActive(true);
        ResizePanel();
        Refresh();
    }

    public void Hide()
    {
        if (modal != null) modal.SetActive(false);
    }

    // 씬의 Button.onClick에서 선택할 스테이지 번호를 int 인자로 전달한다.
    public void Select(int stageNumber)
    {
        if (!initialized || !isActiveAndEnabled) return;
        if (stageManager.SelectStage(stageNumber))
            Hide();
        else
            hint.text = "스테이지를 시작할 수 없습니다. 잠시 후 다시 선택해 주세요.";
    }

    private void HandleResult(StageResult _) => Refresh();

    private void Refresh()
    {
        if (!initialized) return;
        foreach (StageButton stage in stages)
        {
            int number = stage.stageNumber;
            bool current = number == stageManager.CurrentStageNumber;
            bool boss = number % 5 == 0;
            stage.button.image.color = current ? currentColor : boss ? bossColor : normalColor;
            string status = boss ? (current ? "보스 · 현재" : "보스") : (current ? "현재" : "일반");
            stage.label.text = stageManager.GetStageName(number) + "\n" + status;
        }
    }

    private void OnRectTransformDimensionsChange() => ResizePanel();

    private void ResizePanel()
    {
        if (!initialized || panel == null) return;
        Rect bounds = ((RectTransform)transform).rect;
        Rect authoredSize = panel.rect;
        if (authoredSize.width <= 0f || authoredSize.height <= 0f) return;
        // 씬에서 설정한 크기와 배치를 보존하고 작은 화면에서는 전체 배율만 줄인다.
        float scale = Mathf.Min(1f, (bounds.width - 32f) / authoredSize.width,
            (bounds.height - 32f) / authoredSize.height);
        panel.localScale = Vector3.one * Mathf.Max(0.1f, scale);
    }
}