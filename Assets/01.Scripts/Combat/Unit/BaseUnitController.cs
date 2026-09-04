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

    // 비주얼 어댑터(IUnitView). cat이든 crab이든 이 인터페이스로만 다룬다.
    // Unity는 인터페이스 필드를 직접 직렬화하지 못하므로 MonoBehaviour 슬롯으로 받고 Awake에서 캐스팅.
    // 비워두면 자식에서 IUnitView 구현체를 자동 탐색한다. (없어도 로직에는 지장 없음)
    [SerializeField] private MonoBehaviour unitViewSource;
    private IUnitView unitView;

    public bool HasTarget => Target != null && !Target.Health.IsDead;
    public bool IsTargetInAttackRange => HasTarget && Attack.IsInAttackRange(Target.transform);

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

        if (unitViewSource != null && unitView == null)
            Debug.LogWarning($"[{name}] unitViewSource가 IUnitView를 구현하지 않습니다. 연결을 확인하세요.");

        StateMachine = new StateMachine();
        IdleState = new UnitIdleState(this);
        MoveState = new UnitMoveState(this);
        CombatState = new UnitCombatState(this);
        DieState = new UnitDieState(this);

        Health.OnDied += HandleDied;
    }

    protected void Start()
    {
        StateMachine.ChangeState(IdleState);
    }

    protected virtual void OnDestroy()
    {
        Health.OnDied -= HandleDied;
    }

    protected void Update() => StateMachine.Update();
    protected void FixedUpdate() => StateMachine.FixedUpdate();

    // 상태(FSM)가 애니메이션을 요청하는 단일 창구.
    // 비주얼이 없으면(임시 square 등) 조용히 무시되어 로직에는 영향 없음.
    public void PlayAnimation(StudioNAP.AnimationTypeEnum ani)
    {
        if (unitView != null)
            unitView.RunAnimation(ani);
    }

    // Move 상태의 실제 이동. 기본: 타겟을 향해 이동(적 추적).
    // 플레이어는 앞으로 전진하도록 오버라이드.
    public virtual void PerformMove()
    {
        if (HasTarget)
        {
            Move.MoveTo(Target.transform);
        }
    }

    public void SetTarget(BaseUnitController target)
    {
        Target = target == this ? null : target;
    }

    public void ClearTarget()
    {
        Target = null;
    }

    public bool TryAttackTarget()
    {
        // 자기 자신이 죽었으면 공격 불가 (비동기 공격 루프가 죽는 프레임에 한 번 더 도는 것을 차단).
        if (Health.IsDead)
            return false;

        return IsTargetInAttackRange && Attack.Attack(Target.Health);
    }

    public void Revive()
    {
        Health.ResetHealth();                // IsDead = false, HP 복구
        StateMachine.ChangeState(IdleState); // DieState 탈출 → Idle 애니메이션도 여기서 복귀
    }

    // 오브젝트 풀 반납 직전 정리:
    // Idle로 전환하면 CombatState.Exit가 호출돼 진행 중이던 비동기 공격 루프가 취소된다.
    public void PrepareForPool()
    {
        ClearTarget();
        StateMachine.ChangeState(IdleState);
        Move.Stop();
    }

    private void HandleDied()
    {
        StateMachine.ChangeState(DieState);
    }
}