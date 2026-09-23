using UnityEngine;
using UnityEngine.UI;

// 장착 무기의 자동 스킬 지속시간과 쿨타임을 표시한다.
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerWeaponEquipment))]
public sealed class WeaponSkillUI : MonoBehaviour
{
    private PlayerWeaponEquipment equipment;
    private GameObject root;
    private Text label;

    private void Awake()
    {
        equipment = GetComponent<PlayerWeaponEquipment>();
        root = new GameObject("Weapon Skill HUD", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var control = new GameObject("Automatic Weapon Skill", typeof(RectTransform), typeof(Image));
        control.transform.SetParent(root.transform, false);
        RectTransform rect = control.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-32f, 32f);
        rect.sizeDelta = new Vector2(260f, 76f);
        control.GetComponent<Image>().color = new Color(0.12f, 0.35f, 0.45f, 0.95f);
        control.GetComponent<Image>().raycastTarget = false;

        var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(control.transform, false);
        label = textObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 24;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        RectTransform textRect = label.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        Refresh();
    }

    private void Update() => Refresh();

    private void Refresh()
    {
        bool visible = equipment != null && equipment.isActiveAndEnabled && equipment.HasSkill;
        root.SetActive(visible);
        if (!visible) return;
        label.text = equipment.SkillRemaining > 0f
            ? $"무기 강화 중 {Mathf.CeilToInt(equipment.SkillRemaining)}초"
            : equipment.SkillCooldownRemaining > 0f
                ? $"스킬 대기 {Mathf.CeilToInt(equipment.SkillCooldownRemaining)}초"
                : "자동 스킬 준비";
    }

    private void OnEnable() { if (root != null) Refresh(); }
    private void OnDisable() { if (root != null) root.SetActive(false); }
    private void OnDestroy() { if (root != null) Destroy(root); }
}
