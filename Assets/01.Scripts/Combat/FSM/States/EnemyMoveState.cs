public sealed class EnemyMoveState : UnitMoveState
{
    private readonly EnemyController enemy;
    public EnemyMoveState(EnemyController enemy) : base(enemy) => this.enemy = enemy;

    public override void Update()
    {
        if (enemy.Navigation.ShouldReturnHome)
            enemy.StateMachine.ChangeState(enemy.ReturnHomeState);
    }

    public override void FixedUpdate()
    {
        if (enemy.Navigation.ShouldReturnHome)
        {
            enemy.StateMachine.ChangeState(enemy.ReturnHomeState);
            return;
        }
        if (enemy.IsTargetDetected) enemy.Navigation.BeginChasing();
        base.FixedUpdate();
    }

    protected override void MoveTowardsTarget()
    {
        if (enemy.Navigation.TryGetChaseDestination(out var destination))
            enemy.Move.MoveToX(destination.x);
        else enemy.Move.Stop();
    }
}
