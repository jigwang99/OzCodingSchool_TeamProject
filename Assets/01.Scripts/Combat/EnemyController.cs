using System;
using UnityEngine;

public class EnemyController : BaseUnitController, IPoolable
{
    [SerializeField, Min(0f)] private float detectionRange = 4f;

    public float DetectionRange => detectionRange;

    // 오브젝트 풀 등록 키
    public Enum PoolKey => PoolType.Enemy;

    // 풀에서 꺼낼 때: HP/상태 초기화
    public void Init()
    {
        Revive();
    }

    // 풀로 반납할 때: 진행 중 전투 루프/이동/타겟 정리
    public void ReturnToPool()
    {
        PrepareForPool();
    }

    // 플레이어가 감지범위(x거리) 안에 들어오면 교전(추적) 시작.
    // TODO: 추후 트리거 콜라이더 기반 감지로 교체 (지금은 x거리 계산)
    public override bool IsTargetDetected =>
        HasTarget && Mathf.Abs(transform.position.x - Target.transform.position.x) <= detectionRange;

    // 에디터에서 감지범위 확인용 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 left = transform.position + Vector3.left * detectionRange;
        Vector3 right = transform.position + Vector3.right * detectionRange;
        Gizmos.DrawLine(left, right);
        Gizmos.DrawLine(left + Vector3.up * 0.5f, left + Vector3.down * 0.5f);
        Gizmos.DrawLine(right + Vector3.up * 0.5f, right + Vector3.down * 0.5f);
    }
}