using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelRestaurant.Data;
using System.Collections;

namespace PixelRestaurant.Gacha
{
    public class GachaUIController : MonoBehaviour
    {
        [Header("Gacha Open / Close")]
        [SerializeField] private Button gachaOpenButton;

        [SerializeField] private GameObject gachaPopup;
        [SerializeField] private GachaInventoryUI inventoryUI;

        [Header("Gacha Type")]
        [SerializeField] private Button weaponButton;
        [SerializeField] private Button furnitureButton;
        [SerializeField] private Button recipeButton;

        private GachaGroup _currentGachaType = GachaGroup.Weapon;

        [Header("Pull Buttons")]
        [SerializeField] private Button pull1Button;
        [SerializeField] private Button pull10Button;

        [Header("뽑기 가격 텍스트 (숫자만 표시)")]
        [SerializeField] private TextMeshProUGUI pull1CostDisplay;
        [SerializeField] private TextMeshProUGUI pull10CostDisplay;
        [Header("Gacha Animation")]
        [SerializeField] private Animator gachaAnimator;

        [SerializeField] private float shakeDuration = 10.0f;

        private bool isGachaPlaying = false;
        [Header("Gold")]
        [SerializeField] private TextMeshProUGUI goldDisplay;

        [Header("Current Group")]
        [SerializeField] private TextMeshProUGUI currentGroupDisplay;

        [Header("Pity")]
        [SerializeField] private TextMeshProUGUI pityLevelDisplay;
        [SerializeField] private TextMeshProUGUI pityCounterDisplay;
        [SerializeField] private Image pityProgressBar;

        [Header("Probability")]
        [SerializeField] private TextMeshProUGUI probabilityCommonDisplay;
        [SerializeField] private TextMeshProUGUI probabilityRareDisplay;
        [SerializeField] private TextMeshProUGUI probabilityUniqueDisplay;
        [SerializeField] private TextMeshProUGUI probabilityEpicDisplay;

        [Header("Result")]
        [SerializeField] private Transform resultSpawnPoint;

        [Header("Popup")]
        [SerializeField] private GameObject resultPopup;
        [SerializeField] private GameObject infoPopup;
        [SerializeField] private GameObject listPopup;

        [Header("Popup Buttons")]
        [SerializeField] private Button resultOpenButton;
        [SerializeField] private Button resultCloseButton;
        [SerializeField] private Button inventoryOpenButton;
        [SerializeField] private Button listOpenButton;

        [Header("Gacha Result SFX")]
        [SerializeField] private AudioClip commonResultSFX;
        [SerializeField] private AudioClip rareResultSFX;
        [SerializeField] private AudioClip uniqueResultSFX;
        [SerializeField] private AudioClip epicResultSFX;

        [Header("Gacha Warning SFX")]
        [SerializeField] private AudioClip insufficientGoldSFX;
        [SerializeField] private float insufficientGoldSFXDelay = 1.5f;
        [SerializeField] private float warningCooldown = 5f;
        private float nextWarningTime = 0f;

        [Header("Retry Gacha")]
        [SerializeField] private Button retryGachaButton;

        // 마지막으로 성공한 뽑기 횟수
        private int lastPullCount = 1;

        // 결과 팝업의 다시 뽑기: 이전 횟수로 즉시 재추첨 (Shake 없음)
        public void RetryLastGacha()
        {
            if (isGachaPlaying)
                return;

            ExecuteGacha(lastPullCount, true);
        }

        private void Start()
        {
            UpdateDisplay();
        }

        private void Update()
        {
            UpdateGoldDisplay();
        }

        // =========================
        // Gacha Open / Close
        // =========================

        public void OpenGachaPopup()
        {
            CloseAllPopups();

            if (gachaPopup != null)
            {
                gachaPopup.SetActive(true);
                UpdateDisplay();
            }
        }
        public void CloseAllPopups()
        {
            if (gachaPopup != null)
                gachaPopup.SetActive(false);

            if (resultPopup != null)
                resultPopup.SetActive(false);

            if (infoPopup != null)
                infoPopup.SetActive(false);

            if (listPopup != null)
                listPopup.SetActive(false);

            if (inventoryUI != null)
                inventoryUI.CloseInventory();
        }

