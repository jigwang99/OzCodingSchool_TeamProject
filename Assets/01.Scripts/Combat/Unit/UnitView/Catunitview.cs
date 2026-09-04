using UnityEngine;
using StudioNAP; // UnitController, AnimationTypeEnum

// 플레이어(cat) 비주얼 어댑터.
// 애셋 원본(UnitController)을 수정하지 않고 IUnitView로 감싸 위임한다.
// cat19 오브젝트(= UnitController가 붙은 오브젝트)에 함께 붙인다.
[RequireComponent(typeof(UnitController))]
public class CatUnitView : MonoBehaviour, IUnitView
{
    [SerializeField] private UnitController controller;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<UnitController>();
    }

    public void RunAnimation(AnimationTypeEnum ani)
    {
        if (controller != null)
            controller.RunAnimation(ani);
    }
}