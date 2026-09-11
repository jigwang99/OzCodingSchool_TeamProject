using UnityEngine;
using StudioNAP; // UnitController, AnimationTypeEnum

// 플레이어(cat) 비주얼 어댑터.
// 애셋 원본(UnitController)을 수정하지 않고 IUnitView로 감싸 위임한다.
// cat19 오브젝트(= UnitController가 붙은 오브젝트)에 함께 붙인다.
[RequireComponent(typeof(UnitController))]
public class CatUnitView : MonoBehaviour, IUnitView, IAttackRecoveryView
{
    private static readonly int Attack0Hash = Animator.StringToHash("cat0Shoot0");
    private static readonly int Attack1Hash = Animator.StringToHash("cat0Shoot1");
    [SerializeField] private UnitController controller;
    private UnitAttack unitAttack;
    private Animator animator;
    private bool attackHitResolved;

    public bool IsFinishingAttack
    {
        get
        {
            if (!attackHitResolved || animator == null || !animator.isActiveAndEnabled)
                return false;
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            // 고정 지연 대신 실제 클립 진행도를 사용해 두 공격의 다른 길이도 반영한다.
            return state.normalizedTime < 1f &&
                (state.shortNameHash == Attack0Hash || state.shortNameHash == Attack1Hash);
        }
    }

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<UnitController>();
        unitAttack = GetComponentInParent<UnitAttack>();
        animator = GetComponent<Animator>();
    }

    public void RunAnimation(AnimationTypeEnum ani)
    {
        attackHitResolved = false;
        if (animator != null && (ani == AnimationTypeEnum.Attack0 || ani == AnimationTypeEnum.Attack1))
        {
            // 같은 공격 상태에 재진입해도 반드시 첫 프레임부터 재생한다.
            animator.Play(ani == AnimationTypeEnum.Attack0 ? Attack0Hash : Attack1Hash, 0, 0f);
            return;
        }
        if (controller != null)
            controller.RunAnimation(ani);
    }

    // 전투용 공격 클립의 AnimationEvent 수신. 0 = Attack0, 1 = Attack1.
    public void OnAttackImpact(int animationIndex)
    {
        if (isActiveAndEnabled && unitAttack != null && unitAttack.ResolveAnimationHit(animationIndex))
            attackHitResolved = true;
    }

    private void OnDisable() => attackHitResolved = false;
}
