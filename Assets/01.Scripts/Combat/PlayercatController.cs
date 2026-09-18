using UnityEngine;

public class PlayercatController : BaseUnitController
{
    [SerializeField] private AudioClip attackSound;
    [SerializeField, Min(0.05f)] private float detectionInterval = 0.5f;
    [SerializeField, Min(0.1f)] private float floorTravelDuration = 0.65f;
    [SerializeField, Min(0f)] private float jumpArcHeight = 0.6f;

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
            EnemyController nearest = spawner.GetNearestAlive(transform.position, CurrentFloor);
            bool pendingFound = spawner.TryGetPendingDestination(transform.position, CurrentFloor, out searchPosition);
            // 같은 층의 화면 밖 예약을 먼저 찾는다. 다른 층의 활성 적 때문에 왕복하지 않는다.
            bool pendingOnFloor = pendingFound && (FloorMap == null ||
                FloorMap.GetFloorIndex(searchPosition) == CurrentFloor);
            if (pendingOnFloor && nearest != null && nearest.CurrentFloor != CurrentFloor) nearest = null;
            SetTarget(nearest);
            hasSearchPosition = nearest == null && pendingFound;
        }
        base.Update();
    }

    private void LateUpdate()
    {
        if (Health.IsDead || changingFloors) return;

        // 실제 수평 이동 방향으로 몸과 장비를 함께 반전한다.
        // 정지 시에는 FaceDirection의 임계값에 따라 마지막 방향을 유지한다.
        FaceDirection(body.linearVelocity.x);
    }

    protected override void FixedUpdate()
    {
        if (!changingFloors) { base.FixedUpdate(); return; }
        if (Health.IsDead) { CancelFloorTravel(); return; }
        travelElapsed += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(travelElapsed / Mathf.Max(0.1f, floorTravelDuration));
        Vector3 position = Vector3.Lerp(travelStart, travelEnd, t);
        position.y += Mathf.Sin(t * Mathf.PI) * jumpArcHeight;
        body.position = position;
        transform.position = position;
        if (t < 1f) return;
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
                if (!FloorMap.TryGetConnection(current, goal, out int next, out float x))
                { Move.Stop(); return; }
                FaceDirection(x - transform.position.x);
                if (Mathf.Abs(x - transform.position.x) > 0.08f) Move.MoveToX(x);
                else BeginFloorTravel(next, x);
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
