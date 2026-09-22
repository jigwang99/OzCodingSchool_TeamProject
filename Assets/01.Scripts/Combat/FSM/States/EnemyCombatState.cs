public sealed class EnemyCombatState : UnitCombatState
{
    private readonly EnemyController enemy;
    public EnemyCombatState(EnemyController enemy) : base(enemy) => this.enemy = enemy;

    public override void Enter()
    {
        enemy.Navigation.BeginChasing();
        base.Enter();
    }

    public override void Update()
    {
        if (enemy.Navigation.ShouldReturnHome)
            enemy.StateMachine.ChangeState(enemy.ReturnHomeState);
        else base.Update();
    }
}
