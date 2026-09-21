using UnityEngine;

using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class UnitMove : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 2f;

    private Rigidbody2D unitRigidbody;
    private readonly List<Collider2D> collisionBuffer = new List<Collider2D>(2);

    public float MoveSpeed => moveSpeed;
    public bool IsMoving => unitRigidbody.linearVelocity.sqrMagnitude > 0f;

    private void Awake()
    {
        unitRigidbody = GetComponent<Rigidbody2D>();
        unitRigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    // 지정한 방향으로 이동 (예: 플레이어 전진)
    public void MoveInDirection(Vector2 direction)
    {
        // 수평 이동 명령이 중력/착지를 덮어쓰지 않도록 세로 속도를 보존한다.
        unitRigidbody.linearVelocity = new Vector2(Mathf.Clamp(direction.x, -1f, 1f) * moveSpeed,
            unitRigidbody.linearVelocity.y);
    }

    // 대상 쪽으로 이동 (y 무시, 좌우 라인 이동)
    public void MoveTo(Transform targetTransform)
    {
        MoveToX(targetTransform.position.x);
    }

    public void MoveToX(float x)
    {
        float speed = Mathf.Clamp((x - transform.position.x) / Time.fixedDeltaTime, -moveSpeed, moveSpeed);
        unitRigidbody.linearVelocity = new Vector2(speed, unitRigidbody.linearVelocity.y);
    }

    public void Stop()
    {
        unitRigidbody.linearVelocity = Vector2.zero;
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed = Mathf.Max(0f, value);
    }

    // 유닛끼리 밀려나거나 복귀 길을 막지 않게 한다. 발판 충돌은 그대로 유지한다.
    // 풀에서 다시 활성화할 때마다 스포너가 호출한다.
    public void IgnoreUnitCollisions(UnitMove other)
    {
        if (other == null || other == this) return;
        // 풀 재활성화와 런타임 콜라이더 변경을 반영하면서 검색 결과 배열은 할당하지 않는다.
        GetComponentsInChildren<Collider2D>(collisionBuffer);
        other.GetComponentsInChildren<Collider2D>(other.collisionBuffer);
        foreach (Collider2D own in collisionBuffer)
            foreach (Collider2D obstacle in other.collisionBuffer)
                Physics2D.IgnoreCollision(own, obstacle);
    }
}
