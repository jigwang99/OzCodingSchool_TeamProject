using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 전투 씬의 무기 목록. 보유 수량/강화 수치는 PlayerData만 읽고 장착은 Equipment에 위임한다.
[DefaultExecutionOrder(200)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public class WeaponInventoryUI : MonoBehaviour
{
    [Serializable]
    private sealed class Slot
    {
        public string weaponId;
        public Image border, background, icon, countBackground;
        public Text level, count, badge;
        [NonSerialized] public PlayerWeaponCatalog.Weapon weapon;
    }

    [Header("데이터")]
    [SerializeField] private PlayerWeaponEquipment equipment;
    [SerializeField] private Material grayscaleMaterial;

    [Header("씬에 배치된 UI")]
    [SerializeField] private GameObject modal;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button openButton, equipButton;
    [SerializeField] private Text collectionLabel, detailLabel, equipLabel;
    [SerializeField] private List<Slot> slots = new List<Slot>();

    [Header("상태별 색상")]
    [SerializeField] private Color textColor = new Color32(49, 60, 74, 255);
    [SerializeField] private Color selectedColor = new Color32(27, 166, 178, 255);
    [SerializeField] private Color equippedBorder = new Color32(89, 195, 203, 255);
    [SerializeField] private Color ownedBorder = new Color32(151, 176, 205, 255);
    [SerializeField] private Color lockedBorder = new Color32(154, 160, 166, 255);
    [SerializeField] private Color ownedBackground = new Color32(238, 246, 255, 255);
    [SerializeField] private Color lockedBackground = new Color32(205, 208, 211, 255);
    [SerializeField] private Color lockedIconColor = new Color(0.65f, 0.65f, 0.65f, 0.85f);
    [SerializeField] private Color lockedTextColor = new Color32(115, 120, 125, 255);
    [SerializeField] private Color badgeColor = new Color32(119, 130, 145, 255);
    [SerializeField] private Color ownedCountBackground = new Color32(59, 105, 118, 255);
    [SerializeField] private Color lockedCountBackground = new Color32(115, 121, 128, 255);

    private PlayerData subscribedData;
    private string selectedId;
    private bool initialized;

    private void Awake()
    {
        if (equipment == null || equipment.Catalog == null || grayscaleMaterial == null ||
            modal == null || panel == null || openButton == null || equipButton == null ||
            collectionLabel == null || detailLabel == null || equipLabel == null || slots.Count == 0)
        {
            Debug.LogError("[WeaponInventoryUI] 씬의 UI 및 데이터 참조를 연결하세요.", this);
            enabled = false;
            return;
        }

        foreach (Slot slot in slots)
        {
            if (slot == null || slot.border == null || slot.background == null || slot.icon == null ||
                slot.countBackground == null || slot.level == null || slot.count == null || slot.badge == null ||
                (slot.weapon = equipment.Catalog.Find(slot.weaponId)) == null)
            {
                Debug.LogError("[WeaponInventoryUI] 슬롯의 무기 ID 또는 UI 참조를 확인하세요.", this);
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
        equipment.OnWeaponChanged += HandleEquipmentChanged;
        openButton.interactable = true;
        BindData();
        Refresh();
    }

    private void Start()
    {
        // GameManager/Equipment의 시작 시 저장 데이터 복원이 끝난 상태로 표시한다.
        if (!initialized) return;
        BindData();
        Refresh();
    }

    private void OnDisable()
    {
        if (equipment != null) equipment.OnWeaponChanged -= HandleEquipmentChanged;
        if (subscribedData != null) subscribedData.OnWeaponsChanged -= Refresh;
        subscribedData = null;
        if (openButton != null) openButton.interactable = false;
        Hide();
    }

    private void BindData()
    {
        PlayerData data = GameManager.instance.PlayerData;
        if (data == subscribedData) return;
        if (subscribedData != null) subscribedData.OnWeaponsChanged -= Refresh;
        subscribedData = data;
        if (subscribedData != null) subscribedData.OnWeaponsChanged += Refresh;
    }

    public void Show()
    {
        if (!initialized || !isActiveAndEnabled) return;
        BindData();
        selectedId = subscribedData?.equippedWeaponId;
        Refresh();
        modal.SetActive(true);
        ResizePanel();
    }

    public void Hide()
    {
        if (modal != null) modal.SetActive(false);
    }

    // Button.onClick은 씬에서 연결한다. 슬롯 버튼은 해당 weaponId를 문자열 인자로 전달한다.
    public void SelectWeapon(string id)
    {
        if (!initialized || !isActiveAndEnabled) return;
        selectedId = id;
        Refresh();
    }

    public void EquipSelected()
    {
        if (!initialized || !isActiveAndEnabled || subscribedData == null || !subscribedData.OwnsWeapon(selectedId)) return;
        if (!equipment.EquipWeapon(selectedId))
        {
            detailLabel.text = "무기를 장착할 수 없습니다.";
            return;
        }
        SaveManager.instance.Save();
        Refresh();
    }

    private void HandleEquipmentChanged(PlayerWeaponCatalog.Weapon _) => Refresh();

    private void Refresh()
    {
        if (subscribedData == null || collectionLabel == null) return;
        int ownedTypes = 0;
        Slot selected = null;
        for (int i = 0; i < slots.Count; i++)
        {
            Slot slot = slots[i];
            OwnedWeaponData owned = subscribedData.GetOwnedWeapon(slot.weapon.id);
            bool acquired = owned != null;
            bool equipped = acquired && subscribedData.equippedWeaponId == slot.weapon.id;
            bool isSelected = selectedId == slot.weapon.id;
            if (acquired) ownedTypes++;
            if (isSelected) selected = slot;

            slot.border.color = isSelected ? selectedColor : equipped ? equippedBorder
                : acquired ? ownedBorder : lockedBorder;
            slot.background.color = acquired ? ownedBackground : lockedBackground;
            slot.icon.material = acquired ? null : grayscaleMaterial;
            slot.icon.color = acquired ? Color.white : lockedIconColor;
            slot.level.text = acquired ? "+" + subscribedData.GetWeaponLevel(slot.weapon.id) : "+0";
            slot.level.color = acquired ? textColor : lockedTextColor;
            slot.badge.text = equipped ? "장착" : (i + 1).ToString("00");
            slot.badge.color = equipped ? selectedColor : badgeColor;
            slot.count.text = "획득 " + (owned?.count ?? 0).ToString("N0") + "개";
            slot.countBackground.color = acquired ? ownedCountBackground : lockedCountBackground;
        }

        collectionLabel.text = "보유 " + ownedTypes + " / " + slots.Count;
        bool canEquip = selected != null && subscribedData.OwnsWeapon(selectedId);
        bool alreadyEquipped = canEquip && subscribedData.equippedWeaponId == selectedId;
        equipButton.interactable = canEquip && !alreadyEquipped;
        equipLabel.text = alreadyEquipped ? "장착 중" : canEquip ? "장착하기" : "미획득";
        if (selected == null)
            detailLabel.text = "무기를 선택해 주세요.";
        else
        {
            string name = "무기 " + (slots.IndexOf(selected) + 1).ToString("00");
            detailLabel.text = canEquip
                ? name + "  ·  공격력 " + selected.weapon.GetDamage(subscribedData.GetWeaponLevel(selectedId)).ToString("0.##")
                : name + "  ·  아직 획득하지 못했습니다.";
        }
    }

    private void OnRectTransformDimensionsChange() => ResizePanel();

    private void ResizePanel()
    {
        if (!initialized || panel == null) return;
        Rect bounds = ((RectTransform)transform).rect;
        Rect authoredSize = panel.rect;
        if (authoredSize.width <= 0f || authoredSize.height <= 0f) return;
        // Inspector에서 지정한 패널 크기는 유지하고 작은 화면에 맞게 배율만 조절한다.
        float scale = Mathf.Min(1f, (bounds.width - 24f) / authoredSize.width,
            (bounds.height - 24f) / authoredSize.height);
        panel.localScale = Vector3.one * Mathf.Max(0.1f, scale);
    }
}