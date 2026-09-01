using System;
public interface IPoolable
{
    Enum PoolKey { get; }
    void Init();
    void ReturnToPool();
}
public enum PoolType
{
    Enemy,
    // 추후 확장 예: 적 종류별 분리(Crab, KingCrab, Boss ...), Fish, HitEffect 등
}