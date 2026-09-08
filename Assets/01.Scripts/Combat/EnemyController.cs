using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyController : BaseUnitController, IPoolable
{
    [SerializeField] private PoolType enemyType = PoolType.Crab_0001;
    [SerializeField, Min(0f)] private float detectionRange = 4f;
    [SerializeField, Min(0f)] private float despawnDelay = 0f; // 사망 후 반납까지 (연출 있으면 늘리기)

    private Rigidbody2D enemyRigidbody;
    private Quaternion initialLocalRotation;

    public float DetectionRange => detectionRange;
    public Enum PoolKey => enemyType;

    protected override void Awake()
    {
        base.Awake();
        enemyRigidbody = GetComponent<Rigidbody2D>();
        initialLocalRotation = transform.localRotation;

        // 좌우로 이동하는 적이 충돌 때문에 넘어지지 않도록 기존 제약에 회전 고정을 추가한다.
        enemyRigidbody.constraints |= RigidbodyConstraints2D.FreezeRotation;
    }

    protected override void OnEnable()
    {
        // 풀 재사용과 직접 재활성화 모두 이전 생명의 물리 상태를 남기지 않는다.
        enemyRigidbody.linearVelocity = Vector2.zero;
        enemyRigidbody.angularVelocity = 0f;
        transform.localRotation = initialLocalRotation;
        enemyRigidbody.rotation = transform.eulerAngles.z;
        base.OnEnable();
    }

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
        int lifeVersion = Health.LifeVersion;
        try
        {
            if (despawnDelay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(despawnDelay), cancellationToken: token);
            else
                await UniTask.NextFrame(token); // 최소 한 프레임: 사망 이벤트 처리 완료 보장

            if (isActiveAndEnabled && Health.IsDead && Health.LifeVersion == lifeVersion)
                CombatObjectPoolManager.instance.ReturnObject(PoolKey, gameObject);
        }
        catch (OperationCanceledException) { }
    }
}
