using UnityEngine;

using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class UnitMove : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 2f;
    [SerializeField, Min(0.1f)] private float jumpSpeed = 8f;
    [SerializeField, Min(0.1f)] private float dropSpeed = 2f;

    public const float TakeoffTolerance = 0.05f;
    public bool IsChangingFloors { get; private set; }
    private BaseUnitController owner;
    private Quaternion initialLocalRotation;
    private Transform facingRoot;
    private Vector3 initialVisualScale;
    private bool initiallyFacesRight;
    private ParticleSystemRenderer[] facingEffects;
    private Vector3[] originalEffectFlips;
    private float facingDirection = 1f;
    private float attackDirection = 1f;
    private Collider2D bodyCollider;
    private Collider2D landingPlatform;
    private readonly List<Collider2D> ignoredPlatforms = new();

    private Rigidbody2D unitRigidbody;
    private readonly List<Collider2D> collisionBuffer = new List<Collider2D>(2);

    private float skillSpeedMultiplier = 1f;
    public float MoveSpeed => moveSpeed * skillSpeedMultiplier;

    public event System.Action OnMoveSpeedChanged;

    public void SetSkillSpeedBonus(float bonus)
    {
        float previous = MoveSpeed;
        skillSpeedMultiplier = 1f + Mathf.Max(0f, bonus);
        if (previous != MoveSpeed) OnMoveSpeedChanged?.Invoke();
    }
    public bool IsMoving => unitRigidbody.linearVelocity.sqrMagnitude > 0f;

    private void Awake()
    {
        unitRigidbody = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        owner = GetComponent<BaseUnitController>();
        initialLocalRotation = transform.localRotation;
        unitRigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    public void ConfigureFacing(Transform visual, bool facesRight)
    {
        facingRoot = visual;
        initialVisualScale = visual.localScale;
        initiallyFacesRight = facesRight;
        facingEffects = visual.GetComponentsInChildren<ParticleSystemRenderer>(true);
        originalEffectFlips = new Vector3[facingEffects.Length];
        for (int i = 0; i < facingEffects.Length; i++)
        {
            originalEffectFlips[i] = facingEffects[i].flip;
            ParticleSystem particles = facingEffects[i].GetComponent<ParticleSystem>();
            if (particles == null) continue;
            var main = particles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
    }

    public void FaceDirection(float direction)
    {
        if (owner != null && owner.Attack != null && (owner.Attack.HasPendingHit || owner.IsFinishingAttack))
            direction = attackDirection;
        if (Mathf.Abs(direction) >= 0.001f) facingDirection = Mathf.Sign(direction);
        if (facingRoot == null) return;
        Vector3 scale = initialVisualScale;
        scale.x *= (facingDirection > 0f) == initiallyFacesRight ? 1f : -1f;
        facingRoot.localScale = scale;
        // 파티클 빌보드의 UV는 부모의 음수 스케일로 반전되지 않으므로 따로 맞춘다.
        bool mirrored = (facingDirection > 0f) != initiallyFacesRight;
        for (int i = 0; facingEffects != null && i < facingEffects.Length; i++)
        {
            if (facingEffects[i] == null) continue;
            Vector3 flip = originalEffectFlips[i];
            if (mirrored) flip.x = 1f - flip.x;
            facingEffects[i].flip = flip;
        }
    }

    public void CaptureAttackDirection(float direction)
    {
        attackDirection = Mathf.Abs(direction) >= 0.001f ? Mathf.Sign(direction) : facingDirection;
        FaceDirection(attackDirection);
    }

    private void LateUpdate()
    {
        if (owner != null && owner.isActiveAndEnabled && !owner.Health.IsDead)
            FaceDirection(facingDirection);
    }

    public void ResetMotion()
    {
        // Controller와 Module의 Awake 순서에 의존하지 않는다.
        if (unitRigidbody == null) Awake();
        CancelFloorTravel();
        Stop();
        unitRigidbody.angularVelocity = 0f;
        transform.localRotation = initialLocalRotation;
        unitRigidbody.rotation = transform.eulerAngles.z;
    }

    public void SnapToX(float x)
    {
        Vector2 position = unitRigidbody.position;
        position.x = x;
        unitRigidbody.position = position;
    }

    public void BeginFloorTravel(CombatFloorMap map, int nextFloor)
    {
        landingPlatform = map.GetFloorCollider(nextFloor);
        // 상승 중 발판 아래에 머리를 부딪히거나 하강 중 출발 발판에 걸리지 않게 한다.
        for (int i = 0; i < map.FloorCount; i++)
        {
            Collider2D platform = map.GetFloorCollider(i);
            if (platform == null || Physics2D.GetIgnoreCollision(bodyCollider, platform)) continue;
            Physics2D.IgnoreCollision(bodyCollider, platform, true);
            ignoredPlatforms.Add(platform);
        }
        IsChangingFloors = true;
        bool upward = map.GetStandingY(nextFloor) > unitRigidbody.position.y;
        unitRigidbody.linearVelocity = new Vector2(0f, upward ? jumpSpeed : -dropSpeed);
    }

    public void TickFloorTravel()
    {
        if (!IsChangingFloors) return;
        // 발바닥이 발판 위로 올라온 뒤 충돌을 복구한다. 착지는 물리 엔진이 처리한다.
        if (bodyCollider.bounds.min.y >= landingPlatform.bounds.max.y + 0.01f)
            Physics2D.IgnoreCollision(bodyCollider, landingPlatform, false);
        if (unitRigidbody.linearVelocity.y <= 0.1f && bodyCollider.IsTouching(landingPlatform))
            CancelFloorTravel();
    }

    public void CancelFloorTravel()
    {
        foreach (Collider2D platform in ignoredPlatforms)
            if (platform != null && bodyCollider != null)
                Physics2D.IgnoreCollision(bodyCollider, platform, false);
        ignoredPlatforms.Clear();
        landingPlatform = null;
        IsChangingFloors = false;
    }

    // 대상 쪽으로 이동 (y 무시, 좌우 라인 이동)
    public void MoveTo(Transform targetTransform)
    {
        MoveToX(targetTransform.position.x);
    }

    public void MoveToX(float x)
    {
        FaceDirection(x - transform.position.x);
        float speed = Mathf.Clamp((x - transform.position.x) / Time.fixedDeltaTime, -MoveSpeed, MoveSpeed);
        unitRigidbody.linearVelocity = new Vector2(speed, unitRigidbody.linearVelocity.y);
    }

    public void Stop()
    {
        unitRigidbody.linearVelocity = Vector2.zero;
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
