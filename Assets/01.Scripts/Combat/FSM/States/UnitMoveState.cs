using UnityEngine;
using StudioNAP; // AnimationTypeEnum

public class UnitMoveState : UnitBaseState
{
    public UnitMoveState(BaseUnitController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        controller.PlayAnimation(AnimationTypeEnum.Run);
    }

    public override void Exit()
    {
        controller.Move.Stop();
    }

    public override void FixedUpdate()
    {
        // 교전 대상 인식 해제 → 대기 (적: 플레이어가 감지범위 밖으로 이탈)
        if (!controller.IsTargetDetected)
        {
            controller.StateMachine.ChangeState(controller.IdleState);
            return;
        }

        if (controller.IsTargetInAttackRange)
        {
            controller.StateMachine.ChangeState(controller.CombatState);
            return;
        }

        // 플레이어: 앞으로 전진 / 적: 플레이어 추적
        controller.PerformMove();
    }

    public override void Update()
    {
    }
}