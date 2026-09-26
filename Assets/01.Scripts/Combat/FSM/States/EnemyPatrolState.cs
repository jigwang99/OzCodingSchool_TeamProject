using UnityEngine;
using StudioNAP;

public sealed class EnemyPatrolState : UnitBaseState
{
    private readonly EnemyController enemy;
    private float direction;
    private float waitRemaining;
    private bool walking;

    public EnemyPatrolState(EnemyController enemy) : base(enemy) => this.enemy = enemy;
    public override void Enter()
    {
        direction = Random.value < 0.5f ? -1f : 1f;
        StartWaiting();
    }

    private void StartWaiting()
    {
        enemy.Move.Stop();
        enemy.PlayAnimation(AnimationTypeEnum.Idle);
        walking = false;
        waitRemaining = enemy.Navigation.GetWaitDuration();
    }

    public override void Exit() => enemy.Move.Stop();
    public override void Update()
    {
        if (!enemy.IsTargetDetected) return;
        enemy.Navigation.BeginChasing();
        enemy.StateMachine.ChangeState(enemy.IsTargetInAttackRange ? enemy.CombatState : enemy.MoveState);
    }

    public override void FixedUpdate()
    {
        if (enemy.IsTargetDetected) { Update(); return; }
        if (!enemy.Navigation.TryGetPatrolRange(out Vector2 range))
        {
            enemy.StateMachine.ChangeState(enemy.IdleState);
            return;
        }
        if (waitRemaining > 0f) { waitRemaining -= Time.fixedDeltaTime; return; }
        float destination = direction > 0f ? range.y : range.x;
        float delta = destination - enemy.transform.position.x;
        if (Mathf.Abs(delta) <= 0.03f)
        {
            direction = -direction;
            StartWaiting();
            return;
        }
        if (!walking) { enemy.PlayAnimation(AnimationTypeEnum.Run); walking = true; }
        float step = enemy.Move.MoveSpeed * enemy.Navigation.PatrolSpeedMultiplier * Time.fixedDeltaTime;
        enemy.Move.MoveToX(enemy.transform.position.x + Mathf.Clamp(delta, -step, step));
    }
}
