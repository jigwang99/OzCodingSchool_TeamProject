using UnityEngine;

// 여러 층을 함께 보여주면서 플레이어의 가로 위치를 맵 범위 안에서 따라간다.
public class CombatCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float horizontalOffset = 4f;
    [SerializeField] private CombatFloorMap floorMap;
    private Camera viewCamera;

    private void Awake() => viewCamera = GetComponent<Camera>();

    private void LateUpdate() => SnapToTarget();

    public void SnapToTarget()
    {
        if (target == null)
            return;

        // 재시작/스테이지 선택으로 순간이동해도 그 프레임에 시작 위치로 돌아간다.
        Vector3 position = transform.position;
        position.x = target.position.x + horizontalOffset;
        if (floorMap != null && viewCamera != null && viewCamera.orthographic)
            position.x = floorMap.ClampCameraX(position.x, viewCamera.orthographicSize * viewCamera.aspect);
        transform.position = position;
    }
}
