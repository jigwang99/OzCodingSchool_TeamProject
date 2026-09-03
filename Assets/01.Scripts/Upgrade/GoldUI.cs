using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text goldText;

    private void Start()
    {
        OnGoldChanged();
    }

    private void OnEnable()
    {
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.OnGoldChanged += OnGoldChanged;
    }

    private void OnDisable()
    {
        if (CurrencyManager.instance != null)
            CurrencyManager.instance.OnGoldChanged -= OnGoldChanged;
    }

    private void OnGoldChanged()
    {
        int gold = GameManager.instance.PlayerData.gold;
        goldText.text = $"Gold: {gold:N0}";
    }
}