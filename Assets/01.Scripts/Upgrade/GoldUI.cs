using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text goldText;

    private void Start()
    {
        OnGoldChanged(GameManager.instance.PlayerData.gold);
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

    private void OnGoldChanged(BigNumber gold)
    {
        goldText.text = $"{gold}";
    }
}