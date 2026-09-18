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

    private Rigidbody2D enemyRigidbody;
    private Quaternion initialLocalRotation;
    private Vector3 homePosition;
    private bool chasing;
    private bool returningHome;
    private ReturnHomeState returnState;

    public float DetectionRange => detectionRange;
    public Enum PoolKey => enemyType;
    public Vector3 HomePosition => homePosition;
    public bool IsReturningHome => returningHome;
    protected override bool InitiallyFacesRight => false;

    protected override void Awake()
    {
        base.Awake();
        enemyRigidbody = GetComponent<Rigidbody2D>();
        initialLocalRotation = transform.localRotation;
        returnState = new ReturnHomeState(this);

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
        !returningHome && IsTargetDetected && base.CanEngage(other);

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
