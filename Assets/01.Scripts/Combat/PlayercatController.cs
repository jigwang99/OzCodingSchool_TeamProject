using UnityEngine;

public class PlayercatController : BaseUnitController
{
    [SerializeField] private AudioClip attackSound;
    [SerializeField, Min(0.05f)] private float detectionInterval = 0.5f;
    [SerializeField, Min(1f), Tooltip("층 이동에 적용할 중력 가속도. 높일수록 빠르게 도약하고 착지합니다.")]
    private float jumpGravity = 60f;
    [SerializeField, Min(0.05f), Tooltip("위층 착지 높이보다 더 올라가는 높이입니다.")]
    private float jumpApexClearance = 0.35f;
    [SerializeField, Min(0f)] private float jumpHorizontalReach = 3f;

    private EnemySpawner spawner;
    private Rigidbody2D body;
    private float nextDetection;
    private Vector3 searchPosition;
    private bool hasSearchPosition;
    private bool changingFloors;
    private bool previousSimulation;
    private Vector3 travelStart;
    private Vector3 travelEnd;
    private float travelElapsed;
    private float travelDuration;
    private float travelGravity;
    private Vector2 travelVelocity;

    public override bool IsChangingFloors => changingFloors;

    protected override void Awake()
    {
        base.Awake();
        body = GetComponent<Rigidbody2D>();
    }

    public void ConfigureNavigation(EnemySpawner source, CombatFloorMap map)
    {
        CancelFloorTravel();
        spawner = source;
        SetFloorMap(map);
        hasSearchPosition = false;
        nextDetection = 0f;
    }

    // 이벤트에서는 예약만 하고 실제 탐지는 공격 모션이 끝난 뒤 수행한다.
    public void RequestDetection() => nextDetection = 0f;

    protected override void OnEnable()
    {
        base.OnEnable();
        nextDetection = 0f;
        if (Attack != null) Attack.OnAttackStarted += PlayAttackSound;
    }

    protected override void OnDisable()
    {
        CancelFloorTravel();
        hasSearchPosition = false;
        if (Attack != null) Attack.OnAttackStarted -= PlayAttackSound;
        base.OnDisable();
    }

    private void PlayAttackSound()
    {
        if (attackSound != null) SoundManager.instance?.PlaySFX(attackSound);
    }

    public bool HasPendingEnemies { get; set; }
    public override bool IsTargetDetected => HasTarget || hasSearchPosition || changingFloors;

    protected override void Update()
    {
        if (!Health.IsDead && !changingFloors && !IsFinishingAttack && !Attack.HasPendingHit &&
            spawner != null && Time.time >= nextDetection)
        {
            nextDetection = Time.time + Mathf.Max(0.05f, detectionInterval);
            EnemyController nearest = spawner.GetNearestAlive(transform.position);
            bool pendingFound = spawner.TryGetPendingDestination(transform.position, out searchPosition);
            // 층과 활성화 여부에 관계없이 X·Y 직선거리로 다음 목적지를 결정한다.
            if (pendingFound && nearest != null &&
                ((Vector2)(searchPosition - transform.position)).sqrMagnitude <
                ((Vector2)(nearest.transform.position - transform.position)).sqrMagnitude)
                nearest = null;
            SetTarget(nearest);
            hasSearchPosition = nearest == null && pendingFound;
        }
        base.Update();
    }

    private void LateUpdate()
    {
        if (Health.IsDead || changingFloors) return;

        // 정지 공격도 대상을 바라보며, 남은 이동 속도가 공격 방향을 덮어쓰지 않게 한다.
        if (HasTarget && (IsTargetInAttackRange || Attack.HasPendingHit || IsFinishingAttack))
        {
            FaceDirection(Target.transform.position.x - transform.position.x);
            return;
        }
        // 처치 후 남은 공격 모션은 마지막 공격 방향을 유지한다.
        if (Attack.HasPendingHit || IsFinishingAttack) return;

        // 실제 수평 이동 방향으로 몸과 장비를 함께 반전한다.
        // 정지 시에는 FaceDirection의 임계값에 따라 마지막 방향을 유지한다.
        FaceDirection(body.linearVelocity.x);
    }

    protected override void FixedUpdate()
    {
        if (!changingFloors) { base.FixedUpdate(); return; }
        if (Health.IsDead) { CancelFloorTravel(); return; }
        travelElapsed = Mathf.Min(travelElapsed + Time.fixedDeltaTime, travelDuration);
        float t = travelElapsed;
        // 일정한 중력 가속도: 상승 중 감속하고 정점을 지난 뒤 가속하며 내려온다.
        Vector3 position = travelStart + (Vector3)(travelVelocity * t);
        position.y -= 0.5f * travelGravity * t * t;
        bool landed = travelElapsed >= travelDuration;
        if (landed) position = travelEnd;
        body.position = position;
        transform.position = position;
        if (!landed) return;
        changingFloors = false;
        body.simulated = previousSimulation;
        Move.Stop();
        RequestDetection();
    }

    public override void PerformMove()
    {
        if (!HasTarget && !hasSearchPosition) { Move.Stop(); return; }
        Vector3 destination = HasTarget ? Target.transform.position : searchPosition;
        if (FloorMap != null)
        {
            int current = CurrentFloor;
            int goal = FloorMap.GetFloorIndex(destination);
            if (current < 0 || goal < 0) { Move.Stop(); return; }
            if (current != goal)
            {
                if (!FloorMap.TryGetJump(current, goal, transform.position.x, destination.x, jumpHorizontalReach,
                    out int next, out float takeoffX, out float landingX))
                { Move.Stop(); return; }
                if (Mathf.Abs(takeoffX - transform.position.x) > 0.08f)
                {
                    FaceDirection(takeoffX - transform.position.x);
                    Move.MoveToX(takeoffX);
                }
                else
                {
                    FaceDirection(landingX - transform.position.x);
                    BeginFloorTravel(next, landingX);
                }
                return;
            }
            destination = FloorMap.ClampToFloor(destination, current);
        }
        FaceDirection(destination.x - transform.position.x);
        Move.MoveToX(destination.x);
    }

    private void BeginFloorTravel(int nextFloor, float x)
    {
        Attack.CancelPendingHit();
        Move.Stop();
        travelStart = transform.position;
        travelEnd = new Vector3(x, FloorMap.GetStandingY(nextFloor), travelStart.z);
        travelElapsed = 0f;
        travelGravity = Mathf.Max(1f, jumpGravity);
        float heightDifference = travelEnd.y - travelStart.y;
        // 위층은 착지 높이를 넘은 뒤 하강하며 착지하고, 아래층은 바로 낙하한다.
        float initialYSpeed = heightDifference > 0f
            ? Mathf.Sqrt(2f * travelGravity * (heightDifference + Mathf.Max(0.05f, jumpApexClearance)))
            : 0f;
        travelDuration = Mathf.Max(Time.fixedDeltaTime,
            (initialYSpeed + Mathf.Sqrt(initialYSpeed * initialYSpeed - 2f * travelGravity * heightDifference))
            / travelGravity);
        travelVelocity = new Vector2((travelEnd.x - travelStart.x) / travelDuration, initialYSpeed);
        previousSimulation = body.simulated;
        body.simulated = false;
        changingFloors = true;
    }

    private void CancelFloorTravel()
    {
        if (!changingFloors || body == null) return;
        body.position = travelStart;
        transform.position = travelStart;
        body.simulated = previousSimulation;
        changingFloors = false;
        Move.Stop();
    }
}
