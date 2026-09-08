using System;
public interface IPoolable
{
    Enum PoolKey { get; }
    void Init();
    void ReturnToPool();
}
public enum PoolType
{
    // 기존 3종의 값(0, 1, 2)은 각 0001 외형으로 유지한다. 직렬화된 번호를 변경하지 않는다.
    Crab_0001 = 0,
    Crab_0002 = 3,
    Crab_0003 = 4,
    Crab_0004 = 5,
    Crab_0005 = 6,

    Fishman_0001 = 1,
    Fishman_0002 = 7,
    Fishman_0003 = 8,
    Fishman_0004 = 9,
    Fishman_0005 = 10,

    HermitCrab_0001 = 2,
    HermitCrab_0002 = 11,
    HermitCrab_0003 = 12,
    HermitCrab_0004 = 13,
    HermitCrab_0005 = 14,
}