        // =========================
        // Gacha Type
        // =========================
        public void CloseGachaPopup()
        {
            CloseAllPopups();
        }
        public void SelectWeapon()
        {
            SelectGachaType(GachaGroup.Weapon);
        }

        public void SelectFurniture()
        {
            SelectGachaType(GachaGroup.Furniture);
        }

        public void SelectRecipe()
        {
            SelectGachaType(GachaGroup.Recipe);
        }

        private void SelectGachaType(GachaGroup group)
        {
            _currentGachaType = group;


            UpdateDisplay();
        }
        public void OpenResultPopup()
        {
            if (resultPopup != null)
                resultPopup.SetActive(true);
        }

        public void CloseResultPopup()
        {
            if (resultPopup != null)
                resultPopup.SetActive(false);
        }

        // =========================
        // Pull
        // =========================


        public void Pull1()
        {
            ExecuteGacha(1);
        }

        public void Pull10()
        {
            ExecuteGacha(10);
        }

        private bool ExecuteGacha(int pullCount, bool isRetry = false)
        {
            // 애니메이션 재생 중에는 추가 뽑기 금지
            if (isGachaPlaying)
                return false;

            GachaManager gachaManager = GachaManager.Instance;

            if (gachaManager == null)
            {
                return false;
            }

            if (CurrencyManager.instance == null)
            {
                return false;
            }

            if (pullCount != 1 && pullCount != 10)
            {
                return false;
            }

            // =========================
            // 가챠 종류별 비용 가져오기
            // =========================

            BigNumber costPerPull =
                gachaManager.GetGachaCost(_currentGachaType);

            // 비용 유효성 검사
            if (costPerPull <= new BigNumber(0))
            {
                return false;
            }

            // =========================
            // 전체 뽑기 비용 계산
            // =========================

            BigNumber totalCost = new BigNumber(0);

            for (int i = 0; i < pullCount; i++)
            {
                totalCost += costPerPull;
            }

            BigNumber currentGold =
                CurrencyManager.instance.GetCurrentGold();

            // 골드 부족 시: ButtonSFX 클릭음이 먼저 들린 뒤 경고음을 재생
            if (currentGold < totalCost)
            {
                if (Time.unscaledTime >= nextWarningTime)
                {
                    nextWarningTime = Time.unscaledTime + warningCooldown;
                    StartCoroutine(PlayInsufficientGoldSFXAfterClick());
                }

                // 결과 팝업의 다시 뽑기에서 골드가 부족하면 팝업을 닫는다.
                if (isRetry && resultPopup != null)
                    resultPopup.SetActive(false);

                return false;
            }
            // =========================
            // 가챠 실행
            // =========================

            List<GachaItem> results =
                gachaManager.DrawGacha(
                    _currentGachaType,
                    pullCount,
                    costPerPull
                );

            if (results == null || results.Count == 0)
            {
                return false;
            }

            // 실제 성공한 뽑기 횟수를 기억한다.
            lastPullCount = pullCount;

            if (isRetry)
            {
                // 다시 뽑기는 Shake와 대기 없이 기존 카드를 교체한다.
                if (resultPopup != null)
                    resultPopup.SetActive(true);

                ShowGachaResults(results);
            }
            else
            {
                // 일반 뽑기만 Shake 애니메이션을 실행한다.
                StartCoroutine(PlayGachaAnimation(results));
            }

            // UI 갱신
            UpdateDisplay();

            return true;
        }
        private IEnumerator PlayInsufficientGoldSFXAfterClick()
        {
            if (insufficientGoldSFXDelay > 0f)
                yield return new WaitForSecondsRealtime(insufficientGoldSFXDelay);
            else
                yield return null;

            if (SoundManager.instance != null && insufficientGoldSFX != null)
            {
                SoundManager.instance.PlaySFX(insufficientGoldSFX);
            }
        }

