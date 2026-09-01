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
        // 교전 대상을 인식하지 못하면 대기 (적: 플레이어가 감지범위 밖)
        if (!controller.IsTargetDetected)
        {
            return;
        }

        controller.StateMachine.ChangeState(
            controller.IsTargetInAttackRange ? controller.CombatState : controller.MoveState);
    }
}