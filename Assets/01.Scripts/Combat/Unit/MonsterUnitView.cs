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

    private void Awake()
    {
        if (monster == null)
            monster = GetComponent<MonsterPrefabController>();

        // 애셋 흐름에서는 MonsterParent가 Init을 호출하지만, 여기선 단독 사용이므로 직접 호출한다.
        // (Init이 Animator를 찾고 클립 이름 목록을 캐시한다 → PlayAnimation 전제 조건)
        monster.Init();

        if (logClipNames)
        {
            var names = monster.GetAnimationNames();
            Debug.Log($"[MonsterUnitView] {name} 클립 목록: " +
                      (names != null && names.Count > 0 ? string.Join(", ", names) : "(없음)"));
        }
    }

    public void RunAnimation(AnimationTypeEnum ani)
    {
        string clip = ResolveClip(ani);
        if (string.IsNullOrEmpty(clip))
            return;

        // 존재하지 않는 이름이면 MonsterPrefabController가 경고만 남기고 무시하므로 안전하다.
        monster.PlayAnimation(clip, crossFade, 0f);
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
