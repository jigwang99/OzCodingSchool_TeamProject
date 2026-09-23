using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 전투 판정의 수치를 그대로 표시한다. 아직 화면에 등장하지 않은 적도 전체 수에 포함된다.
[DisallowMultipleComponent]
public class RemainingEnemyUI : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private TMP_Text countLabel;

    private void OnEnable()
    {
        if (combatManager == null || countLabel == null)
        {
#if UNITY_EDITOR
            Debug.LogError("[RemainingEnemyUI] CombatManager와 수량 텍스트를 연결하세요.", this);
#endif
            enabled = false;
            return;
        }

        combatManager.OnEnemyCountChanged += Refresh;
        Refresh(combatManager.RemainingEnemyCount, combatManager.TotalEnemyCount);
    }

    private void OnDisable()
    {
        if (combatManager != null)
            combatManager.OnEnemyCountChanged -= Refresh;
    }

    private void Refresh(int remaining, int total)
    {
        countLabel.text = $"남은 적  {remaining:N0} / {total:N0}";
    }
}
