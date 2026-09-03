using TMPro;
using UnityEngine;

// 담당 물고기의 (등급, 종)은 인스펙터에서 명시적으로 지정한다.
// 재고는 CurrencyManager(게이트)에서만 읽고, 변경은 OnFishChanged 구독으로 갱신.
public class FishCountView : MonoBehaviour
{
    [SerializeField] private FishGrade grade;
    [SerializeField, Min(0)] private int species;   // 0-based 종 인덱스
    [SerializeField] private TextMeshProUGUI label;

    private bool isSubscribed;
    private CurrencyManager subscribedCurrencyManager;

    private void Awake()
    {
        if (label == null)
            label = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void OnEnable()
    {
        SubscribeAndRefresh();
    }

    private void Start()
    {
        // 씬 초기화 순서상 CurrencyManager가 늦게 준비되는 경우를 한 번 보완한다.
        SubscribeAndRefresh();
    }

    private void OnDisable()
    {
        // 씬 종료 중 CurrencyManager.instance에 접근하면 싱글턴 getter가 새 오브젝트를
        // 만들 수 있으므로, 구독했던 인스턴스만 직접 해제한다.
        if (isSubscribed && subscribedCurrencyManager != null)
            subscribedCurrencyManager.OnFishChanged -= HandleFishChanged;

        isSubscribed = false;
        subscribedCurrencyManager = null;
    }

    private void SubscribeAndRefresh()
    {
        CurrencyManager currencyManager = CurrencyManager.instance;
        if (currencyManager == null)
            return;

        if (!isSubscribed)
        {
            subscribedCurrencyManager = currencyManager;
            subscribedCurrencyManager.OnFishChanged += HandleFishChanged;
            isSubscribed = true;
        }

        UpdateText();
    }

    // 바뀐 등급이 내 등급일 때만 갱신
    private void HandleFishChanged(FishGrade changedGrade)
    {
        if (changedGrade == grade)
            UpdateText();
    }

    private void UpdateText()
    {
        if (label == null || subscribedCurrencyManager == null)
            return;

        int count = subscribedCurrencyManager.GetFish(grade, species);
        label.text = $"{grade} {species + 1}\n{count}";
    }

    // 기존 버튼 이벤트가 남아 있는 씬에서도 안전하게 동작하도록 유지한 호환용 진입점.
    // 물고기 수량 변경은 OnFishChanged 이벤트가 자동 갱신한다.
    public void GetFish()
    {
        UpdateText();
    }
}
