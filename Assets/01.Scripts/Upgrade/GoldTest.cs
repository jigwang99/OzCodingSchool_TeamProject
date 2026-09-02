using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldTest : MonoBehaviour
{
    [Header("테스트용 골드 치트")]
    [SerializeField] private Button goldCheatButton;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private int cheatGold = 500;

    private void Start()
    {
        goldCheatButton.onClick.AddListener(AddGold);
    }

    private void OnDestroy()
    {
        goldCheatButton.onClick.RemoveListener(AddGold);
    }

    private void AddGold()
    {
        PlayerData playerData = GameManager.instance.PlayerData;

        CurrencyManager.instance.AddGold(cheatGold);

        goldText.text = $"Gold: {playerData.gold:N0}";
    }
}
