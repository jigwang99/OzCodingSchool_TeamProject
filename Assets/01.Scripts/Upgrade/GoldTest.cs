using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoldTest : MonoBehaviour
{
    [Header("테스트용 골드 치트")]
    [SerializeField] private Button goldCheatButton;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private double cheatGold = 500;

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

        CurrencyManager.instance.AddGold(new BigNumber(cheatGold));

        BigNumber gold = GameManager.instance.PlayerData.gold;
        goldText.text = $"{gold}";
    }
}
