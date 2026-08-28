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
    }

    private async UniTaskVoid AttackLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
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
