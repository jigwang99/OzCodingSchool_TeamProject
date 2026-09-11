using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// 스테이지 흐름 관리:
//  1) 플레이어를 시작 위치로 되돌리고 StageData 기준으로 적 스폰
//  2) CombatManager에 승패판정 위임
//  3) 재도전 토글 상태에 따라 진행/후퇴/반복 처리
public class StageManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CombatManager combatManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayercatController playerCat;
    [SerializeField] private FishDropSystem fishDropSystem;

    // 비워두면 씬에 배치된 플레이어의 최초 위치를 시작 위치로 사용
    [SerializeField] private Transform playerSpawnPoint;

    [Header("스테이지 데이터")]
    [SerializeField] private StageDataList stageDataList;   // SO 에셋 하나
    [SerializeField] private Transform enemySpawnOrigin;    // 비우면 enemySpawner 위치를 기준점으로 사용

    [Header("연출 딜레이(초)")]
    [SerializeField, Min(0f)] private float clearDelay = 2f;
    [SerializeField, Min(0f)] private float failDelay = 2f;

    [Header("스테이지 전환")]
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.2f;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.2f;
    private CanvasGroup fade;
    private bool isChangingStage;

    private int CurrentStage => GameManager.instance.PlayerData.currentStage;
    private bool IsRetry => GameManager.instance.PlayerData.isRetryEnabled;
    private int MaxStage => stageDataList != null ? Mathf.Max(1, stageDataList.Count) : 1;

    private Vector3 playerStartPosition;
    private bool isTransitioning;
    private float resultEndsAt;
    private bool isInitialized;
    private CancellationTokenSource restartCancellation;

    public int StageCount => stageDataList != null ? stageDataList.Count : 0;
    public int CurrentStageNumber => CurrentStage;

    public event Action<StageResult> OnStageResult;
    public event Action OnStageStarted;
    public StageResult? CurrentResult { get; private set; }
    public float ResultRemainingSeconds => isTransitioning
        ? Mathf.Max(0f, resultEndsAt - Time.time)
        : 0f;

    private void Start()
    {
        if (combatManager == null || enemySpawner == null || playerCat == null)
        {
            Debug.LogError("[StageManager] 참조가 비어 있습니다.");
            return;
        }

        playerStartPosition = playerSpawnPoint != null
            ? playerSpawnPoint.position
            : playerCat.transform.position;

        combatManager.OnStageCleared += HandleStageCleared;
        combatManager.OnStageFailed += HandleStageFailed;
        combatManager.OnEnemyDefeated += HandleEnemyDefeated;
        enemySpawner.OnEnemyActivated += HandleEnemyActivated;

        isInitialized = true;
        ChangeStageAsync().Forget();
    }

    private void OnDestroy()
    {
        isInitialized = false;
        CancelPendingRestart();
        if (enemySpawner != null) enemySpawner.OnEnemyActivated -= HandleEnemyActivated;
        if (combatManager != null)
        {
            combatManager.OnStageCleared -= HandleStageCleared;
            combatManager.OnStageFailed -= HandleStageFailed;
            combatManager.OnEnemyDefeated -= HandleEnemyDefeated;
        }
    }

    public string GetStageName(int stageNumber)
    {
        string stageName = stageDataList?.GetClone(stageNumber)?.StageName;
        return string.IsNullOrEmpty(stageName) ? stageNumber.ToString() : stageName;
    }

    // 선택 UI의 진입점. 미완료 전투에 클리어/패배 보상을 새로 발생시키지 않는다.
    public bool SelectStage(int stageNumber)
    {
        if (!isInitialized || !isActiveAndEnabled || isChangingStage || stageNumber < 1 || stageNumber > StageCount ||
            !CombatObjectPoolManager.instance.CanPrepare(stageDataList.GetClone(stageNumber)))
            return false;

        CancelPendingRestart();
        GameManager.instance.PlayerData.SetCurrentStage(stageNumber);
        ChangeStageAsync().Forget();
        SaveManager.instance?.Save();
        return true;
    }

    private void CancelPendingRestart()
    {
        // Dispose는 대기 작업의 finally에서 처리한다. 취소 직후 새 작업이 생겨도 서로 간섭하지 않는다.
        CancellationTokenSource pending = restartCancellation;
        restartCancellation = null;
        pending?.Cancel();
    }

    // 수동 선택/자동 진행/재도전 모두 동일한 전환을 사용한다.
    private async UniTask ChangeStageAsync()
    {
        StageData data = stageDataList?.GetClone(CurrentStage);
        if (!CombatObjectPoolManager.instance.CanPrepare(data))
        {
            Debug.LogError("[StageManager] 스테이지 데이터 또는 적 프리팹 등록을 확인하세요.", this);
            return;
        }
        isChangingStage = true;
        isTransitioning = true;
        combatManager.StopBattle();
        enemySpawner.SetCombatRunning(false);
        playerCat.HasPendingEnemies = false;
        playerCat.PrepareForPool();
        playerCat.enabled = false;
        CancellationToken token = this.GetCancellationTokenOnDestroy();
        EnsureFade();
        fade.gameObject.SetActive(true);
        try
        {
            await FadeAsync(1f, fadeOutDuration, token);
            // 완전히 검은 프레임을 그린 뒤 풀을 갱신한다.
            await UniTask.NextFrame(token);
            enemySpawner.Clear();
            await CombatFeedbackPool.PrepareForStageAsync(token);
            await CombatObjectPoolManager.instance.PrepareStageAsync(data, token);
            playerCat.transform.position = playerStartPosition;
            playerCat.Revive();
            fishDropSystem?.SetDropTable(data.DropTable);
            Vector3 origin = enemySpawnOrigin != null ? enemySpawnOrigin.position : enemySpawner.transform.position;
            combatManager.BeginBattle(playerCat, data.EnemyCount);
            enemySpawner.PrepareStage(data, origin, playerCat);
            RetargetPlayer();
            CurrentResult = null;
            // 결과 UI와 스테이지 표시는 검은 화면에서 갱신한다.
            OnStageStarted?.Invoke();
            await FadeAsync(0f, fadeInDuration, token);
            isTransitioning = false;
            enemySpawner.SetCombatRunning(true);
            playerCat.enabled = true;
        }
        finally
        {
            isChangingStage = false;
            if (fade != null) fade.gameObject.SetActive(false);
        }
    }

    private void EnsureFade()
    {
        if (fade != null) return;
        var root = new GameObject("StageTransitionFade", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasGroup), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;
        fade = root.GetComponent<CanvasGroup>();
        fade.alpha = 0f;
        var cover = new GameObject("Black", typeof(RectTransform), typeof(Image));
        cover.transform.SetParent(root.transform, false);
        Image image = cover.GetComponent<Image>();
        image.color = Color.black;
        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private async UniTask FadeAsync(float target, float duration, CancellationToken token)
    {
        float initial = fade.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            await UniTask.NextFrame(token);
            elapsed += Time.unscaledDeltaTime;
            fade.alpha = Mathf.Lerp(initial, target, Mathf.Clamp01(elapsed / duration));
        }
        fade.alpha = target;
    }

    private void HandleEnemyActivated(EnemyController enemy)
    {
        combatManager.RegisterEnemy(enemy);
        RetargetPlayer();
    }

    // 적이 하나 죽을 때마다 플레이어 타겟을 가장 가까운 살아있는 적으로 갱신
    private void HandleEnemyDefeated(EnemyController _)
    {
        RetargetPlayer();
    }

    private void RetargetPlayer()
    {
        EnemyController nearest = enemySpawner.GetNearestAlive(playerCat.transform.position);
        playerCat.SetTarget(nearest);
    }

    // 승리
    private void HandleStageCleared()
    {
        if (isTransitioning) return;
        int completedStage = CurrentStage;
        bool retryWasEnabled = IsRetry;

        if (!IsRetry && CurrentStage < MaxStage)
            GameManager.instance.PlayerData.SetCurrentStage(CurrentStage + 1);

        RestartAfterAsync(CreateResult(true, completedStage, retryWasEnabled, clearDelay)).Forget();
    }

    // 패배
    private void HandleStageFailed()
    {
        if (isTransitioning) return;
        int completedStage = CurrentStage;
        bool retryWasEnabled = IsRetry;

        if (!IsRetry)
        {
            // 챕터당 5개 스테이지: 1-1, 2-1, 3-1(진행도 1, 6, 11)에서는 후퇴하지 않는다.
            if ((completedStage - 1) % 5 != 0)
                GameManager.instance.PlayerData.SetCurrentStage(completedStage - 1);

            GameManager.instance.PlayerData.SetRetryEnabled(true);
        }

        RestartAfterAsync(CreateResult(false, completedStage, retryWasEnabled, failDelay)).Forget();
    }

    private StageResult CreateResult(bool isClear, int completedStage, bool retryWasEnabled, float delay)
    {
        string completedName = stageDataList?.GetClone(completedStage)?.StageName;
        string nextName = stageDataList?.GetClone(CurrentStage)?.StageName;
        return new StageResult(isClear, completedStage, CurrentStage,
            string.IsNullOrEmpty(completedName) ? completedStage.ToString() : completedName,
            string.IsNullOrEmpty(nextName) ? CurrentStage.ToString() : nextName,
            retryWasEnabled, Mathf.Max(0f, delay));
    }

    private async UniTaskVoid RestartAfterAsync(StageResult result)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        CurrentResult = result;
        resultEndsAt = Time.time + result.Delay;
        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        restartCancellation = cancellation;
        CancellationToken token = cancellation.Token;

        try
        {
            OnStageResult?.Invoke(result);
            await UniTask.Delay(TimeSpan.FromSeconds(result.Delay),
                cancellationToken: token);
            if (!token.IsCancellationRequested)
                await ChangeStageAsync();
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (restartCancellation == cancellation)
                restartCancellation = null;
            cancellation.Dispose();
        }
    }
}
