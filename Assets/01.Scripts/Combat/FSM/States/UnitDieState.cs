using UnityEngine;
using StudioNAP; // AnimationTypeEnum

public class UnitDieState : UnitBaseState
{
    public UnitDieState(BaseUnitController controller) : base(controller) { }

    public override void Enter()
    {
        controller.Move.Stop();
        controller.PlayAnimation(AnimationTypeEnum.Dead);

        // 적만 자동 반납. 플레이어는 Revive로 되살아나야 하므로 제외.
        // 사망 모션을 보여주려면 EnemyController.despawnDelay를 애니 길이만큼 늘릴 것.
        if (controller is EnemyController enemy)
            enemy.DespawnAfterDeath();
    }

    public override void Exit() { }
    public override void FixedUpdate() { }
    public override void Update() { }
}