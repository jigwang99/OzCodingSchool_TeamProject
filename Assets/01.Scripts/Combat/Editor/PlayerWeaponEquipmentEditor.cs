using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerWeaponEquipment))]
public class PlayerWeaponEquipmentEditor : Editor
{
    private int selectedIndex;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var equipment = (PlayerWeaponEquipment)target;
        var catalog = equipment.Catalog;
        if (catalog == null || catalog.Weapons.Count == 0)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("무기 장착 테스트", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("현재 무기", equipment.CurrentWeapon?.id ?? "미장착");
        var attack = equipment.GetComponent<UnitAttack>();
        if (attack != null)
            EditorGUILayout.LabelField("현재 공격력", attack.AttackDamage.ToString());

        string[] options = new string[catalog.Weapons.Count];
        for (int i = 0; i < options.Length; i++)
            options[i] = catalog.Weapons[i]?.id ?? "(비어 있음)";

        selectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
        selectedIndex = EditorGUILayout.Popup("장착할 무기", selectedIndex, options);
        using (new EditorGUI.DisabledScope(!Application.isPlaying || !equipment.isActiveAndEnabled))
        {
            if (GUILayout.Button("선택한 무기 장착"))
                equipment.EquipWeapon(options[selectedIndex]);
        }
        EditorGUILayout.HelpBox("Play 모드에서 외형과 공격력 변경을 확인하세요. 장착한 ID는 기존 자동 저장에 포함됩니다.", MessageType.Info);
    }
}
