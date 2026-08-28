public class UnitIdleState : UnitBaseState
{
    public UnitIdleState(BaseUnitController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        controller.Move.Stop();
    }

    public override void Exit()
    {
    }

    public override void FixedUpdate()
    {
    }

    public override void Update()
    {
        if (!controller.HasTarget)
        {
            return;
        }

        controller.StateMachine.ChangeState(
            controller.IsTargetInAttackRange ? controller.CombatState : controller.MoveState);
    }
}
