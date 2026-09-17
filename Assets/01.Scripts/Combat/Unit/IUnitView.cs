using StudioNAP; // AnimationTypeEnum (현재 애니메이션 어휘로 사용 중인 enum)

// 유닛의 '비주얼/애니메이션' 계약.
// BaseUnitController(로직)는 이 인터페이스에만 의존하고,
// 실제로 무엇을(cat, crab 등) 어떻게 그리는지는 각 구현체(어댑터)가 책임진다.
// → 유닛 종류가 늘어도 BaseUnitController와 상태(FSM)는 바뀌지 않는다.
public interface IUnitView
{
    void RunAnimation(AnimationTypeEnum ani);
}

// 타격 후 남은 모션을 끝까지 보여주는 비주얼에서 선택적으로 구현한다.
public interface IAttackRecoveryView
{
    bool IsFinishingAttack { get; }
}