        private IEnumerator PlayGachaAnimation(List<GachaItem> results)
        {
            isGachaPlaying = true;

            // 이전 가챠 결과 팝업이 열려 있으면 먼저 숨긴다.
            // 순서: 클릭 -> Shake 애니메이션 -> 대기 -> 결과 팝업/카드 표시
            if (resultPopup != null)
                resultPopup.SetActive(false);

            // 중복 클릭 방지
            if (pull1Button != null)
                pull1Button.interactable = false;

            if (pull10Button != null)
                pull10Button.interactable = false;

            // 애니메이션 재생
            if (gachaAnimator != null)
            {
                gachaAnimator.SetTrigger("Shake");
            }

            // Animator 연결 여부와 상관없이 결과 팝업은 지정 시간 뒤에 표시
            yield return new WaitForSecondsRealtime(shakeDuration);

            // 결과 팝업 표시
            if (resultPopup != null)
                resultPopup.SetActive(true);

            // 결과 카드 생성
            ShowGachaResults(results);

            // 버튼 다시 활성화
            if (pull1Button != null)
                pull1Button.interactable = true;

            if (pull10Button != null)
                pull10Button.interactable = true;

            isGachaPlaying = false;
        }
        // =========================
        // Result
        // =========================

        private void ShowGachaResults(List<GachaItem> items)
        {
            ClearPreviousResults();

            if (resultSpawnPoint == null ||
                items == null ||
                items.Count == 0)
            {
                return;
            }

            GachaResultHandler resultHandler =
                GachaResultHandler.Instance;

            if (resultHandler == null)
                return;

            // null 아이템을 제외한 실제 카드 수
            List<GachaItem> validItems =
                new List<GachaItem>();

            foreach (GachaItem item in items)
            {
                if (item != null)
                {
                    validItems.Add(item);
                }
            }

            int totalCards = validItems.Count;

            if (totalCards == 0)
                return;

            // 결과 전체 중 최고 등급 등장음만 한 번 재생
            PlayHighestRarityResultSFX(validItems);

            for (int i = 0; i < totalCards; i++)
            {
                resultHandler.HandleGachaResult(
                    validItems[i],
                    resultSpawnPoint,
                    i,
                    totalCards
                );
            }
        }
        private void PlayHighestRarityResultSFX(List<GachaItem> items)
        {
            if (SoundManager.instance == null ||
                items == null ||
                items.Count == 0)
            {
                return;
            }

            GachaRarity highestRarity = GachaRarity.Common;

            foreach (GachaItem item in items)
            {
                if (item == null)
                    continue;

                if ((int)item.Rarity > (int)highestRarity)
                {
                    highestRarity = item.Rarity;
                }
            }

            AudioClip clip = null;

            switch (highestRarity)
            {
                case GachaRarity.Common:
                    clip = commonResultSFX;
                    break;

                case GachaRarity.Rare:
                    clip = rareResultSFX;
                    break;

                case GachaRarity.Unique:
                    clip = uniqueResultSFX;
                    break;

                case GachaRarity.Epic:
                    clip = epicResultSFX;
                    break;
            }

            if (clip != null)
            {
                SoundManager.instance.PlaySFX(clip);
            }
        }
        private void ClearPreviousResults()
        {
            if (resultSpawnPoint == null)
                return;

            GachaResultHandler resultHandler =
                GachaResultHandler.Instance;

            if (resultHandler == null)
            {
                return;
            }

            GachaObjectPool objectPool =
                resultHandler.GetComponent<GachaObjectPool>();

            if (objectPool == null)
            {
                return;
            }

            List<GameObject> resultCards = new List<GameObject>();

            foreach (Transform child in resultSpawnPoint)
            {
                resultCards.Add(child.gameObject);
            }

            foreach (GameObject card in resultCards)
            {
                objectPool.ReturnObject(card);
            }
        }
        // =========================
        // Display
        // =========================

        private void UpdateDisplay()
        {
            UpdateGoldDisplay();
            UpdateGroupDisplay();
            UpdatePullCostDisplay();
            UpdatePityDisplay();
            UpdateProbabilityDisplay();
        }
        // 실제 결제와 동일한 GachaManager의 그룹별 1회 가격을 표시합니다.
        private void UpdatePullCostDisplay()
        {
            GachaManager manager = GachaManager.Instance;
            if (manager == null)
            {
                if (pull1CostDisplay != null) pull1CostDisplay.text = " -";
                if (pull10CostDisplay != null) pull10CostDisplay.text = "-";
                return;
            }

            BigNumber costPerPull = manager.GetGachaCost(_currentGachaType);
            if (pull1CostDisplay != null)
                pull1CostDisplay.text = costPerPull.ToString();

            if (pull10CostDisplay != null)
            {
                BigNumber totalCost = new BigNumber(0);
                for (int i = 0; i < 10; i++)
                    totalCost += costPerPull;
                pull10CostDisplay.text = totalCost.ToString();
            }
        }

