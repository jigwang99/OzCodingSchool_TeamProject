using System;
using UnityEngine;

// 스테이지 하나의 드롭 규칙. 상위 스테이지일수록 희귀 등급 weight를 크게 준다.
[CreateAssetMenu(fileName = "StageDropTable", menuName = "Combat/Stage Drop Table")]
public class StageDropTable : ScriptableObject
{
    [Header("드롭 확률 (적 1마리 처치당 물고기 획득 확률)")]
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.7f;

    [Header("등급 가중치")]
    [SerializeField]
    private GradeWeight[] gradeWeights =
    {
        new GradeWeight { grade = FishGrade.Common, weight = 70f },
        new GradeWeight { grade = FishGrade.Rare,   weight = 25f },
        new GradeWeight { grade = FishGrade.Unique, weight = 5f },
    };

    public float DropChance => dropChance;
    public GradeWeight[] Weights => gradeWeights;

    [Serializable]
    public struct GradeWeight
    {
        public FishGrade grade;
        [Min(0f)] public float weight;
    }
}