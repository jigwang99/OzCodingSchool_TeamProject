using UnityEngine;

public class UnitDieState : UnitBaseState
{
    public UnitDieState(BaseUnitController controller) : base(controller)
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
        
    }
}
