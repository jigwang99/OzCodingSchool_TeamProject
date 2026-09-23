using UnityEngine;

using System.Collections;

public class PlayercatController : BaseUnitController
{
    [SerializeField] private AudioClip attackSound;

    private EnemySpawner spawner;
    private Coroutine detectionRoutine;
    private Vector3 searchPosition;
    private bool hasSearchPosition;

    public bool HasSearchPosition => hasSearchPosition;
    public Vector3 SearchPosition => searchPosition;

    protected override void CreateStates()
    {
        base.CreateStates();
        MoveState = new PlayerMoveState(this);
    }

    public void ConfigureNavigation(EnemySpawner source, CombatFloorMap map)
    {
        Move.CancelFloorTravel();
        spawner = source;
        SetFloorMap(map);
        hasSearchPosition = false;
        ClearTarget();
        RequestDetection();
    }

    // 이벤트에서는 예약만 하고 실제 탐지는 공격 모션이 끝난 뒤 수행한다.
    public void RequestDetection()
    {
        if (!isActiveAndEnabled || Health.IsDead || HasTarget || spawner == null || detectionRoutine != null)
            return;
        detectionRoutine = StartCoroutine(DetectWhenReady());
    }

    private IEnumerator DetectWhenReady()
    {
        // 사망 이벤트 안에서는 아직 타격 성공/후반 모션 상태가 갱신되기 전이다.
        yield return null;
        while (IsChangingFloors || Attack.HasPendingHit || IsFinishingAttack)
            yield return null;
        detectionRoutine = null;
        if (!isActiveAndEnabled || Health.IsDead || HasTarget || spawner == null) yield break;

        EnemyController nearest = spawner.GetNearestAlive(transform.position);
        bool pendingFound = spawner.TryGetPendingDestination(transform.position, out searchPosition);
        if (pendingFound && nearest != null &&
            ((Vector2)(searchPosition - transform.position)).sqrMagnitude <
            ((Vector2)(nearest.transform.position - transform.position)).sqrMagnitude)
            nearest = null;
        hasSearchPosition = nearest == null && pendingFound;
        // 탐색 결과가 없으면 다음 생성 이벤트를 기다린다.
        SetTarget(nearest);
    }

    public override void SetTarget(BaseUnitController target)
    {
        bool hadTarget = Target != null;
        if (Target != null) Target.OnUnavailable -= HandleTargetUnavailable;
        base.SetTarget(target);
        if (Target != null)
        {
            Target.OnUnavailable += HandleTargetUnavailable;
            hasSearchPosition = false;
        }
        else if (hadTarget) RequestDetection();
    }

    private void HandleTargetUnavailable()
    {
        ClearTarget();
        if (Attack.HasPendingHit && !Attack.IsPendingTargetValid) Attack.CancelPendingHit();
        RequestDetection();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (Attack != null) Attack.OnAttackStarted += HandleAttackStarted;
        RequestDetection();
    }

    protected override void OnDisable()
    {
        if (detectionRoutine != null) StopCoroutine(detectionRoutine);
        detectionRoutine = null;
        ClearTarget();
        hasSearchPosition = false;
        if (Attack != null) Attack.OnAttackStarted -= HandleAttackStarted;
        base.OnDisable();
    }

    private void HandleAttackStarted()
    {
        if (attackSound != null) SoundManager.instance?.PlaySFX(attackSound);
    }

    public bool HasPendingEnemies { get; set; }
    public override bool IsTargetDetected => HasTarget || hasSearchPosition || IsChangingFloors;
}
