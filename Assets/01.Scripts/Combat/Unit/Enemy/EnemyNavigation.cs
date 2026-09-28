using UnityEngine;

// 적의 이동 판단에 필요한 데이터와 탐지/경로 계산. 상태 전환은 State에서 담당한다.
public sealed class EnemyNavigation
{
    private readonly EnemyController enemy;
    private readonly float detectionRange;
    private readonly float patrolRadius;
    private readonly Vector2 waitRange;
    public float PatrolSpeedMultiplier { get; }
    public Vector3 HomePosition { get; private set; }
    public bool IsChasing { get; private set; }
    public bool IsReturningHome { get; private set; }

    public EnemyNavigation(EnemyController enemy, float detectionRange, float patrolRadius,
        float patrolSpeedMultiplier, Vector2 waitRange)
    {
        this.enemy = enemy;
        this.detectionRange = detectionRange;
        this.patrolRadius = patrolRadius;
        PatrolSpeedMultiplier = Mathf.Clamp01(patrolSpeedMultiplier);
        this.waitRange = waitRange;
    }

    public bool IsTargetDetected => !IsReturningHome && enemy.HasTarget && enemy.IsOnSameFloor(enemy.Target) &&
        (IsChasing || Mathf.Abs(enemy.transform.position.x - enemy.Target.transform.position.x) <= detectionRange);
    public bool ShouldReturnHome => IsChasing && (!enemy.HasTarget || !enemy.IsOnSameFloor(enemy.Target));
    public void BeginChasing() => IsChasing = true;
    public void BeginReturn() { IsChasing = false; IsReturningHome = true; }
    public void ResetTracking() { IsChasing = false; IsReturningHome = false; }
    public void ResetHome() { HomePosition = enemy.transform.position; ResetTracking(); }

    public float GetWaitDuration()
    {
        float min = Mathf.Max(0f, Mathf.Min(waitRange.x, waitRange.y));
        return Random.Range(min, Mathf.Max(min, Mathf.Max(waitRange.x, waitRange.y)));
    }

    public bool TryGetPatrolRange(out Vector2 range)
    {
        range = new Vector2(HomePosition.x - patrolRadius, HomePosition.x + patrolRadius);
        if (patrolRadius <= 0f) return false;
        if (enemy.FloorMap != null)
        {
            int floor = enemy.FloorMap.GetFloorIndex(HomePosition);
            if (floor < 0) return false;
            Vector2 walkable = enemy.FloorMap.GetWalkableRange(floor);
            range.x = Mathf.Max(range.x, walkable.x);
            range.y = Mathf.Min(range.y, walkable.y);
        }
        return range.y - range.x > 0.06f;
    }

    public bool TryGetChaseDestination(out Vector3 destination)
    {
        destination = enemy.HasTarget ? enemy.Target.transform.position : HomePosition;
        if (!IsTargetDetected) return false;
        if (enemy.FloorMap == null) return true;
        int floor = enemy.FloorMap.GetFloorIndex(HomePosition);
        if (floor < 0) return false;
        destination = enemy.FloorMap.ClampToFloor(destination, floor);
        return true;
    }
}
