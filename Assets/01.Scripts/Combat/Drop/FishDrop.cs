using UnityEngine;

public readonly struct FishDrop
{
    public readonly FishGrade Grade;
    public readonly int Species;            // 등급 안에서의 종 인덱스 (0-based)
    public readonly int Count;
    public readonly Vector3 SourcePosition; // 적이 죽은 위치 (획득 연출용)

    public FishDrop(FishGrade grade, int species, int count, Vector3 sourcePosition)
    {
        Grade = grade;
        Species = species;
        Count = count;
        SourcePosition = sourcePosition;
    }
}