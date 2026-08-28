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

        float directionX = Mathf.Sign(controller.Target.transform.position.x - controller.transform.position.x);
        controller.Move.MoveTo(new Vector2(directionX, 0f));
    }

    public override void Update()
    {
    }
}
