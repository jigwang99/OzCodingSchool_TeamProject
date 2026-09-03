using System;
using UnityEngine;

// 스테이지 하나의 드롭 규칙. 상위 스테이지일수록 희귀 등급 weight를 크게 준다.
[CreateAssetMenu(fileName = "StageDropTable", menuName = "Combat/Stage Drop Table")]
public class StageDropTable : ScriptableObject
{
    [Header("드롭 확률 (적 1마리 처치당 물고기 획득 확률)")]
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.7f;

    [Header("등급 가중치 + 등급별 종 구성")]
    [SerializeField]
    private GradeWeight[] gradeWeights =
    {
        new GradeWeight { grade = FishGrade.Common, weight = 70f, speciesCount = 8 },
        new GradeWeight { grade = FishGrade.Rare,   weight = 25f, speciesCount = 4 },
        new GradeWeight { grade = FishGrade.Unique, weight = 5f,  speciesCount = 2 },
    };

    public float DropChance => dropChance;
    public GradeWeight[] Weights => gradeWeights;

    [Serializable]
    public struct GradeWeight
    {
        public FishGrade grade;

        [Min(0f)] public float weight;      // 등급 선택 가중치

        [Min(1)] public int speciesCount;   // 이 등급의 종 수 (Common 8, Rare 4, Unique 2, Epic 1)

        // (선택) 종별 가중치. 비워두면 speciesCount 범위에서 균등 추첨.
        // 채울 경우 배열 길이를 speciesCount와 맞출 것.
        public float[] speciesWeights;
    }
}