using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// CombatScene 전용. 열려 있는 동안 전투는 계속되며, 선택 시 StageManager가 새 전투를 시작한다.
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public class StageSelectUI : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private Font font;

    private readonly List<Button> stageButtons = new List<Button>();
    private readonly List<Text> stageLabels = new List<Text>();
    private GameObject modal;
    private RectTransform panel;
    private Button openButton;
    private Text hint;

    private static readonly Color NormalColor = new Color(0.18f, 0.23f, 0.32f);
    private static readonly Color BossColor = new Color(0.38f, 0.27f, 0.15f);
    private static readonly Color CurrentColor = new Color(0.13f, 0.46f, 0.48f);
    private const string SelectionHint = "번호를 누르면 해당 스테이지에서 전투를 새로 시작합니다.";

    private void Awake()
    {
        if (stageManager == null || font == null || stageManager.StageCount == 0)
        {
            Debug.LogError("[StageSelectUI] StageManager, 스테이지 데이터와 한글 폰트를 확인하세요.", this);
            enabled = false;
            return;
        }

        BuildUI();
        Hide();
    }

    private void OnEnable()
    {
        if (modal == null) return;
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
        if (modal == null) return;
        hint.text = SelectionHint;
        modal.SetActive(true);
        ResizePanel();
        Refresh();
    }

    public void Hide()
    {
        if (modal != null) modal.SetActive(false);
    }

    private void Select(int stageNumber)
    {
        if (stageManager.SelectStage(stageNumber))
            Hide();
        else
            hint.text = "스테이지를 시작할 수 없습니다. 잠시 후 다시 선택해 주세요.";
    }

    private void HandleResult(StageResult _) => Refresh();

    private void Refresh()
    {
        for (int i = 0; i < stageButtons.Count; i++)
        {
            int number = i + 1;
            bool current = number == stageManager.CurrentStageNumber;
            bool boss = number % 5 == 0;
            stageButtons[i].image.color = current ? CurrentColor : boss ? BossColor : NormalColor;
            string status = boss ? (current ? "보스 · 현재" : "보스") : (current ? "현재" : "일반");
            stageLabels[i].text = stageManager.GetStageName(number) + "\n" + status;
        }
    }

    private void BuildUI()
    {
        openButton = CreateButton("OpenStageSelect", transform, "스테이지 선택", 20, Show);
        var openRect = (RectTransform)openButton.transform;
        openRect.anchorMin = openRect.anchorMax = new Vector2(0.5f, 1f);
        openRect.anchoredPosition = new Vector2(190f, -65f);
        openRect.sizeDelta = new Vector2(170f, 44f);

        Image backdrop = CreateImage("StageSelectModal", transform, new Color(0f, 0f, 0f, 0.6f));
        modal = backdrop.gameObject;
        SetRect(backdrop.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        // 패널 바깥 클릭은 닫기로 처리하고 뒤쪽 전투 UI에 전달하지 않는다.
        Button dismiss = backdrop.gameObject.AddComponent<Button>();
        dismiss.targetGraphic = backdrop;
        dismiss.transition = Selectable.Transition.None;
        dismiss.navigation = new Navigation { mode = Navigation.Mode.None };
        dismiss.onClick.AddListener(Hide);

        Image card = CreateImage("StageSelectPanel", modal.transform, new Color(0.075f, 0.10f, 0.16f, 1f));
        panel = card.rectTransform;
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        // 공백 영역도 클릭을 소비해 패널 바깥 닫기로 전파하지 않는다.
        Button panelSurface = card.gameObject.AddComponent<Button>();
        panelSurface.targetGraphic = card;
        panelSurface.transition = Selectable.Transition.None;
        panelSurface.navigation = new Navigation { mode = Navigation.Mode.None };

        Text title = CreateText("Title", panel, "스테이지 선택", 32);
        TopRect(title.rectTransform, 24f, 75f, 16f, 44f);
        title.alignment = TextAnchor.MiddleLeft;
        Button close = CreateButton("Close", panel, "닫기", 18, Hide);
        var closeRect = (RectTransform)close.transform;
        closeRect.anchorMin = closeRect.anchorMax = Vector2.one;
        closeRect.anchoredPosition = new Vector2(-50f, -38f);
        closeRect.sizeDelta = new Vector2(64f, 36f);

        int rows = Mathf.CeilToInt(stageManager.StageCount / 5f);
        for (int row = 0; row < rows; row++)
        {
            float rowTop = 74f + row * 116f;
            Text chapter = CreateText("Chapter" + (row + 1), panel, (row + 1) + "장", 22);
            chapter.alignment = TextAnchor.MiddleLeft;
            TopRect(chapter.rectTransform, 24f, 24f, rowTop, 28f);

            var rowObject = new GameObject("Stages" + (row + 1), typeof(RectTransform));
            rowObject.layer = gameObject.layer;
            var rowRect = (RectTransform)rowObject.transform;
            rowRect.SetParent(panel, false);
            TopRect(rowRect, 20f, 20f, rowTop + 32f, 70f);

            for (int column = 0; column < 5; column++)
            {
                int stageNumber = row * 5 + column + 1;
                if (stageNumber > stageManager.StageCount) break;
                Button button = CreateButton("Stage" + stageNumber, rowRect, "", 22, () => Select(stageNumber));
                SetRect((RectTransform)button.transform,
                    new Vector2(column / 5f, 0f), new Vector2((column + 1) / 5f, 1f),
                    new Vector2(4f, 0f), new Vector2(-4f, 0f));
                stageButtons.Add(button);
                stageLabels.Add(button.GetComponentInChildren<Text>());
            }
        }

        hint = CreateText("Hint", panel, SelectionHint, 17);
        TopRect(hint.rectTransform, 24f, 24f, 80f + rows * 116f, 40f);
        ResizePanel();
    }

    private void OnRectTransformDimensionsChange() => ResizePanel();

    private void ResizePanel()
    {
        if (panel == null) return;
        Rect bounds = ((RectTransform)transform).rect;
        float height = 140f + Mathf.CeilToInt(stageManager.StageCount / 5f) * 116f;
        panel.sizeDelta = new Vector2(Mathf.Min(700f, Mathf.Max(280f, bounds.width - 32f)), height);
        panel.localScale = Vector3.one * Mathf.Min(1f, Mathf.Max(0.1f, (bounds.height - 32f) / height));
    }

    private Button CreateButton(string objectName, Transform parent, string label, int size, UnityAction clicked)
    {
        Image image = CreateImage(objectName, parent, NormalColor);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick.AddListener(clicked);
        Text text = CreateText("Label", image.transform, label, size);
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
        return button;
    }

    private Image CreateImage(string objectName, Transform parent, Color color)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        obj.layer = gameObject.layer;
        var image = obj.GetComponent<Image>();
        image.rectTransform.SetParent(parent, false);
        image.color = color;
        return image;
    }

    private Text CreateText(string objectName, Transform parent, string value, int size)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        obj.layer = gameObject.layer;
        Text text = obj.GetComponent<Text>();
        text.rectTransform.SetParent(parent, false);
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 12;
        text.resizeTextMaxSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        text.supportRichText = false;
        return text;
    }

    private static void TopRect(RectTransform rect, float left, float right, float top, float height)
    {
        SetRect(rect, Vector2.up, Vector2.one, new Vector2(left, -top - height), new Vector2(-right, -top));
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
