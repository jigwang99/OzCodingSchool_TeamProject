using UnityEngine;

public class PlayercatController : BaseUnitController
{
    // 플레이어는 타겟 위치와 무관하게 항상 앞(+x)으로 전진한다.
    // (씬에서 전진 방향이 왼쪽이라면 Vector2.left 로 변경)
    public override void PerformMove()
    {
        Move.MoveInDirection(Vector2.right);
    }
}