using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class UnitCombatState : UnitBaseState
{
    private CancellationTokenSource attackCancellationTokenSource;

    public UnitCombatState(BaseUnitController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        controller.Move.Stop();
        attackCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            controller.GetCancellationTokenOnDestroy());
        AttackLoopAsync(attackCancellationTokenSource.Token).Forget();
    }

    public override void Exit()
    {
        if (attackCancellationTokenSource == null)
        {
            return;
        }

        attackCancellationTokenSource.Cancel();
        attackCancellationTokenSource.Dispose();
        attackCancellationTokenSource = null;
    }

    public override void FixedUpdate()
    {
    }

    public override void Update()
    {
        // 현재 대상이 사거리 안이면 공격 루프가 처리 중이므로 유지.
        if (controller.IsTargetInAttackRange)
        {
            return;
        }

        // 대상이 죽었거나(타겟 교체) 사거리를 벗어남 → 재교전 위해 복귀.
        // 다음 가까운 적으로 이어서 전진/공격하기 위한 핵심 전환.
        controller.StateMachine.ChangeState(
            controller.IsTargetDetected ? controller.MoveState : controller.IdleState);
    }

    private async UniTaskVoid AttackLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            // 대상이 도중에 교체돼도 controller.Target 기준으로 계속 공격.
            while (!cancellationToken.IsCancellationRequested && controller.IsTargetInAttackRange)
            {
                if (!controller.TryAttackTarget())
                {
                    break;
                }

                await UniTask.Delay(
                    TimeSpan.FromSeconds(controller.Attack.AttackInterval),
                    cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}