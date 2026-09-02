using UnityEngine;
using UnityEngine.UI;

public class RetryToggleUI : MonoBehaviour
{
    [SerializeField] private Toggle retryToggle;

    private void OnEnable()
    {
        if (retryToggle == null)
            retryToggle = GetComponent<Toggle>();

        if (GameManager.instance != null && GameManager.instance.PlayerData != null)
        {
            // 데이터의 토글 상태를 UI 스위치에 반영 (패배 시 자동 ON 된 것도 여기서 UI가 켜짐)
            retryToggle.isOn = GameManager.instance.PlayerData.isRetryEnabled;
            retryToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnDisable()
    {
        if (retryToggle != null)
        {
            retryToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (GameManager.instance != null && GameManager.instance.PlayerData != null)
        {
            GameManager.instance.PlayerData.isRetryEnabled = isOn;
        }
    }
}
