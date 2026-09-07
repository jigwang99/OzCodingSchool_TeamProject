using UnityEngine;
using StudioNAP;                 // AnimationTypeEnum
using SP1Assets.MonsterPack2D;   // MonsterPrefabController

// 적(MonsterPack2D 크랩 등) 비주얼 어댑터.
// BaseUnitController의 AnimationTypeEnum 요청을 이 몬스터의 '실제 클립 이름'으로 매핑해
// 애셋의 MonsterPrefabController.PlayAnimation(string)에 위임한다.
// 애셋 원본은 수정하지 않는다.
//
// 배치: 크랩 프리팹(MonsterPrefabController가 붙은 오브젝트)에 함께 붙인다.
[RequireComponent(typeof(MonsterPrefabController))]
public class MonsterUnitView : MonoBehaviour, IUnitView
{
    [SerializeField] private MonsterPrefabController monster;

    [Header("AnimationTypeEnum → 이 몬스터의 클립 이름 (Animator에 있는 정확한 이름)")]
    [SerializeField] private string idleClip = "idle";
    [SerializeField] private string runClip = "walk";
    [SerializeField] private string attack0Clip = "attack";
    [SerializeField] private string attack1Clip = "attack";
    [SerializeField] private string deadClip = "die";

    [Header("전환")]
    [SerializeField, Min(0f)] private float crossFade = 0.1f; // 0이면 즉시 전환(Play), >0이면 CrossFade

    [Header("디버그")]
    [Tooltip("켜면 시작 시 이 몬스터가 가진 클립 이름을 콘솔에 전부 출력한다. (매핑 값 채울 때 사용)")]
    [SerializeField] private bool logClipNames = false;

    private Animator animator;
    private Transform[] poseTransforms;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;
    private Vector3[] initialScales;
    private bool playingDeath;

    private void Awake()
    {
        EnsureInitialized();

        if (logClipNames)
        {
            var names = monster.GetAnimationNames();
            Debug.Log($"[MonsterUnitView] {name} 클립 목록: " +
                      (names != null && names.Count > 0 ? string.Join(", ", names) : "(없음)"));
        }
    }

    private void OnEnable() => ResetPose();

    private void EnsureInitialized()
    {
        if (poseTransforms != null)
            return;

        if (monster == null)
            monster = GetComponent<MonsterPrefabController>();

        // 애셋 흐름에서는 MonsterParent가 Init을 호출하지만, 여기선 단독 사용이므로 직접 호출한다.
        // (Init이 Animator를 찾고 클립 이름 목록을 캐시한다 → PlayAnimation 전제 조건)
        monster.Init();
        animator = monster.GetAnimator();
        if (animator == null)
            return;

        // 부모 컨트롤러가 먼저 애니메이션을 요청해도 최초 재생 전에 원래 자세를 저장한다.
        poseTransforms = animator.GetComponentsInChildren<Transform>(true);
        initialPositions = new Vector3[poseTransforms.Length];
        initialRotations = new Quaternion[poseTransforms.Length];
        initialScales = new Vector3[poseTransforms.Length];
        for (int i = 0; i < poseTransforms.Length; i++)
        {
            initialPositions[i] = poseTransforms[i].localPosition;
            initialRotations[i] = poseTransforms[i].localRotation;
            initialScales[i] = poseTransforms[i].localScale;
        }
    }

    public void ResetPose()
    {
        EnsureInitialized();
        if (animator == null || !animator.isActiveAndEnabled)
            return;

        // die가 바꾼 bone_center의 회전/위치/크기는 idle에 키가 없어 그대로 남을 수 있다.
        // Rebind 전에 원래 자세를 복구해 사망 자세가 새 기본값으로 잡히지 않게 한다.
        for (int i = 0; i < poseTransforms.Length; i++)
        {
            Transform bone = poseTransforms[i];
            if (bone == null)
                continue;

            bone.localPosition = initialPositions[i];
            bone.localRotation = initialRotations[i];
            bone.localScale = initialScales[i];
        }

        animator.Rebind();
        monster.PlayAnimation(idleClip, 0f, 0f);
        animator.Update(0f); // 렌더링 전에 대기 자세를 즉시 적용한다.
        playingDeath = false;
    }

    public void RunAnimation(AnimationTypeEnum ani)
    {
        EnsureInitialized();
        if (animator == null)
            return;

        string clip = ResolveClip(ani);
        if (string.IsNullOrEmpty(clip))
            return;

        // 풀 반납 또는 직접 부활 시 사망 모션과 다음 모션을 섞지 않는다.
        bool leavingDeath = playingDeath && ani != AnimationTypeEnum.Dead;
        if (leavingDeath)
            ResetPose();

        // 존재하지 않는 이름이면 MonsterPrefabController가 경고만 남기고 무시하므로 안전하다.
        monster.PlayAnimation(clip, leavingDeath ? 0f : crossFade, 0f);
        playingDeath = ani == AnimationTypeEnum.Dead;
    }

    private string ResolveClip(AnimationTypeEnum ani)
    {
        switch (ani)
        {
            case AnimationTypeEnum.Idle: return idleClip;
            case AnimationTypeEnum.Run: return runClip;
            case AnimationTypeEnum.Attack0: return attack0Clip;
            case AnimationTypeEnum.Attack1: return attack1Clip;
            case AnimationTypeEnum.Dead: return deadClip;
            default: return idleClip;
        }
    }
}
