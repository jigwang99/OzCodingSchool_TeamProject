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

    private Rigidbody2D enemyRigidbody;
    private Quaternion initialLocalRotation;
    private Vector3 homePosition;
    private bool chasing;
    private bool returningHome;
    private ReturnHomeState returnState;
    private PatrolState patrolState;

    public float DetectionRange => detectionRange;
    public Enum PoolKey => enemyType;
    public Vector3 HomePosition => homePosition;
    public bool IsReturningHome => returningHome;
    public bool IsPatrolling => StateMachine != null && StateMachine.CurrentState == patrolState;
    protected override bool InitiallyFacesRight => false;

    protected override void Awake()
    {
        base.Awake();
        enemyRigidbody = GetComponent<Rigidbody2D>();
        initialLocalRotation = transform.localRotation;
        returnState = new ReturnHomeState(this);
        patrolState = new PatrolState(this);

        // 좌우로 이동하는 적이 충돌 때문에 넘어지지 않도록 기존 제약에 회전 고정을 추가한다.
        enemyRigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    protected override void OnEnable()
    {
        // 풀 재사용과 직접 재활성화 모두 이전 생명의 물리 상태를 남기지 않는다.
        enemyRigidbody.linearVelocity = Vector2.zero;
        enemyRigidbody.angularVelocity = 0f;
        transform.localRotation = initialLocalRotation;
        enemyRigidbody.rotation = transform.eulerAngles.z;
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
        homePosition = transform.position;
        chasing = returningHome = false;
        ClearTarget();
        Revive();
    }
    public void ReturnToPool() => PrepareForPool();

    public override bool IsTargetDetected =>
        !returningHome && HasTarget && IsOnSameFloor(Target) &&
        (chasing || Mathf.Abs(transform.position.x - Target.transform.position.x) <= detectionRange);

    public override bool CanEngage(BaseUnitController other) =>
        IsTargetDetected && (other == Target || base.CanEngage(other));

    protected override void Update()
    {
        if (!Health.IsDead && !returningHome)
        {
            if (chasing && (!HasTarget || !IsOnSameFloor(Target)))
            {
                chasing = false;
                returningHome = true;
                StateMachine.ChangeState(returnState);
            }
            else if (IsTargetDetected) chasing = true;
            else if (StateMachine.CurrentState == IdleState && TryGetPatrolRange(out _))
                StateMachine.ChangeState(patrolState);
        }
        base.Update();
    }

    public override void PerformMove()
    {
        if (!IsTargetDetected) { Move.Stop(); return; }
        Vector3 destination = Target.transform.position;
        if (FloorMap != null)
        {
            int homeFloor = FloorMap.GetFloorIndex(homePosition);
            if (homeFloor < 0) { Move.Stop(); return; }
            destination = FloorMap.ClampToFloor(destination, homeFloor);
        }
        FaceDirection(destination.x - transform.position.x);
        Move.MoveToX(destination.x);
    }

    public override void PrepareForPool()
    {
        chasing = returningHome = false;
        base.PrepareForPool();
    }

    private bool TryGetPatrolRange(out Vector2 range)
    {
        range = new Vector2(homePosition.x - patrolRadius, homePosition.x + patrolRadius);
        if (patrolRadius <= 0f) return false;
        if (FloorMap != null)
        {
            int floor = FloorMap.GetFloorIndex(homePosition);
            if (floor < 0) return false;
            Vector2 walkable = FloorMap.GetWalkableRange(floor);
            range.x = Mathf.Max(range.x, walkable.x);
            range.y = Mathf.Min(range.y, walkable.y);
        }
        return range.y - range.x > 0.06f;
    }

    private sealed class PatrolState : UnitBaseState
    {
        private readonly EnemyController enemy;
        private float direction;
        private float waitRemaining;
        private bool walking;

        public PatrolState(EnemyController enemy) : base(enemy) => this.enemy = enemy;

        public override void Enter()
        {
            direction = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            StartWaiting();
        }

        private void StartWaiting()
        {
            enemy.Move.Stop();
            enemy.PlayAnimation(StudioNAP.AnimationTypeEnum.Idle);
            walking = false;
            float min = Mathf.Max(0f, Mathf.Min(enemy.patrolWaitRange.x, enemy.patrolWaitRange.y));
            float max = Mathf.Max(min, Mathf.Max(enemy.patrolWaitRange.x, enemy.patrolWaitRange.y));
            waitRemaining = UnityEngine.Random.Range(min, max);
        }

        public override void Exit() => enemy.Move.Stop();

        public override void Update()
        {
            if (!enemy.IsTargetDetected) return;
            enemy.chasing = true;
            enemy.StateMachine.ChangeState(enemy.IsTargetInAttackRange ? enemy.CombatState : enemy.MoveState);
        }

        public override void FixedUpdate()
        {
            if (enemy.IsTargetDetected) { Update(); return; }
            if (!enemy.TryGetPatrolRange(out Vector2 range))
            {
                enemy.StateMachine.ChangeState(enemy.IdleState);
                return;
            }
            if (waitRemaining > 0f)
            {
                waitRemaining -= Time.fixedDeltaTime;
                return;
            }
            float destination = direction > 0f ? range.y : range.x;
            float delta = destination - enemy.transform.position.x;
            if (Mathf.Abs(delta) <= 0.03f)
            {
                direction = -direction;
                StartWaiting();
                return;
            }
            if (!walking)
            {
                enemy.PlayAnimation(StudioNAP.AnimationTypeEnum.Run);
                walking = true;
            }
            enemy.FaceDirection(delta);
            // 추적 속도 자체는 바꾸지 않고 이번 물리 프레임의 순찰 이동량만 제한한다.
            float step = enemy.Move.MoveSpeed * Mathf.Clamp01(enemy.patrolSpeedMultiplier) * Time.fixedDeltaTime;
            enemy.Move.MoveToX(enemy.transform.position.x + Mathf.Clamp(delta, -step, step));
        }
    }

    private sealed class ReturnHomeState : UnitBaseState
    {
        private readonly EnemyController enemy;
        public ReturnHomeState(EnemyController enemy) : base(enemy) => this.enemy = enemy;
        public override void Enter()
        {
            enemy.Attack.CancelPendingHit();
            enemy.Move.Stop();
            enemy.PlayAnimation(StudioNAP.AnimationTypeEnum.Run);
        }
        public override void Exit() => enemy.Move.Stop();
        public override void Update() { }
        public override void FixedUpdate()
        {
            float delta = enemy.homePosition.x - enemy.transform.position.x;
            if (Mathf.Abs(delta) <= 0.03f)
            {
                Vector2 position = enemy.enemyRigidbody.position;
                position.x = enemy.homePosition.x;
                enemy.enemyRigidbody.position = position;
                enemy.returningHome = false;
                enemy.StateMachine.ChangeState(enemy.IdleState);
                return;
            }
            enemy.FaceDirection(delta);
            enemy.Move.MoveToX(enemy.homePosition.x);
        }
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
