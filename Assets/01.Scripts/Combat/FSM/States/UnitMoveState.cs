using UnityEngine;

public class UnitMoveState : UnitBaseState
{
    public UnitMoveState(BaseUnitController controller) : base(controller)
    {
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
        controller.Move.Stop();
    }

    public override void FixedUpdate()
    {
        if (!controller.HasTarget)
        {
            controller.StateMachine.ChangeState(controller.IdleState);
            return;
        }

        if (controller.IsTargetInAttackRange)
        {
            controller.StateMachine.ChangeState(controller.CombatState);
            return;
        }

        controller.Move.MoveTo(controller.Target.transform);
    }

    public override void Update()
    {
    }
}
