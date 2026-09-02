using UnityEngine;

public readonly struct FishDrop
{
    public readonly FishGrade Grade;
    public readonly int Count;
    public readonly Vector3 SourcePosition; // 적이 죽은 위치 (획득 연출용)

    public FishDrop(FishGrade grade, int count, Vector3 sourcePosition)
    {
        Grade = grade;
        Count = count;
        SourcePosition = sourcePosition;
    }
}