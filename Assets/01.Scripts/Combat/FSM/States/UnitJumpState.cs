using UnityEngine;

public sealed class UnitJumpState : UnitBaseState
{
    private int nextFloor;
    public UnitJumpState(BaseUnitController controller) : base(controller) { }
    public void SetDestination(int floor) => nextFloor = floor;

    public override void Enter()
    {
        controller.Attack.CancelPendingHit();
        controller.Move.BeginFloorTravel(controller.FloorMap, nextFloor);
    }

    public override void Exit() => controller.Move.CancelFloorTravel();
    public override void Update() { }

    public override void FixedUpdate()
    {
        if (controller.Health.IsDead)
        {
            controller.StateMachine.ChangeState(controller.DieState);
            return;
        }
        controller.Move.TickFloorTravel();
        if (!controller.IsChangingFloors)
            controller.StateMachine.ChangeState(controller.IsTargetDetected ? controller.MoveState : controller.IdleState);
    }
}
