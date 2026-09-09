using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerWeaponEquipment))]
public class PlayerWeaponEquipmentEditor : Editor
{
    private int selectedIndex;
    private UpgradeData weaponUpgrade;

    private void OnEnable()
    {
        weaponUpgrade = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/01.Scripts/Upgrade/WeaponData.asset");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var equipment = (PlayerWeaponEquipment)target;
        var catalog = equipment.Catalog;
        if (catalog == null || catalog.Weapons.Count == 0)
            return;

        bool canTest = Application.isPlaying && equipment.isActiveAndEnabled && equipment.gameObject.scene.IsValid();
        PlayerData data = canTest ? GameManager.instance.PlayerData : null;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("무기 획득 / 장착 테스트", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("현재 무기", equipment.CurrentWeapon?.id ?? "미장착");
        if (data != null && equipment.CurrentWeapon != null)
            EditorGUILayout.LabelField("현재 무기 레벨", data.GetWeaponLevel(equipment.CurrentWeapon.id).ToString());
        var attack = equipment.GetComponent<UnitAttack>();
        if (attack != null)
            EditorGUILayout.LabelField("현재 공격력", attack.AttackDamage.ToString());

        string[] options = new string[catalog.Weapons.Count];
        for (int i = 0; i < options.Length; i++)
        {
            string id = catalog.Weapons[i]?.id;
            OwnedWeaponData owned = data?.GetOwnedWeapon(id);
            options[i] = id == null ? "(비어 있음)" : owned == null ? $"{id} / 미보유" : $"{id} / Lv.{owned.level} / {owned.count}개";
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
        selectedIndex = EditorGUILayout.Popup("테스트할 무기", selectedIndex, options);
        string selectedId = catalog.Weapons[selectedIndex]?.id;
        using (new EditorGUI.DisabledScope(!canTest || string.IsNullOrEmpty(selectedId)))
        {
            if (GUILayout.Button("선택한 무기 1개 획득"))
                equipment.AcquireWeapon(selectedId);

            using (new EditorGUI.DisabledScope(data == null || !data.OwnsWeapon(selectedId)))
            {
                if (GUILayout.Button("선택한 무기 장착"))
                    equipment.EquipWeapon(selectedId);
            }
        }

        EditorGUILayout.Space();
        weaponUpgrade = (UpgradeData)EditorGUILayout.ObjectField("무기 강화 설정", weaponUpgrade, typeof(UpgradeData), false);
        bool canUpgrade = data != null && data.OwnsWeapon(data.equippedWeaponId)
            && weaponUpgrade != null && weaponUpgrade.type == UpgradeType.WeaponPower;
        if (canUpgrade)
        {
            int level = data.GetWeaponLevel(data.equippedWeaponId);
            BigNumber cost = UpgradeManager.instance.GetUpgradeCost(weaponUpgrade, level);
            EditorGUILayout.LabelField("강화 비용", level >= weaponUpgrade.maxLevel ? "MAX" : $"{cost} G");
            canUpgrade = level < weaponUpgrade.maxLevel;
        }
        using (new EditorGUI.DisabledScope(!canTest || !canUpgrade))
        {
            if (GUILayout.Button("장착 중인 무기 강화 (골드 사용)"))
                UpgradeManager.instance.TryUpgrade(weaponUpgrade, data);
        }
        EditorGUILayout.HelpBox("미보유 무기는 먼저 획득하세요. 중복 획득은 수량만 증가하며 강화 레벨은 무기별로 유지됩니다. 변경 사항은 기존 자동 저장에 포함됩니다.", MessageType.Info);
    }
}
