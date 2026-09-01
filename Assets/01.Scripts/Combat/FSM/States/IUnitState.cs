using UnityEngine;

public interface IUnitState
{
    public void Enter();
    public void Exit();
    public void Update();
    public void FixedUpdate();
}
