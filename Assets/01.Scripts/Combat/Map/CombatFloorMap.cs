using UnityEngine;

using System.Collections.Generic;

// 0번은 연속된 지상. 나머지는 구역별 발판 ID이며 같은 높이에도 여러 발판이 있을 수 있다.
[ExecuteAlways]
public class CombatFloorMap : MonoBehaviour
{
    [SerializeField] private BoxCollider2D[] floors;
    [SerializeField, Min(0.1f)] private float standingOffset = 0.5f;
    [SerializeField, Min(0.05f)] private float floorTolerance = 0.65f;
    [Header("구역별 발판")]
    [SerializeField, Min(0.1f)] private float levelSpacing = 2.6f;
    [SerializeField, Min(0.6f)] private float zoneEdgePadding = 0.8f;
    [Header("발판 색상")]
    [SerializeField] private Color surfaceColor = new Color(0.94f, 0.78f, 0.46f);
    [SerializeField] private Color rockColor = new Color(0.36f, 0.29f, 0.24f);
    [SerializeField] private bool hideGroundVisual = true;
    [System.Serializable]
    private struct FloorPalette
    {
        public Color surface;
        public Color rock;
        public FloorPalette(Color surface, Color rock) { this.surface = surface; this.rock = rock; }
    }
    [SerializeField] private FloorPalette[] chapterPalettes =
    {
        new FloorPalette(new Color(0.96f, 0.83f, 0.5f), new Color(0.63f, 0.45f, 0.24f)),
        new FloorPalette(new Color(0.58f, 0.84f, 0.75f), new Color(0.13f, 0.43f, 0.53f)),
        new FloorPalette(new Color(0.19f, 0.39f, 0.61f), new Color(0.035f, 0.10f, 0.25f))
    };
    private int currentChapter;
    private GameObject visualRoot;
    private Sprite solidSprite;
    private Bounds[] visualBounds;
    private bool visualsDirty;
    private readonly List<(SpriteRenderer renderer, float surfaceBlend)> visualBlocks = new();
    private BoxCollider2D ground;
    private readonly List<BoxCollider2D> stagePlatforms = new(6);
    private int[] routePrevious;
    private int[] routeQueue;

    public int FloorCount => floors != null ? floors.Length : 0;
    public float GetStandingY(int floor) => floors[floor].bounds.max.y + standingOffset;
    public Collider2D GetFloorCollider(int floor) => floors[floor];

    public Vector3 GetGroundStartPosition()
    {
        Bounds bounds = floors[0].bounds;
        return new Vector3(bounds.center.x, GetStandingY(0), transform.position.z);
    }

    public void ApplyStageLayout(Vector3Int elevatedCounts, Vector2 horizontalRange)
    {
        if (FloorCount == 0 || floors[0] == null) return;
        if (ground == null)
        {
            ground = floors[0];
            // 씬에 있던 통짜 공중 발판 대신 구역별 콜라이더를 재사용한다.
            for (int i = 1; i < floors.Length; i++)
                if (floors[i] != null) floors[i].enabled = false;
        }
        float left = Mathf.Min(horizontalRange.x, horizontalRange.y);
        float right = Mathf.Max(horizontalRange.x, horizontalRange.y);
        float width = right - left;
        if (width < 6f) throw new System.ArgumentException("구역형 전투 맵의 폭은 6 이상이어야 합니다.");
        Bounds groundBounds = ground.bounds;
        float groundY = groundBounds.max.y + standingOffset;
        SetPlatformBounds(ground, new Vector2((left + right) * 0.5f, groundBounds.center.y),
            new Vector2(width, groundBounds.size.y));
        elevatedCounts = new Vector3Int(Mathf.Clamp(elevatedCounts.x, 0, 2),
            Mathf.Clamp(elevatedCounts.y, 0, 2), Mathf.Clamp(elevatedCounts.z, 0, 2));
        int count = elevatedCounts.x + elevatedCounts.y + elevatedCounts.z;
        while (stagePlatforms.Count < count)
        {
            var platform = new GameObject("StagePlatform_" + stagePlatforms.Count);
            platform.transform.SetParent(transform, false);
            platform.layer = ground.gameObject.layer;
            stagePlatforms.Add(platform.AddComponent<BoxCollider2D>());
        }
        if (floors.Length != count + 1) floors = new BoxCollider2D[count + 1];
        floors[0] = ground;
        float zoneWidth = width / 3f;
        float padding = Mathf.Clamp(zoneEdgePadding, 0.6f, (zoneWidth - 1.4f) * 0.5f);
        int slot = 0;
        for (int zone = 0; zone < 3; zone++)
        {
            int levels = elevatedCounts[zone];
            for (int level = 1; level <= levels; level++)
            {
                BoxCollider2D platform = stagePlatforms[slot];
                platform.gameObject.SetActive(true);
                SetPlatformBounds(platform, new Vector2(left + zoneWidth * (zone + 0.5f),
                    groundY + level * levelSpacing - standingOffset - 0.18f),
                    new Vector2(zoneWidth - 2f * padding, 0.36f));
                floors[++slot] = platform;
            }
        }
        for (int i = slot; i < stagePlatforms.Count; i++) stagePlatforms[i].gameObject.SetActive(false);
        // 전환 프레임의 스폰/경로 탐색이 갱신된 Collider bounds를 즉시 읽도록 한다.
        Physics2D.SyncTransforms();
        if (NeedsVisualRebuild()) BuildVisuals();
    }

