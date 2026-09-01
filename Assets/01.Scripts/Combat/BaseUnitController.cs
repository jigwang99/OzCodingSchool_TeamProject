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
        return IsTargetInAttackRange && Attack.Attack(Target.Health);
    }

    public void Revive()
    {
        Health.ResetHealth();                // IsDead = false, HP 복구
        StateMachine.ChangeState(IdleState); // DieState 탈출
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