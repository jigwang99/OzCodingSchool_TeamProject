using UnityEngine;
using UnityEngine.UI;

// CombatScene의 전용 Canvas에만 배치한다. 진행/재시작은 StageManager가 전담한다.
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public class CombatResultUI : MonoBehaviour
{
    [System.Serializable]
    private sealed class RewardSlot
    {
        public FishGrade grade;
        [Min(0)] public int species;
        public CanvasGroup group;
        public Text countLabel;
    }

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
    [Header("이번 전투 드롭 (Business 인벤토리 순서)")]
    [SerializeField] private RewardSlot[] rewardSlots;
    [Header("드롭 목록 레이아웃")]
    [SerializeField, Min(1)] private int rewardColumns = 4;
    [SerializeField, Min(1f)] private float rewardRowHeight = 88f;
    [Tooltip("목록 아래의 등급 안내, 메시지, 카운트다운, 진행 바")]
    [SerializeField] private RectTransform[] rewardFooter;
    private Vector2[] slotPositions;
    private Vector2[] footerPositions;
    private float fullPanelHeight;
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

        CacheRewardLayout();
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
        RefreshRewardSlots();
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

    private void RefreshRewardSlots()
    {
        if (rewardSlots == null) return;

        int visibleCount = 0;
        foreach (RewardSlot slot in rewardSlots)
        {
            if (slot == null || slot.countLabel == null || slot.group == null) continue;
            long count = rewardTracker.GetFishCount(slot.grade, slot.species);
            slot.group.gameObject.SetActive(count > 0);
            if (count <= 0) continue;

            slot.countLabel.text = $"+{count:N0}";
            slot.group.alpha = 1f;
            // 매 결과마다 원래 슬롯 위치를 앞에서부터 채워 이전 전투의 빈칸을 남기지 않는다.
            ((RectTransform)slot.group.transform).anchoredPosition = slotPositions[visibleCount++];
        }

        int columns = Mathf.Max(1, rewardColumns);
        int fullRows = Mathf.CeilToInt((float)rewardSlots.Length / columns);
        int visibleRows = Mathf.CeilToInt((float)visibleCount / columns);
        float removedHeight = (fullRows - visibleRows) * rewardRowHeight;
        panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fullPanelHeight - removedHeight);
        for (int i = 0; i < footerPositions.Length; i++)
        {
            if (rewardFooter[i] != null)
                rewardFooter[i].anchoredPosition = footerPositions[i] + Vector2.up * removedHeight;
        }

        if (rewardTracker.TotalFishCount == 0)
            rewardLabel.text = "이번 전투에서 획득한 물고기가 없습니다";
    }

    private void CacheRewardLayout()
    {
        // 첫 결과 표시 전에 한 번만 저장해 결과창을 반복해 열어도 높이와 위치가 누적되지 않는다.
        fullPanelHeight = panelRect.rect.height;
        slotPositions = new Vector2[rewardSlots?.Length ?? 0];
        for (int i = 0; i < slotPositions.Length; i++)
        {
            if (rewardSlots[i]?.group != null)
                slotPositions[i] = ((RectTransform)rewardSlots[i].group.transform).anchoredPosition;
        }

        footerPositions = new Vector2[rewardFooter?.Length ?? 0];
        for (int i = 0; i < footerPositions.Length; i++)
        {
            if (rewardFooter[i] != null)
                footerPositions[i] = rewardFooter[i].anchoredPosition;
        }
    }
}
