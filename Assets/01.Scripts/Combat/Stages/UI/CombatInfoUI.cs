using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투는 계속 진행하며, 최종 전투 모듈의 값을 읽어 강화·버프를 함께 표시한다.
[DisallowMultipleComponent]
public sealed class CombatInfoUI : MonoBehaviour
{
    [SerializeField] private PlayercatController player;
    [SerializeField] private PlayerWeaponEquipment equipment;
    [SerializeField] private GameObject root;
    [SerializeField] private GameObject modal;
    [SerializeField] private RectTransform panel;
    [SerializeField] private TMP_Text weaponLabel;
    [SerializeField] private TMP_Text traitLabel;
    [SerializeField] private TMP_Text[] values;
    private UnitHealth health;
    private UnitAttack attack;
    private UnitMove movement;
    private PlayerData subscribedData;
    private bool subscribed;
    private static readonly Color Ink = new Color32(36, 49, 67, 255);
    private static readonly Color Accent = new Color32(37, 157, 163, 255);

    private void Awake()
    {
        if (player == null || equipment == null || root == null || modal == null ||
            panel == null || weaponLabel == null || traitLabel == null ||
            values == null || values.Length != 10 || System.Array.Exists(values, value => value == null))
        {
            Debug.LogError("[CombatInfoUI] 플레이어, 장비와 씬의 전투 정보 UI 참조를 연결하세요.", this);
            enabled = false;
            return;
        }
        Hide();
    }

    public void Show()
    {
        if (!isActiveAndEnabled || modal == null) return;
        if (player.Health == null || player.Attack == null || player.Move == null) return;
        Subscribe();
        modal.SetActive(true);
        RefreshHealth(health.CurrentHp, health.MaxHp);
        RefreshWeapon();
        RefreshAttack();
        RefreshMoveSpeed();
        RefreshSkill();
        FitPanel();
    }

    public void Hide()
    {
        Unsubscribe();
        if (modal != null) modal.SetActive(false);
    }

    private void Subscribe()
    {
        if (subscribed) return;
        health = player.Health;
        attack = player.Attack;
        movement = player.Move;
        subscribedData = GameManager.instance != null ? GameManager.instance.PlayerData : null;
        health.OnHealthChanged += RefreshHealth;
        attack.OnStatsChanged += RefreshAttack;
        movement.OnMoveSpeedChanged += RefreshMoveSpeed;
        equipment.OnWeaponChanged += HandleWeaponChanged;
        equipment.OnSkillChanged += RefreshSkill;
        if (subscribedData != null) subscribedData.OnWeaponsChanged += RefreshWeapon;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        if (health != null) health.OnHealthChanged -= RefreshHealth;
        if (attack != null) attack.OnStatsChanged -= RefreshAttack;
        if (movement != null) movement.OnMoveSpeedChanged -= RefreshMoveSpeed;
        if (equipment != null)
        {
            equipment.OnWeaponChanged -= HandleWeaponChanged;
            equipment.OnSkillChanged -= RefreshSkill;
        }
        if (subscribedData != null) subscribedData.OnWeaponsChanged -= RefreshWeapon;
        subscribedData = null;
        subscribed = false;
    }

    private void RefreshHealth(float current, float max)
    {
        SetTextIfChanged(values[0], $"{System.Math.Round(current, System.MidpointRounding.AwayFromZero):0} / {System.Math.Round(max, System.MidpointRounding.AwayFromZero):0}");
    }

    private void HandleWeaponChanged(PlayerWeaponCatalog.Weapon weapon)
    {
        RefreshWeapon();
        RefreshSkill();
    }

    private void RefreshWeapon()
    {
        PlayerWeaponCatalog.Weapon weapon = equipment.CurrentWeapon;
        PlayerData data = GameManager.instance != null ? GameManager.instance.PlayerData : null;
        SetTextIfChanged(weaponLabel, weapon == null ? "장착 무기 없음"
            : $"{weapon.DisplayName} · {weapon.rarity} · Lv.{(data != null ? data.GetWeaponLevel(weapon.id) : 1)}");
        string traits = weapon != null ? weapon.GetTraitDescription() : "";
        SetTextIfChanged(traitLabel, "무기 특성\n" + (string.IsNullOrEmpty(traits) ? "없음" : traits));
    }

    private void RefreshAttack()
    {
        SetTextIfChanged(values[1], $"{System.Math.Round(attack.AttackDamage, System.MidpointRounding.AwayFromZero):0}");
        SetTextIfChanged(values[2], $"{attack.CriticalChance * 100f:0.#}%");
        SetTextIfChanged(values[3], $"{attack.CriticalDamageMultiplier:0.##}배");
        SetTextIfChanged(values[4], $"초당 {1f / attack.AttackInterval:0.##}회");
        SetTextIfChanged(values[5], $"{attack.AttackInterval:0.###}초");
        SetTextIfChanged(values[6], $"{attack.AttackRange:0.##}");
    }

    private void RefreshMoveSpeed()
    {
        SetTextIfChanged(values[7], $"{movement.MoveSpeed:0.##}");
    }

    private void RefreshSkill()
    {
        SetTextIfChanged(values[8], !equipment.HasSkill ? "없음" : equipment.SkillRemaining > 0f
            ? $"발동 중 · {equipment.SkillRemaining:0.0}초" : "대기");
        SetTextIfChanged(values[9], !equipment.HasSkill ? "—" : equipment.SkillCooldownRemaining > 0f
            ? $"{equipment.SkillCooldownRemaining:0.0}초" : "준비 완료");
        bool buff = equipment.SkillRemaining > 0f;
        for (int i = 0; i < values.Length; i++)
        {
            Color color = buff && (i == 1 || i == 2 || i == 4 || i == 5 || i == 7 || i == 8) ? Accent : Ink;
            if (values[i].color != color)
                values[i].color = color;
        }
    }

    private static void SetTextIfChanged(TMP_Text target, string text)
    {
        if (target != null && target.text != text)
            target.text = text;
    }

    private void FitPanel()
    {
        Rect bounds = ((RectTransform)root.transform).rect;
        float scale = Mathf.Min(1f, (bounds.width - 32f) / panel.rect.width, (bounds.height - 32f) / panel.rect.height);
        panel.localScale = Vector3.one * Mathf.Max(0.1f, scale);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (root != null && panel != null && modal != null && modal.activeSelf)
            FitPanel();
    }

    private void OnEnable() { if (root != null) root.SetActive(true); }
    private void OnDisable() { Hide(); if (root != null) root.SetActive(false); }
}
