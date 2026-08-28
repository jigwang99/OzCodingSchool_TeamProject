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

    public void MoveTo(Vector2 direction)
    {
        unitRigidbody.linearVelocity = direction.sqrMagnitude > 0f
            ? direction.normalized * moveSpeed
            : Vector2.zero;
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
