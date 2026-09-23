public sealed class EnemyIdleState : UnitIdleState
{
    private readonly EnemyController enemy;
    public EnemyIdleState(EnemyController enemy) : base(enemy) => this.enemy = enemy;

    public override void Update()
    {
        if (enemy.IsTargetDetected)
        {
            enemy.Navigation.BeginChasing();
            base.Update();
        }
        else if (enemy.Navigation.TryGetPatrolRange(out _))
            enemy.StateMachine.ChangeState(enemy.PatrolState);
    }
}
