using UnityEngine;

public class PlayercatController : BaseUnitController
{
    [SerializeField] private AudioClip attackSound;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (Attack != null) Attack.OnAttackStarted += PlayAttackSound;
    }

    protected override void OnDisable()
    {
        if (Attack != null) Attack.OnAttackStarted -= PlayAttackSound;
        base.OnDisable();
    }

    private void PlayAttackSound()
    {
        if (attackSound != null) SoundManager.instance?.PlaySFX(attackSound);
    }

    public bool HasPendingEnemies { get; set; }
    public override bool IsTargetDetected => HasTarget || HasPendingEnemies;
    // 플레이어는 타겟 위치와 무관하게 항상 앞(+x)으로 전진한다.
    // (씬에서 전진 방향이 왼쪽이라면 Vector2.left 로 변경)
    public override void PerformMove()
    {
        Move.MoveInDirection(Vector2.right);
    }
}
