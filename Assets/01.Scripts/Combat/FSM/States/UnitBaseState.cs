using UnityEngine;

public abstract class UnitBaseState : IUnitState
{
    protected BaseUnitController controller;
    
    protected UnitBaseState(BaseUnitController controller)
    {
        this.controller = controller;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();
    public abstract void FixedUpdate();
}
