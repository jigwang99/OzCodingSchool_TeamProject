using UnityEngine;

// 긴 전투 구간에서도 플레이어 앞쪽이 보이도록 가로 위치만 따라간다.
public class CombatCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float horizontalOffset = 4f;

    private void LateUpdate() => SnapToTarget();

    public void SnapToTarget()
    {
        if (target == null)
            return;

        // 재시작/스테이지 선택으로 순간이동해도 그 프레임에 시작 위치로 돌아간다.
        Vector3 position = transform.position;
        position.x = target.position.x + horizontalOffset;
        transform.position = position;
    }
}
