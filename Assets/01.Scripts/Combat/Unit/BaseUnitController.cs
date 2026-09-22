using UnityEngine;

[RequireComponent(typeof(UnitHealth), typeof(UnitMove), typeof(UnitAttack))]
public abstract class BaseUnitController : MonoBehaviour
{
    public UnitHealth Health { get; protected set; }
    public UnitMove Move { get; protected set; }
    public UnitAttack Attack { get; protected set; }

    public StateMachine StateMachine { get; protected set; }
    public UnitIdleState IdleState { get; protected set; }
    public UnitCombatState CombatState { get; protected set; }
    public UnitMoveState MoveState { get; protected set; }
    public UnitDieState DieState { get; protected set; }
    public BaseUnitController Target { get; private set; }
    public event System.Action OnUnavailable;

    // 비주얼 어댑터(IUnitView). cat이든 enemy이든 이 인터페이스로만 다룬다.
    // 비워두면 자식에서 IUnitView 구현체를 자동 탐색한다. (없어도 로직에는 지장 없음)
    [SerializeField] private MonoBehaviour unitViewSource;
    private IUnitView unitView;

    public CombatFloorMap FloorMap { get; private set; }
    public bool IsChangingFloors => Move != null && Move.IsChangingFloors;
    public int CurrentFloor => FloorMap != null ? FloorMap.GetFloorIndex(transform.position) : 0;
    protected virtual bool InitiallyFacesRight => true;

    public bool IsFinishingAttack => unitView is IAttackRecoveryView recovery && recovery.IsFinishingAttack;

    public bool HasTarget => Target != null && Target.gameObject.activeInHierarchy && !Target.Health.IsDead;
    public bool IsTargetInAttackRange => HasTarget && Attack.IsUnitInAttackRange(Target);

    public void SetFloorMap(CombatFloorMap map) => FloorMap = map;

    public bool IsOnSameFloor(BaseUnitController other)
    {
        if (other == null || IsChangingFloors || other.IsChangingFloors) return false;
        if (FloorMap == null && other.FloorMap == null)
            return Mathf.Abs(transform.position.y - other.transform.position.y) <= 0.75f;
        if (FloorMap != other.FloorMap) return false;
        int currentFloor = CurrentFloor;
        return currentFloor >= 0 && currentFloor == other.CurrentFloor;
    }

    public virtual bool CanEngage(BaseUnitController other) => IsOnSameFloor(other);

    // 타겟을 '교전 대상'으로 인식했는가.
    // 기본(플레이어): 타겟이 있으면 항상 교전.
    // 적: 감지범위 안에 들어와야 교전 → EnemyController에서 오버라이드.
    public virtual bool IsTargetDetected => HasTarget;

    protected virtual void Awake()
    {
        Health = GetComponent<UnitHealth>();
        Move = GetComponent<UnitMove>();
        Attack = GetComponent<UnitAttack>();

        // 비주얼 어댑터 연결: 명시 지정 우선, 없으면 자식(자기 포함)에서 탐색.
        unitView = unitViewSource as IUnitView;
        if (unitView == null)
            unitView = GetComponentInChildren<IUnitView>(true);
        if (unitView is Component view && view.transform != transform)
        {
            Move.ConfigureFacing(view.transform, InitiallyFacesRight);
        }

#if UNITY_EDITOR
        if (unitViewSource != null && unitView == null)
            Debug.LogWarning($"[{name}] unitViewSource가 IUnitView를 구현하지 않습니다. 연결을 확인하세요.");
#endif

        StateMachine = new StateMachine();
        CreateStates();

        Health.OnDied += HandleDied;
    }

    protected virtual void CreateStates()
    {
        IdleState = new UnitIdleState(this);
        MoveState = new UnitMoveState(this);
        CombatState = new UnitCombatState(this);
        DieState = new UnitDieState(this);
    }

    protected void Start()
    {
        StateMachine.ChangeState(IdleState);
    }

    protected virtual void OnDestroy()
    {
        Health.OnDied -= HandleDied;
    }

    protected virtual void OnDisable()
    {
        Move.CancelFloorTravel();
        Move.Stop();
        OnUnavailable?.Invoke();
        // 비활성/풀 반납 시 실행 중인 공격 루프와 애니메이션 타격 예약을 모두 취소한다.
        CombatState?.Exit();
        if (Attack != null)
            Attack.CancelPendingHit();
    }

    protected virtual void OnEnable()
    {
        Move.ResetMotion();
        if (StateMachine?.CurrentState != null && Health != null && !Health.IsDead)
            StateMachine.ChangeState(IdleState);
    }

    protected virtual void Update() => StateMachine.Update();
    protected virtual void FixedUpdate() => StateMachine.FixedUpdate();

    // 상태(FSM)가 애니메이션을 요청하는 단일 창구.
    // 비주얼이 없으면(임시 square 등) 조용히 무시되어 로직에는 영향 없음.
    public void PlayAnimation(StudioNAP.AnimationTypeEnum ani)
    {
        if (unitView != null)
            unitView.RunAnimation(ani);
    }

    public virtual void SetTarget(BaseUnitController target)
    {
        Target = target == this ? null : target;
    }

    public void ClearTarget()
    {
        SetTarget(null);
    }

    public void Revive()
    {
        Attack.ResetAttack();
        Health.ResetHealth();                // IsDead = false, HP 복구
        StateMachine.ChangeState(IdleState); // DieState 탈출 → Idle 애니메이션도 여기서 복귀
    }

    // 오브젝트 풀 반납 직전 정리:
    // Idle로 전환하면 CombatState.Exit가 호출돼 진행 중이던 비동기 공격 루프가 취소된다.
    public virtual void PrepareForPool()
    {
        ClearTarget();
        StateMachine.ChangeState(IdleState);
        Move.Stop();
    }

    private void HandleDied()
    {
        StateMachine.ChangeState(DieState);
        OnUnavailable?.Invoke();
    }
}