        private void UpdateGoldDisplay()
        {
            if (goldDisplay == null)
                return;

            if (CurrencyManager.instance == null)
            {
                goldDisplay.text = "0";
                return;
            }

            goldDisplay.text =
                CurrencyManager.instance
                    .GetCurrentGold()
                    .ToString();
        }
        private void UpdateGroupDisplay()
        {
            if (currentGroupDisplay == null)
                return;

            switch (_currentGachaType)
            {
                case GachaGroup.Weapon:
                    currentGroupDisplay.text = "무기";
                    break;

                case GachaGroup.Furniture:
                    currentGroupDisplay.text = "가구";
                    break;

                case GachaGroup.Recipe:
                    currentGroupDisplay.text = "레시피";
                    break;
            }
        }
        public void OpenInventoryPopup()
        {
            CloseAllPopups();

            if (inventoryUI != null)
                inventoryUI.OpenInventory();
        }
        public void OpenListPopup()
        {
            CloseAllPopups();

            if (listPopup != null)
                listPopup.SetActive(true);
        }
        private void UpdatePityDisplay()
        {
            if (GachaPitySystem.Instance == null)
                return;

            GachaPoolData pool =
                GachaManager.Instance != null
                    ? GachaManager.Instance.GetPoolData(_currentGachaType)
                    : null;

            if (pool == null || pool.PityConfig == null)
                return;

            GachaPityConfig config = pool.PityConfig;

            int level =
                GachaPitySystem.Instance.GetCurrentPityLevel(
                    _currentGachaType
                );

            if (pityLevelDisplay != null)
                pityLevelDisplay.text = $"Lv.{level}";

            if (pityCounterDisplay != null)
            {
                pityCounterDisplay.text =
                    GachaPitySystem.Instance.GetPityDisplayText(
                        _currentGachaType,
                        config
                    );
            }

            if (pityProgressBar != null)
            {
                pityProgressBar.fillAmount =
                    GachaPitySystem.Instance.GetPityProgressFillAmount(
                        _currentGachaType,
                        config
                    );
            }
        }

        private void UpdateProbabilityDisplay()
        {
            if (GachaManager.Instance == null)
                return;

            GachaPoolData pool =
                GachaManager.Instance.GetPoolData(
                    _currentGachaType
                );

            if (pool == null || pool.PityConfig == null)
                return;

            GachaPityConfig config = pool.PityConfig;

            UpdateRarityText(
                probabilityCommonDisplay,
                GachaRarity.Common,
                config
            );

            UpdateRarityText(
                probabilityRareDisplay,
                GachaRarity.Rare,
                config
            );

            UpdateRarityText(
                probabilityUniqueDisplay,
                GachaRarity.Unique,
                config
            );

            UpdateRarityText(
                probabilityEpicDisplay,
                GachaRarity.Epic,
                config
            );
        }

        private void UpdateRarityText(
            TextMeshProUGUI text,
            GachaRarity rarity,
            GachaPityConfig config)
        {
            if (text == null || config == null)
                return;

            float weight =
                GachaPitySystem.Instance.GetRarityWeight(
                    _currentGachaType,
                    config,
                    rarity
                );

            text.text = $"{rarity}: \n{weight:0.##}%";
        }
        // 가챠 결과 카드 모두 뒤집기
        public void RevealAllCards()
        {
            if (resultSpawnPoint == null)
                return;

            foreach (Transform child in resultSpawnPoint)
            {
                if (!child.gameObject.activeInHierarchy)
                    continue;

                GachaRevealCard card =
                    child.GetComponent<GachaRevealCard>();

                if (card != null)
                {
                    card.RevealCard();
                }
            }
        }
    }

}