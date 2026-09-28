public class StateMachine
{
    public IUnitState CurrentState { get; private set; }
   
    public void ChangeState(IUnitState state)
    {
        CurrentState?.Exit();
        CurrentState = state;
        CurrentState.Enter();
    }
    public void Update() => CurrentState?.Update();
    public void FixedUpdate() => CurrentState?.FixedUpdate();
}
