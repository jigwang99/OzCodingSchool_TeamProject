using System;
using System.Collections.Generic;
using UnityEngine;

public enum Rarity
{
    Common,
    Rare,
    Unique,
    Epic
}

/// <summary>
/// 풀 전체를 ScriptableObject로 만들어 에디터에서 쉽게 편집하도록 함
/// </summary>
[CreateAssetMenu(menuName = "Gacha/GachaPool")]
public class GachaPool : ScriptableObject
{
    public string poolId;
    public string displayName;
    public List<GachaGroup> groups = new List<GachaGroup>();
}

[Serializable]
public class GachaGroup
{
    public string groupName;
    public Rarity rarity;
    [Tooltip("그룹이 뽑힐 확률(가중치)")]
    public float weight = 1f;

    [Tooltip("이 그룹 내부의 아이템 목록과 가중치")]
    public List<GachaEntry> entries = new List<GachaEntry>();
}

[Serializable]
public class GachaEntry
{
    public string id;
    public GameObject prefab; // 또는 ScriptableObject나 데이터 참조로 변경 가능
    [Tooltip("아이템 뽑힐 확률(가중치)")]
    public float weight = 1f;
}