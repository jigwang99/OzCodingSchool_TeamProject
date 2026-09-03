using UnityEngine;
using UnityEngine.UI;

public class RetryToggleUI : MonoBehaviour
{
    [SerializeField] private Toggle retryToggle;

    private PlayerData boundData; // 구독한 데이터 인스턴스 추적 (정확한 해제용)

    private void OnEnable()
    {
        if (retryToggle == null)
            retryToggle = GetComponent<Toggle>();

        PlayerData data = GameManager.instance != null ? GameManager.instance.PlayerData : null;
        if (data == null)
        {
            // GameManager 초기화보다 UI가 먼저 뜨는 순서 문제. 조용히 먹통되지 않도록 경고.
            Debug.LogWarning("[RetryToggleUI] PlayerData가 아직 없습니다. (GameManager 초기화 순서 확인)");
            return;
        }

        boundData = data;

        // 사용자 조작 리스너
        retryToggle.onValueChanged.AddListener(OnUserToggled);
        // 데이터가 코드로 바뀔 때(패배 시 자동 ON 등) UI 동기화
        boundData.OnRetryChanged += SyncFromData;

        // 최초 표시값 반영 (콜백 안 튀게 WithoutNotify)
        SyncFromData();
    }

    private void OnDisable()
    {
        if (retryToggle != null)
            retryToggle.onValueChanged.RemoveListener(OnUserToggled);

        if (boundData != null)
        {
            boundData.OnRetryChanged -= SyncFromData;
            boundData = null;
        }
    }

    // 사용자가 직접 토글 → 데이터에 반영 + 저장
    private void OnUserToggled(bool isOn)
    {
        if (boundData == null)
            return;

        boundData.SetRetryEnabled(isOn); // 값이 바뀌면 OnRetryChanged가 울려 SyncFromData가 다시 돌지만,
                                         // SetIsOnWithoutNotify라 콜백이 안 튀어 루프가 안 생김.
        SaveManager.instance?.Save();    // 사용자 의사 결정이므로 즉시 저장
    }

    // 데이터 → UI (프로그래밍 변경/최초 표시 모두 여기로)
    private void SyncFromData()
    {
        if (boundData == null || retryToggle == null)
            return;

        // onValueChanged를 발생시키지 않고 표시만 갱신 → OnUserToggled와 무한 왕복 방지
        retryToggle.SetIsOnWithoutNotify(boundData.isRetryEnabled);
    }
}