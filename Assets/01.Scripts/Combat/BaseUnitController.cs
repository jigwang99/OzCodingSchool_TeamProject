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
        Health.ResetHealth();               // IsDead = false, HP 복구
        StateMachine.ChangeState(IdleState); // DieState 탈출 → 다음 Update에 타겟 재탐색
    }
    private void HandleDied()
    {
        StateMachine.ChangeState(DieState);
    }
}
