using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image hpFillImage;
    [SerializeField] private UnitHealth targetUnitHealth;

    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = (RectTransform)transform;
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
            UpdateHPBar(targetUnitHealth.CurrentHp, targetUnitHealth.MaxHp);
        }
    }

    private void OnDisable()
    {
        if (targetUnitHealth != null)
        {
            targetUnitHealth.OnHealthChanged -= UpdateHPBar;
        }
    }

    // 공유 Canvas의 풀에서 대여할 때마다 새로운 체력 대상을 연결한다.
    public void Bind(UnitHealth target)
    {
        Unbind();
        targetUnitHealth = target;
        if (targetUnitHealth == null) return;
        if (isActiveAndEnabled) targetUnitHealth.OnHealthChanged += UpdateHPBar;
        UpdateHPBar(targetUnitHealth.CurrentHp, targetUnitHealth.MaxHp);
    }

    public void Unbind()
    {
        if (targetUnitHealth != null) targetUnitHealth.OnHealthChanged -= UpdateHPBar;
        targetUnitHealth = null;
    }

    private void UpdateHPBar(float current, float max)
    {
        if (hpFillImage != null) hpFillImage.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
    }
}
