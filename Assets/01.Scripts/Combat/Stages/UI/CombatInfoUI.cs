using UnityEngine;
using UnityEngine.UI;

// 전투는 계속 진행하며, 최종 전투 모듈의 값을 읽어 강화·버프를 함께 표시한다.
[DisallowMultipleComponent]
public sealed class CombatInfoUI : MonoBehaviour
{
    [SerializeField] private PlayercatController player;
    [SerializeField] private PlayerWeaponEquipment equipment;
    [SerializeField] private Font font;
    private GameObject root;
    private GameObject modal;
    private RectTransform panel;
    private Text weaponLabel;
    private Text traitLabel;
    private Text[] values;
    private float nextRefresh;
    private static readonly Color Ink = new Color32(36, 49, 67, 255);
    private static readonly Color Accent = new Color32(37, 157, 163, 255);
    private static readonly string[] Labels =
    {
        "현재 체력", "공격력", "치명타 확률", "치명타 피해", "공격속도",
        "공격 간격", "공격 사거리", "이동속도", "자동 스킬", "스킬 쿨타임"
    };

    private void Awake()
    {
        if (player == null || equipment == null || font == null)
        {
            Debug.LogError("[CombatInfoUI] 플레이어, 장비, 한글 폰트를 연결하세요.", this);
            enabled = false;
            return;
        }
        root = new GameObject("Combat Info Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Button open = CreateButton(root.transform, "Info", "Info · 전투 정보", Accent);
        RectTransform openRect = (RectTransform)open.transform;
        openRect.anchorMin = openRect.anchorMax = openRect.pivot = Vector2.zero;
        openRect.anchoredPosition = new Vector2(32f, 32f);
        openRect.sizeDelta = new Vector2(210f, 64f);
        open.onClick.AddListener(Show);

        Button backdrop = CreateButton(root.transform, "Backdrop", "", new Color(0.02f, 0.04f, 0.08f, 0.68f));
        Stretch((RectTransform)backdrop.transform);
        backdrop.onClick.AddListener(Hide);
        modal = backdrop.gameObject;
        var body = new GameObject("Info Panel", typeof(RectTransform), typeof(Image), typeof(Button));
        body.transform.SetParent(modal.transform, false);
        panel = (RectTransform)body.transform;
        panel.anchorMin = panel.anchorMax = panel.pivot = Vector2.one * 0.5f;
        panel.sizeDelta = new Vector2(640f, 760f);
        body.GetComponent<Image>().color = new Color32(240, 246, 249, 255);
        Button panelButton = body.GetComponent<Button>();
        panelButton.targetGraphic = body.GetComponent<Image>();
        panelButton.transition = Selectable.Transition.None;
        panelButton.navigation = new Navigation { mode = Navigation.Mode.None };

        Text title = CreateText(panel, "Title", "전투 정보", 32, TextAnchor.MiddleLeft);
        Place(title.rectTransform, 28f, 24f, 470f, 44f);
        Button close = CreateButton(panel, "Close", "닫기", Accent);
        Place((RectTransform)close.transform, 526f, 24f, 86f, 44f);
        close.onClick.AddListener(Hide);
        weaponLabel = CreateText(panel, "Equipped Weapon", "", 24, TextAnchor.MiddleLeft);
        Place(weaponLabel.rectTransform, 28f, 82f, 584f, 38f);
        Text note = CreateText(panel, "Note", "장비·강화·스킬 버프가 반영된 현재 수치입니다.", 18, TextAnchor.MiddleLeft);
        Place(note.rectTransform, 28f, 124f, 584f, 30f);
        note.color = new Color32(98, 111, 125, 255);

        values = new Text[Labels.Length];
        for (int i = 0; i < Labels.Length; i++)
        {
            float y = 176f + i * 44f;
            Text label = CreateText(panel, "Label " + i, Labels[i], 22, TextAnchor.MiddleLeft);
            Place(label.rectTransform, 32f, y, 240f, 36f);
            values[i] = CreateText(panel, "Value " + i, "—", 22, TextAnchor.MiddleRight);
            Place(values[i].rectTransform, 280f, y, 328f, 36f);
        }
        traitLabel = CreateText(panel, "Weapon Traits", "", 20, TextAnchor.UpperLeft);
        Place(traitLabel.rectTransform, 32f, 636f, 576f, 96f);
        modal.SetActive(false);
    }

    private void Show()
    {
        modal.SetActive(true);
        Refresh();
        FitPanel();
        nextRefresh = Time.unscaledTime + 0.1f;
    }

    private void Hide() { if (modal != null) modal.SetActive(false); }

    private void LateUpdate()
    {
        if (modal == null || !modal.activeSelf) return;
        FitPanel();
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.1f;
        Refresh();
    }

    private void Refresh()
    {
        if (player == null || player.Health == null || player.Attack == null || player.Move == null) return;
        PlayerWeaponCatalog.Weapon weapon = equipment.CurrentWeapon;
        PlayerData data = GameManager.instance != null ? GameManager.instance.PlayerData : null;
        weaponLabel.text = weapon == null ? "장착 무기 없음"
            : $"{weapon.DisplayName} · {weapon.rarity} · Lv.{(data != null ? data.GetWeaponLevel(weapon.id) : 1)}";
        UnitAttack attack = player.Attack;
        values[0].text = $"{player.Health.CurrentHp:0.#} / {player.Health.MaxHp:0.#}";
        values[1].text = $"{attack.AttackDamage:0.##}";
        values[2].text = $"{attack.CriticalChance * 100f:0.#}%";
        values[3].text = $"{attack.CriticalDamageMultiplier:0.##}배";
        values[4].text = $"초당 {1f / attack.AttackInterval:0.##}회";
        values[5].text = $"{attack.AttackInterval:0.###}초";
        values[6].text = $"{attack.AttackRange:0.##}";
        values[7].text = $"{player.Move.MoveSpeed:0.##}";
        values[8].text = !equipment.HasSkill ? "없음" : equipment.SkillRemaining > 0f
            ? $"발동 중 · {equipment.SkillRemaining:0.0}초" : "자동 발동 대기";
        values[9].text = !equipment.HasSkill ? "—" : equipment.SkillCooldownRemaining > 0f
            ? $"{equipment.SkillCooldownRemaining:0.0}초" : "준비 완료";
        bool buff = equipment.SkillRemaining > 0f;
        for (int i = 0; i < values.Length; i++)
            values[i].color = buff && (i == 1 || i == 2 || i == 4 || i == 5 || i == 7 || i == 8) ? Accent : Ink;
        string traits = weapon != null ? weapon.GetTraitDescription() : "";
        traitLabel.text = "무기 특성\n" + (string.IsNullOrEmpty(traits) ? "없음" : traits);
    }

    private void FitPanel()
    {
        Rect bounds = ((RectTransform)root.transform).rect;
        float scale = Mathf.Min(1f, (bounds.width - 32f) / 640f, (bounds.height - 32f) / 760f);
        panel.localScale = Vector3.one * Mathf.Max(0.1f, scale);
    }

    private Text CreateText(Transform parent, string name, string text, int size, TextAnchor alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text label = go.GetComponent<Text>();
        label.font = font;
        label.fontSize = size;
        label.text = text;
        label.color = Ink;
        label.alignment = alignment;
        label.raycastTarget = false;
        return label;
    }

    private Button CreateButton(Transform parent, string name, string text, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color;
        Button button = go.GetComponent<Button>();
        button.targetGraphic = image;
        Text label = CreateText(go.transform, "Label", text, 22, TextAnchor.MiddleCenter);
        label.color = Color.white;
        Stretch(label.rectTransform);
        return button;
    }

    private static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void OnEnable() { if (root != null) root.SetActive(true); }
    private void OnDisable() { Hide(); if (root != null) root.SetActive(false); }
    private void OnDestroy() { if (root != null) Destroy(root); }
}
