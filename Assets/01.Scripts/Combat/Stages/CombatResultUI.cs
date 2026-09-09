using UnityEngine;
using UnityEngine.UI;

// CombatScene의 전용 Canvas에만 배치한다. 진행/재시작은 StageManager가 전담한다.
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public class CombatResultUI : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private CombatRewardTracker rewardTracker;
    [SerializeField] private Font font;
    [SerializeField] private Color clearColor = new Color(1f, 0.82f, 0.39f);
    [SerializeField] private Color failColor = new Color(1f, 0.48f, 0.46f);

    private CanvasGroup panel;
    private RectTransform panelRect;
    private Text stageLabel;
    private Text titleLabel;
    private Text messageLabel;
    private Text rewardLabel;
    private Text countdownLabel;
    private Image accent;
    private Image progress;
    private float duration;
    private float visibleTime;
    private int displayedSeconds = -1;

    private void Awake()
    {
        if (stageManager == null || rewardTracker == null || font == null)
        {
            Debug.LogError("[CombatResultUI] StageManager, CombatRewardTracker와 한글 폰트를 연결하세요.", this);
            enabled = false;
            return;
        }

        BuildPanel();
        Hide();
    }

    private void OnEnable()
    {
        if (stageManager == null || panel == null) return;
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
        if (panel == null || !panel.gameObject.activeSelf) return;

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
        if (panelRect == null) return;
        float canvasWidth = ((RectTransform)transform).rect.width;
        panelRect.sizeDelta = new Vector2(Mathf.Min(640f, Mathf.Max(240f, canvasWidth - 40f)), 304f);
    }

    private void BuildPanel()
    {
        Image background = CreateImage("ResultPanel", transform, new Color(0.075f, 0.10f, 0.16f, 0.96f));
        panelRect = background.rectTransform;
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.64f);
        panel = background.gameObject.AddComponent<CanvasGroup>();
        panel.interactable = false;
        panel.blocksRaycasts = false;

        accent = CreateImage("Accent", panelRect, clearColor);
        SetRect(accent.rectTransform, new Vector2(0f, 1f), Vector2.one,
            new Vector2(0f, -4f), Vector2.zero);

        stageLabel = CreateText("Stage", 20, new Color(0.71f, 0.77f, 0.85f), 14f, 30f);
        titleLabel = CreateText("Title", 42, clearColor, 47f, 55f);
        messageLabel = CreateText("Message", 23, new Color(0.94f, 0.95f, 0.98f), 108f, 70f);
        rewardLabel = CreateText("Rewards", 23, new Color(0.75f, 0.90f, 0.85f), 188f, 36f);
        countdownLabel = CreateText("Countdown", 19, new Color(0.71f, 0.77f, 0.85f), 234f, 30f);

        Image track = CreateImage("CountdownTrack", panelRect, new Color(1f, 1f, 1f, 0.12f));
        SetRect(track.rectTransform, Vector2.zero, Vector2.right,
            new Vector2(28f, 22f), new Vector2(-28f, 26f));
        progress = CreateImage("Remaining", track.transform, clearColor);
        SetRect(progress.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        ResizePanel();
    }

    private Text CreateText(string objectName, int size, Color color, float top, float height)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        obj.layer = gameObject.layer;
        var text = obj.GetComponent<Text>();
        text.rectTransform.SetParent(panelRect, false);
        SetRect(text.rectTransform, Vector2.up, Vector2.one,
            new Vector2(24f, -top - height), new Vector2(-24f, -top));
        // TTF를 직접 사용해 기존 공용 TMP 비트맵 폰트를 수정하지 않고 한글을 표시한다.
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 14;
        text.resizeTextMaxSize = size;
        text.supportRichText = false;
        text.raycastTarget = false;
        return text;
    }

    private Image CreateImage(string objectName, Transform parent, Color color)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        obj.layer = gameObject.layer;
        var image = obj.GetComponent<Image>();
        image.rectTransform.SetParent(parent, false);
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
