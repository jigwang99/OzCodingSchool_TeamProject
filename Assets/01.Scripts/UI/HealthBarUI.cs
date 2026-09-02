using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image hpFillImage;
    [SerializeField] private UnitHealth targetUnitHealth;

    private void Awake()
    {
        if (targetUnitHealth == null)
        {
            targetUnitHealth = GetComponentInParent<UnitHealth>();
        }
    }
    private void OnEnable()
    {
        if (targetUnitHealth != null)
        {
            targetUnitHealth.OnHealthChanged += UpdateHPBar;
        }
    }

    private void OnDisable()
    {
        if (targetUnitHealth != null)
        {
            targetUnitHealth.OnHealthChanged -= UpdateHPBar;
        }
    }

    private void LateUpdate()
    {
        if (targetUnitHealth != null && targetUnitHealth.MaxHp > 0)
        {
            UpdateHPBar(targetUnitHealth.CurrentHp, targetUnitHealth.MaxHp);
        }
    }

    private void UpdateHPBar(float current, float max)
    {
        if (max > 0)
        {
            hpFillImage.fillAmount = current / max;
        }
    }
}
