using UnityEngine;
using TMPro;

public class CurrentStageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private StageDataList stageDataList;
    [SerializeField] private string format = "Stage {0}"; // 표시 형식

    private PlayerData boundData;

    private void OnEnable()
    {
        if (stageText == null)
            stageText = GetComponent<TextMeshProUGUI>();

        PlayerData data = GameManager.instance != null ? GameManager.instance.PlayerData : null;
        if (data == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[CurrentStageText] PlayerData가 아직 없습니다. (GameManager 초기화 순서 확인)");
#endif
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

        if (stageDataList != null && stageDataList.IsEndlessStage(boundData.currentStage))
        {
            stageText.text = $"Stage {boundData.currentStage - stageDataList.Count}";
            return;
        }

        // 일반 스테이지는 챕터-스테이지 표시를 유지한다.
        string stageName = stageDataList != null
            ? stageDataList.GetStageName(boundData.currentStage)
            : null;
        stageText.text = string.Format(format,
            string.IsNullOrEmpty(stageName) ? boundData.currentStage.ToString() : stageName);
    }
}
