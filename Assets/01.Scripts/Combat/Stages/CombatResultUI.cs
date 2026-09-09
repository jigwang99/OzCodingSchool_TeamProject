using UnityEngine;
using UnityEngine.UI;

// CombatScene의 전용 Canvas에만 배치한다. 진행/재시작은 StageManager가 전담한다.
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public class CombatResultUI : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private CombatRewardTracker rewardTracker;
    [Header("결과 색상")]
    [SerializeField] private Color clearColor = new Color(1f, 0.82f, 0.39f);
    [SerializeField] private Color failColor = new Color(1f, 0.48f, 0.46f);

    [Header("씬에 배치된 UI")]
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Text stageLabel;
    [SerializeField] private Text titleLabel;
    [SerializeField] private Text messageLabel;
    [SerializeField] private Text rewardLabel;
    [SerializeField] private Text countdownLabel;
    [SerializeField] private Image accent;
    [SerializeField] private Image progress;
    private bool initialized;
    private float duration;
    private float visibleTime;
    private int displayedSeconds = -1;

    private void Awake()
    {
        if (stageManager == null || rewardTracker == null || panel == null || panelRect == null ||
            stageLabel == null || titleLabel == null || messageLabel == null || rewardLabel == null ||
            countdownLabel == null || accent == null || progress == null)
        {
            Debug.LogError("[CombatResultUI] 결과 데이터와 씬에 배치된 UI 참조를 연결하세요.", this);
            enabled = false;
            return;
        }

        initialized = true;
        Hide();
    }

    private void OnEnable()
    {
        if (!initialized) return;
        stageManager.OnStageResult += Show;
        stageManager.OnStageStarted += Hide;

        // 결과 표시 중 컴포넌트가 다시 활성화되어도 실제 남은 대기 시간을 사용한다.
        if (stageManager.CurrentResult.HasValue)
            Show(stageManager.CurrentResult.Value);
    }

    private void OnDisable()
    {
        if (stageManager != null)
        {
            stageManager.OnStageResult -= Show;
            stageManager.OnStageStarted -= Hide;
        }
        Hide();
    }

    private void Update()
    {
        if (!initialized || !panel.gameObject.activeSelf) return;

        // 전투/재시작과 동일하게 일시정지와 배속을 따른다.
        visibleTime += Time.deltaTime;
        panel.alpha = Mathf.Clamp01(visibleTime / 0.15f);
        UpdateCountdown();
    }

    private void Show(StageResult result)
    {
        stageLabel.text = $"스테이지 {result.CompletedStageName}";
        titleLabel.text = result.Title;
        messageLabel.text = result.Message;
        // CombatManager가 마지막 적의 드롭 이벤트를 처리한 뒤 결과를 알리므로 마지막 보상도 포함된다.
        rewardLabel.text = $"이번 전투 획득 물고기 ×{rewardTracker.TotalFishCount:N0}";
        Color color = result.IsClear ? clearColor : failColor;
        titleLabel.color = color;
        accent.color = color;
        progress.color = color;
        duration = result.Delay;
        visibleTime = 0f;
        displayedSeconds = -1;
        panel.alpha = 0f;
        panel.gameObject.SetActive(true);
        ResizePanel();
        UpdateCountdown();
    }

    private void Hide()
    {
        if (panel != null)
        {
            panel.alpha = 0f;
            panel.gameObject.SetActive(false);
        }
    }

    private void UpdateCountdown()
    {
        float remaining = stageManager.ResultRemainingSeconds;
        int seconds = Mathf.CeilToInt(remaining);
        if (displayedSeconds != seconds)
        {
            displayedSeconds = seconds;
            countdownLabel.text = seconds > 0 ? $"{seconds}초 후 자동 시작" : "전투를 준비하고 있습니다";
        }
        progress.rectTransform.anchorMax = new Vector2(
            duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f, 1f);
    }

    private void OnRectTransformDimensionsChange() => ResizePanel();

    private void ResizePanel()
    {
        if (!initialized || panelRect == null) return;
        Rect bounds = ((RectTransform)transform).rect;
        Rect authoredSize = panelRect.rect;
        if (authoredSize.width <= 0f || authoredSize.height <= 0f) return;
        // 씬에서 지정한 크기와 위치는 유지하고 작은 화면에 맞게 배율만 조절한다.
        float scale = Mathf.Min(1f, (bounds.width - 40f) / authoredSize.width,
            (bounds.height - 40f) / authoredSize.height);
        panelRect.localScale = Vector3.one * Mathf.Max(0.1f, scale);
    }
}