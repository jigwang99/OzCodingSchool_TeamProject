using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class UnitMove : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 2f;

    private Rigidbody2D unitRigidbody;

    public float MoveSpeed => moveSpeed;
    public bool IsMoving => unitRigidbody.linearVelocity.sqrMagnitude > 0f;

    private void Awake()
    {
        unitRigidbody = GetComponent<Rigidbody2D>();
    }

    // 지정한 방향으로 이동 (예: 플레이어 전진)
    public void MoveInDirection(Vector2 direction)
    {
        unitRigidbody.linearVelocity = direction.sqrMagnitude > 0f
            ? direction.normalized * moveSpeed
            : Vector2.zero;
    }

    // 대상 쪽으로 이동 (y 무시, 좌우 라인 이동)
    public void MoveTo(Transform targetTransform)
    {
        Vector2 dir = targetTransform.position - transform.position;
        dir.y = 0f;
        MoveInDirection(dir);
    }

    public void Stop()
    {
        unitRigidbody.linearVelocity = Vector2.zero;
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed = Mathf.Max(0f, value);
    }
}