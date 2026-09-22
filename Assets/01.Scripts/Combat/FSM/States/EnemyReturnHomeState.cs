using UnityEngine;
using StudioNAP;

public sealed class EnemyReturnHomeState : UnitBaseState
{
    private readonly EnemyController enemy;
    public EnemyReturnHomeState(EnemyController enemy) : base(enemy) => this.enemy = enemy;
    public override void Enter()
    {
        enemy.Navigation.BeginReturn();
        enemy.Attack.CancelPendingHit();
        enemy.Move.Stop();
        enemy.PlayAnimation(AnimationTypeEnum.Run);
    }

    public override void Exit() => enemy.Move.Stop();
    public override void Update() { }
    public override void FixedUpdate()
    {
        float x = enemy.Navigation.HomePosition.x;
        if (Mathf.Abs(x - enemy.transform.position.x) <= 0.03f)
        {
            enemy.Move.SnapToX(x);
            enemy.Navigation.ResetTracking();
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }
        enemy.Move.MoveToX(x);
    }
}
