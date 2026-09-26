using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyController : BaseUnitController, IPoolable
{
    [SerializeField] private PoolType enemyType = PoolType.Crab_0001;
    [SerializeField, Min(0f)] private float detectionRange = 4f;
    [SerializeField, Min(0f)] private float despawnDelay = 0f; // 사망 후 반납까지 (연출 있으면 늘리기)
    [SerializeField] private AudioClip attackSound;
    [Header("미감지 시 순찰")]
    [SerializeField, Min(0f)] private float patrolRadius = 2.5f;
    [SerializeField, Range(0.1f, 1f)] private float patrolSpeedMultiplier = 0.5f;
    [SerializeField] private Vector2 patrolWaitRange = new Vector2(0.4f, 1.2f);

    public EnemyNavigation Navigation { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyReturnHomeState ReturnHomeState { get; private set; }
    public float DetectionRange => detectionRange;
    public Enum PoolKey => enemyType;
    public Vector3 HomePosition => Navigation.HomePosition;
    public bool IsReturningHome => Navigation.IsReturningHome;
    public bool IsPatrolling => StateMachine != null && StateMachine.CurrentState == PatrolState;
    protected override bool InitiallyFacesRight => false;

    protected override void CreateStates()
    {
        base.CreateStates();
        Navigation = new EnemyNavigation(this, detectionRange, patrolRadius, patrolSpeedMultiplier, patrolWaitRange);
        IdleState = new EnemyIdleState(this);
        MoveState = new EnemyMoveState(this);
        CombatState = new EnemyCombatState(this);
        PatrolState = new EnemyPatrolState(this);
        ReturnHomeState = new EnemyReturnHomeState(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (Attack != null) Attack.OnAttackHit += PlayAttackSound;
    }

    protected override void OnDisable()
    {
        Move?.Stop();
        if (Attack != null) Attack.OnAttackHit -= PlayAttackSound;
        base.OnDisable();
    }

    private void PlayAttackSound(IDamageable target, DamageInfo damage)
    {
        if (attackSound != null) SoundManager.instance?.PlayEnemyAttackSFX(attackSound);
    }

    public void Init()
    {
        Navigation.ResetHome();
        Navigation.ResetTracking();
        ClearTarget();
        Revive();
    }
    public void ReturnToPool() => PrepareForPool();

    public override bool IsTargetDetected => Navigation.IsTargetDetected;

    public override bool CanEngage(BaseUnitController other) =>
        IsTargetDetected && (other == Target || base.CanEngage(other));

    public override void PrepareForPool()
    {
        Navigation.ResetTracking();
        base.PrepareForPool();
    }

    // 사망 시 호출: 이번 프레임 이벤트(드롭/리타겟)가 끝난 뒤 풀로 반납.
    public void DespawnAfterDeath()
    {
        DespawnAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid DespawnAsync(CancellationToken token)
    {
        int lifeVersion = Health.LifeVersion;
        try
        {
            if (despawnDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(despawnDelay), cancellationToken: token);
            else
                await UniTask.NextFrame(token); // 최소 한 프레임: 사망 이벤트 처리 완료 보장

            if (isActiveAndEnabled && Health.IsDead && Health.LifeVersion == lifeVersion)
                CombatObjectPoolManager.instance.ReturnObject(PoolKey, gameObject);
        }
        catch (OperationCanceledException) { }
    }
}
