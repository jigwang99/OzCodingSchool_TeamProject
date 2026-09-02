using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;
using PixelRestaurant.Managers;

/// <summary>
/// 가챠 UI 컨트롤러
/// UI 버튼 이벤트 처리 및 화면 관리
/// </summary>
public class GachaUIController : MonoBehaviour
{
    // 팝업 관련
    [SerializeField] private Button gachaOpenButton;
    [SerializeField] private GameObject gachaPopup;
    [SerializeField] private Button gachaCloseButton;

    // 콘텐츠 관련
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private GachaPool gachaPool;

    // 가챠 타입 버튼
    [SerializeField] private Button weaponButton;
    [SerializeField] private Button furnitureButton;
    [SerializeField] private Button foodButton;

    // 뽑기 버튼
    [SerializeField] private Button pull1Button;
    [SerializeField] private Button pull5Button;
    [SerializeField] private Button pull30Button;

    // UI 표시
    [SerializeField] private TextMeshProUGUI goldDisplay;
    [SerializeField] private TextMeshProUGUI currentGachaTypeDisplay;

    // 현재 선택된 가챠 타입
    private GachaGroup currentGachaType = GachaGroup.Weapon;

    private void Start()
    {
        InitializeUI();
        RegisterButtonEvents();
        InitializeGame();
    }

    private void Update()
    {
        // 매 프레임 골드 업데이트
        UpdateGoldDisplay();
    }

    /// <summary>
    /// UI 컴포넌트 초기화
    /// </summary>
    private void InitializeUI()
    {
        Debug.Log($"gachaOpenButton: {(gachaOpenButton != null ? "OK" : "NULL")}");
        Debug.Log($"gachaPopup: {(gachaPopup != null ? "OK" : "NULL")}");
        Debug.Log($"gachaCloseButton: {(gachaCloseButton != null ? "OK" : "NULL")}");
        Debug.Log($"scrollViewContent: {(scrollViewContent != null ? "OK" : "NULL")}");
        Debug.Log($"gachaPool: {(gachaPool != null ? "OK" : "NULL")}");
        Debug.Log($"goldDisplay: {(goldDisplay != null ? "OK" : "NULL")}");

        // 팝업 초기 상태: 닫혀있음
        if (gachaPopup != null)
            gachaPopup.SetActive(false);
    }

    /// <summary>
    /// 버튼 이벤트 등록
    /// </summary>
    private void RegisterButtonEvents()
    {
        // 팝업 버튼
        if (gachaOpenButton != null)
            gachaOpenButton.onClick.AddListener(OpenGachaPopup);
        if (gachaCloseButton != null)
            gachaCloseButton.onClick.AddListener(CloseGachaPopup);

        // 가챠 타입 버튼
        if (weaponButton != null)
            weaponButton.onClick.AddListener(() => SelectGachaType(GachaGroup.Weapon));
        if (furnitureButton != null)
            furnitureButton.onClick.AddListener(() => SelectGachaType(GachaGroup.Furniture));
        if (foodButton != null)
            foodButton.onClick.AddListener(() => SelectGachaType(GachaGroup.Recipe));

        // 뽑기 버튼
        if (pull1Button != null)
            pull1Button.onClick.AddListener(() => ExecuteGacha(1));
        if (pull5Button != null)
            pull5Button.onClick.AddListener(() => ExecuteGacha(5));
        if (pull30Button != null)
            pull30Button.onClick.AddListener(() => ExecuteGacha(30));
    }

    /// <summary>
    /// 게임 초기화 (골드 추가 등)
    /// </summary>
    private void InitializeGame()
    {
        if (GameManager.instance.PlayerData == null)
        {
            GameManager.instance.CreateNewPlayerData();
        }

        // 테스트용 초기 골드 추가
        //GoldManager.instance.AddGold(1000);

        UpdateGoldDisplay();
        UpdateGachaTypeDisplay();
    }

    /// <summary>
    /// 가챠 팝업 열기
    /// </summary>
    private void OpenGachaPopup()
    {
        if (gachaPopup != null)
        {
            gachaPopup.SetActive(true);
            // 스크롤 뷰 맨 위로 이동
            ScrollRect scrollRect = gachaPopup.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }

    /// <summary>
    /// 가챠 팝업 닫기
    /// </summary>
    private void CloseGachaPopup()
    {
        if (gachaPopup != null)
            gachaPopup.SetActive(false);
    }

    /// <summary>
    /// 가챠 타입 선택
    /// </summary>
    private void SelectGachaType(GachaGroup gachaType)
    {
        currentGachaType = gachaType;
        UpdateGachaTypeDisplay();
        Debug.Log($"가챠 타입 변경: {currentGachaType}");
    }

    /// <summary>
    /// 가챠 실행 (기존 프리팹 삭제 후 새로 표시)
    /// </summary>
    private void ExecuteGacha(int pullCount)
    {
        // 기존 프리팹 모두 삭제
        ClearPreviousResults();

        for (int i = 0; i < pullCount; i++)
        {
            GachaItem item = GachaManager.instance.DrawGacha(currentGachaType, 1);

            if (item != null)
            {
                Debug.Log($"프리팹 생성: {item.itemName}");
                GachaResultHandler.HandleGachaResult(item, scrollViewContent, gachaPool.DefaultResultPrefab);
            }
            else
            {
                Debug.Log("뽑기 실패: 골드 부족 또는 아이템 없음");
                break; // 실패하면 중단
            }
        }

        UpdateGoldDisplay();
    }

    /// <summary>
    /// 기존 프리팹 모두 삭제
    /// </summary>
    private void ClearPreviousResults()
    {
        if (scrollViewContent == null)
            return;

        // Content의 모든 자식 오브젝트 삭제
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("[가챠] 기존 결과 초기화 완료");
    }

    /// <summary>
    /// 골드 표시 업데이트
    /// </summary>
    private void UpdateGoldDisplay()
    {
        if (goldDisplay != null)
        {
            int currentGold = GoldManager.instance.GetCurrentGold();
            goldDisplay.text = $"Gold: {currentGold}";
        }
    }

    /// <summary>
    /// 가챠 타입 표시 업데이트
    /// </summary>
    private void UpdateGachaTypeDisplay()
    {
        if (currentGachaTypeDisplay != null)
        {
            currentGachaTypeDisplay.text = $"Current: {currentGachaType}";
        }
    }
}
