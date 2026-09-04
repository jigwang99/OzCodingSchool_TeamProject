using UnityEngine;
using TMPro;

public class CurrentStageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private string format = "Stage {0}"; // 표시 형식

    private PlayerData boundData;

    private void OnEnable()
    {
        if (stageText == null)
            stageText = GetComponent<TextMeshProUGUI>();

        PlayerData data = GameManager.instance != null ? GameManager.instance.PlayerData : null;
        if (data == null)
        {
            Debug.LogWarning("[CurrentStageText] PlayerData가 아직 없습니다. (GameManager 초기화 순서 확인)");
            return;
        }

        boundData = data;
        boundData.OnStageChanged += Refresh;
        Refresh(); // 최초 표시
    }

    private void OnDisable()
    {
        if (boundData != null)
        {
            boundData.OnStageChanged -= Refresh;
            boundData = null;
        }
    }

    private void Refresh()
    {
        if (boundData == null || stageText == null)
            return;

        stageText.text = string.Format(format, boundData.currentStage);
    }
}