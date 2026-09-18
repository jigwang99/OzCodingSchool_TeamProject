using UnityEngine;

// 아래에서 위 순서로 실제 발판과 인접 층의 연결 X 좌표를 지정한다.
[ExecuteAlways]
public class CombatFloorMap : MonoBehaviour
{
    [SerializeField] private BoxCollider2D[] floors;
    [SerializeField] private float[] connectionX;
    [SerializeField, Min(0.1f)] private float standingOffset = 0.5f;
    [SerializeField, Min(0.05f)] private float floorTolerance = 0.65f;
    [Header("발판 색상")]
    [SerializeField] private Color surfaceColor = new Color(0.94f, 0.78f, 0.46f);
    [SerializeField] private Color rockColor = new Color(0.36f, 0.29f, 0.24f);
    [SerializeField] private Color connectionColor = new Color(0.4f, 0.95f, 0.9f, 0.8f);
    private GameObject visualRoot;
    private Sprite solidSprite;
    private Bounds[] visualBounds;
    private bool visualsDirty;

    public int FloorCount => floors != null ? floors.Length : 0;
    public float GetStandingY(int floor) => floors[floor].bounds.max.y + standingOffset;

    public float ClampCameraX(float x, float halfWidth)
    {
        if (FloorCount == 0) return x;
        float left = float.MaxValue, right = float.MinValue;
        foreach (BoxCollider2D floor in floors)
        {
            if (floor == null) continue;
            left = Mathf.Min(left, floor.bounds.min.x);
            right = Mathf.Max(right, floor.bounds.max.x);
        }
        if (left > right) return x;
        return right - left <= halfWidth * 2f ? (left + right) * 0.5f : Mathf.Clamp(x, left + halfWidth, right - halfWidth);
    }

    private void OnEnable() => visualsDirty = true;
    private void OnValidate() => visualsDirty = true;

    private void LateUpdate()
    {
        bool rebuild = visualsDirty || visualRoot == null || visualBounds == null || visualBounds.Length != FloorCount;
        if (!rebuild)
            for (int i = 0; i < FloorCount; i++)
                if (floors[i] != null && visualBounds[i] != floors[i].bounds) { rebuild = true; break; }
        if (rebuild) BuildVisuals();
    }

    private void OnDisable() => ClearVisuals();

    private void ClearVisuals()
    {
        Release(visualRoot);
        Release(solidSprite);
        visualRoot = null;
        solidSprite = null;
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
            float top = bounds.max.y;
            DrawBlock("Rock", new Vector2(bounds.center.x, top - 0.18f), new Vector2(bounds.size.x, 0.36f), rockColor);
            DrawBlock("Sand", new Vector2(bounds.center.x, top - 0.04f), new Vector2(bounds.size.x, 0.08f), surfaceColor);
            for (float x = bounds.min.x + 0.4f; x < bounds.max.x; x += 1.4f)
                DrawBlock("Stone", new Vector2(x, top - 0.15f), new Vector2(0.6f, 0.08f), Color.Lerp(rockColor, surfaceColor, 0.25f));
        }
        for (int i = 0; i < FloorCount - 1; i++)
            if (TryGetConnection(i, i + 1, out int next, out float x))
                for (int floor = i; floor <= next; floor++)
                {
                    float y = floors[floor].bounds.max.y;
                    DrawBlock("Jump pad", new Vector2(x, y + 0.025f), new Vector2(1f, 0.05f), connectionColor);
                    for (int step = 0; step < 3; step++)
                        DrawBlock("Jump marker", new Vector2(x, y + 0.18f + step * 0.12f),
                            new Vector2(0.34f - step * 0.09f, 0.035f), connectionColor);
                }
    }

    private void DrawBlock(string label, Vector2 position, Vector2 size, Color color)
    {
        var block = new GameObject(label) { hideFlags = HideFlags.HideAndDontSave };
        block.transform.SetParent(visualRoot.transform, false);
        block.transform.position = new Vector3(position.x, position.y, transform.position.z);
        block.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = solidSprite;
        renderer.color = color;
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
            float delta = Mathf.Abs(position.y - GetStandingY(i));
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
        position.y = GetStandingY(floor);
        return position;
    }

    public bool TryGetConnection(int from, int destination, out int nextFloor, out float x)
    {
        nextFloor = from;
        x = 0f;
        if (from < 0 || destination < 0 || from >= FloorCount || destination >= FloorCount || from == destination)
            return false;
        nextFloor = from + (destination > from ? 1 : -1);
        int link = Mathf.Min(from, nextFloor);
        if (connectionX == null || link >= connectionX.Length || floors[from] == null || floors[nextFloor] == null)
            return false;
        float left = Mathf.Max(floors[from].bounds.min.x, floors[nextFloor].bounds.min.x) + 0.6f;
        float right = Mathf.Min(floors[from].bounds.max.x, floors[nextFloor].bounds.max.x) - 0.6f;
        if (left > right) return false;
        x = Mathf.Clamp(connectionX[link], left, right);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < FloorCount - 1; i++)
            if (TryGetConnection(i, i + 1, out int next, out float x))
            {
                Vector3 a = new Vector3(x, GetStandingY(i), transform.position.z);
                Vector3 b = new Vector3(x, GetStandingY(next), transform.position.z);
                Gizmos.DrawLine(a, b);
                Gizmos.DrawWireSphere(a, 0.2f);
                Gizmos.DrawWireSphere(b, 0.2f);
            }
    }
}
