using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyController : BaseUnitController, IPoolable
{
    [SerializeField, Min(0f)] private float detectionRange = 4f;
    [SerializeField, Min(0f)] private float despawnDelay = 0f; // 사망 후 반납까지 (연출 있으면 늘리기)

    public float DetectionRange => detectionRange;
    public Enum PoolKey => PoolType.Enemy;

    public void Init() => Revive();
    public void ReturnToPool() => PrepareForPool();

    public override bool IsTargetDetected =>
        HasTarget && Mathf.Abs(transform.position.x - Target.transform.position.x) <= detectionRange;

    // 사망 시 호출: 이번 프레임 이벤트(드롭/리타겟)가 끝난 뒤 풀로 반납.
    public void DespawnAfterDeath()
    {
        DespawnAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTaskVoid DespawnAsync(CancellationToken token)
    {
        try
        {
            if (despawnDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(despawnDelay), cancellationToken: token);
            else
                await UniTask.NextFrame(token); // 최소 한 프레임: 사망 이벤트 처리 완료 보장

            CombatObjectPoolManager.instance.ReturnObject(PoolType.Enemy, gameObject);
        }
        catch (OperationCanceledException) { }
    }
}