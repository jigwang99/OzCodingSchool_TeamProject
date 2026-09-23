using UnityEngine;
using UnityEngine.UI;

// 장착 무기 아이콘과 시계 방향 쿨타임 표시. 장비/스킬 변경 이벤트에서 갱신한다.
[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerWeaponEquipment))]
public sealed class WeaponSkillUI : MonoBehaviour
{
    private PlayerWeaponEquipment equipment;
    private GameObject root;
    private Image frame;
    private Image icon;
    private Image cooldownFill;
    private Text countdown;
    private Sprite fillSprite;
    private int displayedSeconds = -1;

    private void Awake()
    {
        equipment = GetComponent<PlayerWeaponEquipment>();
        root = new GameObject("Weapon Skill HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        frame = CreateImage(root.transform, "Automatic Weapon Skill");
        RectTransform rect = frame.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-32f, 32f);
        rect.sizeDelta = new Vector2(112f, 112f);

        Image background = CreateImage(frame.transform, "Background");
        Stretch(background.rectTransform, 4f);
        background.color = new Color(0.04f, 0.08f, 0.14f, 0.95f);
        icon = CreateImage(background.transform, "Weapon Icon");
        Stretch(icon.rectTransform, 10f);
        icon.preserveAspect = true;

        // Filled Image는 스프라이트가 있어야 원형 채움 메시를 생성한다.
        fillSprite = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * 0.5f);
        cooldownFill = CreateImage(background.transform, "Cooldown Fill");
        Stretch(cooldownFill.rectTransform, 0f);
        cooldownFill.sprite = fillSprite;
        cooldownFill.color = new Color(0.08f, 0.42f, 1f, 0.65f);
        cooldownFill.type = Image.Type.Filled;
        cooldownFill.fillMethod = Image.FillMethod.Radial360;
        cooldownFill.fillOrigin = (int)Image.Origin360.Top;
        cooldownFill.fillClockwise = true;

        var textObject = new GameObject("Cooldown Seconds", typeof(RectTransform), typeof(Text), typeof(Outline));
        textObject.transform.SetParent(background.transform, false);
        countdown = textObject.GetComponent<Text>();
        countdown.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        countdown.fontSize = 36;
        countdown.fontStyle = FontStyle.Bold;
        countdown.alignment = TextAnchor.MiddleCenter;
        countdown.color = Color.white;
        countdown.raycastTarget = false;
        Stretch(countdown.rectTransform, 0f);
        textObject.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.9f);
    }

    private void HandleWeaponChanged(PlayerWeaponCatalog.Weapon weapon)
    {
        icon.sprite = weapon != null ? weapon.GetSprite() : null;
        icon.enabled = icon.sprite != null;
        Refresh();
    }

    private void Refresh()
    {
        bool visible = equipment != null && equipment.isActiveAndEnabled && equipment.HasSkill;
        if (root.activeSelf != visible) root.SetActive(visible);
        if (!visible) return;
        float remaining = equipment.SkillCooldownRemaining;
        bool coolingDown = remaining > 0f;
        cooldownFill.enabled = coolingDown;
        // 위쪽에서 시계 방향으로 차오르고, 준비 완료 시 아이콘만 표시한다.
        cooldownFill.fillAmount = coolingDown
            ? 1f - Mathf.Clamp01(remaining / Mathf.Max(0.1f, equipment.SkillCooldownDuration)) : 0f;
        int seconds = coolingDown ? Mathf.CeilToInt(remaining) : 0;
        if (seconds != displayedSeconds)
        {
            displayedSeconds = seconds;
            countdown.text = seconds > 0 ? seconds.ToString() : "";
        }
        frame.color = equipment.SkillRemaining > 0f
            ? new Color(0.25f, 0.95f, 0.7f) : new Color(0.2f, 0.5f, 0.75f);
    }

    private static Image CreateImage(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private static void Stretch(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * padding;
        rect.offsetMax = Vector2.one * -padding;
    }

    private void OnEnable()
    {
        equipment.OnWeaponChanged += HandleWeaponChanged;
        equipment.OnSkillChanged += Refresh;
        HandleWeaponChanged(equipment.CurrentWeapon);
    }

    private void OnDisable()
    {
        if (equipment != null)
        {
            equipment.OnWeaponChanged -= HandleWeaponChanged;
            equipment.OnSkillChanged -= Refresh;
        }
        if (root != null) root.SetActive(false);
    }

    private void OnDestroy()
    {
        if (root != null) Destroy(root);
        if (fillSprite != null) Destroy(fillSprite);
    }
}