    private static void SetPlatformBounds(BoxCollider2D platform, Vector2 center, Vector2 size)
    {
        platform.transform.position = new Vector3(center.x, center.y, platform.transform.position.z);
        platform.transform.localScale = Vector3.one;
        platform.offset = Vector2.zero;
        Vector3 scale = platform.transform.lossyScale;
        platform.size = new Vector2(size.x / Mathf.Abs(scale.x), size.y / Mathf.Abs(scale.y));
    }

    public Vector2 GetWalkableRange(int floor, float inset = 0.6f)
    {
        Bounds bounds = floors[floor].bounds;
        inset = Mathf.Clamp(inset, 0f, bounds.extents.x);
        return new Vector2(bounds.min.x + inset, bounds.max.x - inset);
    }

    public void ApplyChapterPalette(int chapter)
    {
        int nextChapter = Mathf.Max(0, chapter);
        bool paletteChanged = nextChapter != currentChapter;
        currentChapter = nextChapter;
        // 배경 교체와 같은 프레임(전환 페이드 안)에 발판도 교체한다.
        if (!isActiveAndEnabled) return;
        if (NeedsVisualRebuild()) BuildVisuals();
        else if (paletteChanged) ApplyVisualColors();
    }

    public float ClampCameraX(float x, float halfWidth)
    {
        if (FloorCount == 0) return x;
        float left = float.MaxValue, right = float.MinValue;
        foreach (BoxCollider2D floor in floors)
        {
            if (floor == null) continue;
            Bounds bounds = floor.bounds;
            left = Mathf.Min(left, bounds.min.x);
            right = Mathf.Max(right, bounds.max.x);
        }
        if (left > right) return x;
        return right - left <= halfWidth * 2f ? (left + right) * 0.5f : Mathf.Clamp(x, left + halfWidth, right - halfWidth);
    }

    private void OnEnable() => visualsDirty = true;
    private void OnValidate() => visualsDirty = true;

    private void LateUpdate()
    {
        if (NeedsVisualRebuild()) BuildVisuals();
    }

    private bool NeedsVisualRebuild()
    {
        if (visualsDirty || visualRoot == null || visualBounds == null || visualBounds.Length != FloorCount)
            return true;
        for (int i = 0; i < FloorCount; i++)
            if (visualBounds[i] != (floors[i] != null ? floors[i].bounds : default)) return true;
        return false;
    }

    private void OnDisable() => ClearVisuals();

    private void ClearVisuals()
    {
        Release(visualRoot);
        Release(solidSprite);
        visualRoot = null;
        solidSprite = null;
        visualBlocks.Clear();
    }

    private static void Release(Object value)
    {
        if (value == null) return;
        if (Application.isPlaying) Destroy(value);
        else DestroyImmediate(value);
    }

    private void BuildVisuals()
    {
        ClearVisuals();
        visualsDirty = false;
        visualBounds = new Bounds[FloorCount];
        visualRoot = new GameObject("Floor visuals (generated)") { hideFlags = HideFlags.HideAndDontSave };
        visualRoot.transform.SetParent(transform, false);
        solidSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        solidSprite.hideFlags = HideFlags.HideAndDontSave;
        for (int i = 0; i < FloorCount; i++)
        {
            if (floors[i] == null) continue;
            Bounds bounds = floors[i].bounds;
            visualBounds[i] = bounds;
            // 1층은 배경 바닥을 직접 딛도록 물리 발판만 유지한다.
            if (i == 0 && hideGroundVisual) continue;
            float top = bounds.max.y;
            DrawBlock("Rock", new Vector2(bounds.center.x, top - 0.18f), new Vector2(bounds.size.x, 0.36f), 0f);
            DrawBlock("Surface", new Vector2(bounds.center.x, top - 0.04f), new Vector2(bounds.size.x, 0.08f), 1f);
            for (float x = bounds.min.x + 0.4f; x < bounds.max.x; x += 1.4f)
                DrawBlock("Stone", new Vector2(x, top - 0.15f), new Vector2(0.6f, 0.08f), 0.25f);
        }
        ApplyVisualColors();
    }

