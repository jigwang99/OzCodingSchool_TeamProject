using UnityEngine;

public sealed class PlayerMoveState : UnitMoveState
{
    private readonly PlayercatController player;
    private readonly UnitJumpState jumpState;

    public PlayerMoveState(PlayercatController player) : base(player)
    {
        this.player = player;
        jumpState = new UnitJumpState(player);
    }

    protected override void MoveTowardsTarget()
    {
        if (!player.HasTarget && !player.HasSearchPosition) { player.Move.Stop(); return; }
        Vector3 destination = player.HasTarget ? player.Target.transform.position : player.SearchPosition;
        CombatFloorMap map = player.FloorMap;
        if (map != null)
        {
            int current = player.CurrentFloor;
            int goal = map.GetFloorIndex(destination);
            if (current < 0 || goal < 0) { player.Move.Stop(); return; }
            if (current != goal)
            {
                if (!map.TryGetVerticalJump(current, goal, player.transform.position.x,
                    out int next, out float takeoffX)) { player.Move.Stop(); return; }
                // 마찰로 도약점 직전에 멈추는 회귀를 막는다.
                if (Mathf.Abs(takeoffX - player.transform.position.x) > UnitMove.TakeoffTolerance)
                    player.Move.MoveToX(takeoffX);
                else
                {
                    jumpState.SetDestination(next);
                    player.StateMachine.ChangeState(jumpState);
                }
                return;
            }
            destination = map.ClampToFloor(destination, current);
        }
        player.Move.MoveToX(destination.x);
    }
}
