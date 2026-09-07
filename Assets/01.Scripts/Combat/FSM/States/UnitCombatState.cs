using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using StudioNAP;

public class UnitCombatState : UnitBaseState
{
    private CancellationTokenSource attackCancellationTokenSource;
    private bool useSecondAttack;

    public UnitCombatState(BaseUnitController controller) : base(controller) { }

    public override void Enter()
    {
        controller.Move.Stop();
        attackCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            controller.GetCancellationTokenOnDestroy());
        AttackLoopAsync(attackCancellationTokenSource.Token).Forget();
    }

    public override void Exit()
    {
        controller.Attack.CancelPendingHit();
        if (attackCancellationTokenSource == null) return;
        attackCancellationTokenSource.Cancel();
        attackCancellationTokenSource.Dispose();
        attackCancellationTokenSource = null;
    }

    public override void FixedUpdate() { }

    public override void Update()
    {
        // 준비 중에 대상이 바뀌면 새 적에게 이전 공격을 넘기지 않는다.
        bool invalidPending = controller.Attack.HasPendingHit && !controller.Attack.IsPendingTargetValid;
        if (!invalidPending && controller.IsTargetInAttackRange)
            return;
        controller.StateMachine.ChangeState(
            controller.IsTargetDetected ? controller.MoveState : controller.IdleState);
    }

    private async UniTaskVoid AttackLoopAsync(CancellationToken cancellationToken)
    {
        int attackId = 0;
        try
        {
            while (!cancellationToken.IsCancellationRequested && controller.isActiveAndEnabled
                   && !controller.Health.IsDead && controller.IsTargetInAttackRange)
            {
                if (controller.Attack.CooldownRemaining > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(controller.Attack.CooldownRemaining),
                        cancellationToken: cancellationToken);
                    continue;
                }

                int animationIndex = useSecondAttack ? 1 : 0;
                attackId = controller.Attack.BeginAttack(controller.Target, animationIndex);
                if (attackId == 0) break;
                controller.PlayAnimation(useSecondAttack ? AnimationTypeEnum.Attack1 : AnimationTypeEnum.Attack0);
                useSecondAttack = !useSecondAttack;

                if (!controller.Attack.UseAnimationEvents)
                {
                    // 애니메이션 이벤트가 없는 적/임시 유닛은 설정한 준비 시간 뒤 판정한다.
                    await UniTask.Delay(TimeSpan.FromSeconds(controller.Attack.HitDelay),
                        cancellationToken: cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    controller.Attack.TryResolveHit(attackId);
                }

                // 준비 시간을 포함해 공격 시작 간격을 유지한다. 이벤트가 누락돼도 지연 타격을 만들지 않는다.
                await UniTask.Delay(TimeSpan.FromSeconds(controller.Attack.CooldownRemaining),
                    cancellationToken: cancellationToken);
                controller.Attack.CancelPendingHit(attackId);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            // 이전 루프의 finally가 새 공격 예약을 취소하지 않도록 ID를 검사한다.
            controller.Attack.CancelPendingHit(attackId);
        }
    }
}