    private void ApplyVisualColors()
    {
        Color surface = surfaceColor;
        Color rock = rockColor;
        if (chapterPalettes != null && currentChapter < chapterPalettes.Length)
        {
            surface = chapterPalettes[currentChapter].surface;
            rock = chapterPalettes[currentChapter].rock;
        }
        foreach (var block in visualBlocks)
            if (block.renderer != null) block.renderer.color = Color.Lerp(rock, surface, block.surfaceBlend);
    }

    private void DrawBlock(string label, Vector2 position, Vector2 size, float surfaceBlend)
    {
        var block = new GameObject(label) { hideFlags = HideFlags.HideAndDontSave };
        block.transform.SetParent(visualRoot.transform, false);
        block.transform.position = new Vector3(position.x, position.y, transform.position.z);
        block.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = solidSprite;
        visualBlocks.Add((renderer, surfaceBlend));
        renderer.sortingOrder = 0;
    }

    public int GetFloorIndex(Vector3 position)
    {
        int nearest = -1;
        float distance = floorTolerance;
        for (int i = 0; i < FloorCount; i++)
        {
            if (floors[i] == null) continue;
            Bounds bounds = floors[i].bounds;
            float delta = Mathf.Abs(position.y - (bounds.max.y + standingOffset));
            if (position.x < bounds.min.x - 0.1f || position.x > bounds.max.x + 0.1f || delta > distance)
                continue;
            nearest = i;
            distance = delta;
        }
        return nearest;
    }

    public Vector3 ClampToFloor(Vector3 position, int floor)
    {
        Bounds bounds = floors[floor].bounds;
        position.x = Mathf.Clamp(position.x, bounds.min.x + 0.6f, bounds.max.x - 0.6f);
        position.y = bounds.max.y + standingOffset;
        return position;
    }

    public bool TryGetVerticalJump(int from, int destination, float fromX,
        out int nextFloor, out float takeoffX)
    {
        nextFloor = from;
        takeoffX = fromX;
        if (from < 0 || destination < 0 || from >= FloorCount || destination >= FloorCount || from == destination)
            return false;
        // 높이 순서가 아닌 발판 연결 그래프를 탐색한다. 분리된 구역은 지상을 경유한다.
        if (routePrevious == null || routePrevious.Length != FloorCount)
        {
            routePrevious = new int[FloorCount];
            routeQueue = new int[FloorCount];
        }
        for (int i = 0; i < FloorCount; i++) routePrevious[i] = -1;
        int head = 0, tail = 0;
        routeQueue[tail++] = from;
        routePrevious[from] = from;
        while (head < tail && routePrevious[destination] < 0)
        {
            int current = routeQueue[head++];
            for (int candidate = 0; candidate < FloorCount; candidate++)
            {
                if (routePrevious[candidate] >= 0 || !ArePlatformsConnected(current, candidate)) continue;
                routePrevious[candidate] = current;
                routeQueue[tail++] = candidate;
            }
        }
        if (routePrevious[destination] < 0) return false;
        nextFloor = destination;
        while (routePrevious[nextFloor] != from) nextFloor = routePrevious[nextFloor];
        Bounds fromBounds = floors[from].bounds;
        Bounds nextBounds = floors[nextFloor].bounds;
        float left = Mathf.Max(fromBounds.min.x, nextBounds.min.x) + 0.6f;
        float right = Mathf.Min(fromBounds.max.x, nextBounds.max.x) - 0.6f;
        if (left > right) return false;
        takeoffX = Mathf.Clamp(fromX, left, right);
        return true;
    }

    private bool ArePlatformsConnected(int from, int to)
    {
        if (floors[from] == null || floors[to] == null) return false;
        Bounds a = floors[from].bounds;
        Bounds b = floors[to].bounds;
        float height = Mathf.Abs(a.max.y - b.max.y);
        if (height < 0.1f || height > levelSpacing + 0.1f) return false;
        return Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x) >= 1.2f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < FloorCount; i++)
            if (floors[i] != null)
            {
                Vector2 range = GetWalkableRange(i);
                Gizmos.DrawLine(new Vector3(range.x, GetStandingY(i), transform.position.z),
                    new Vector3(range.y, GetStandingY(i), transform.position.z));
            }
    }
}
