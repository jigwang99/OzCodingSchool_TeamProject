using UnityEngine;
using UnityEngine.UI;

// 여섯 스파크를 하나의 Graphic으로 그려 개별 Image/Transform 갱신을 없앤다.
public sealed class CombatSparkGraphic : MaskableGraphic
{
    private const int SparkCount = 6;
    private static readonly Vector2[] Directions = CreateDirections();
    private float progress;

    public void SetProgress(float value)
    {
        value = Mathf.Clamp01(value);
        if (progress == value) return;
        progress = value;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        float remaining = 1f - progress;
        float distance = Mathf.Lerp(5f, 32f, progress);
        float halfWidth = 2.5f * remaining;
        float length = 25f * remaining;
        Color tint = color;
        tint.a *= remaining;
        Color32 vertexColor = tint;

        for (int i = 0; i < SparkCount; i++)
        {
            Vector2 direction = Directions[i];
            Vector2 side = new Vector2(direction.y, -direction.x) * halfWidth;
            Vector2 bottom = direction * distance;
            Vector2 top = bottom + direction * length;
            int start = mesh.currentVertCount;
            mesh.AddVert(bottom - side, vertexColor, Vector2.zero);
            mesh.AddVert(top - side, vertexColor, Vector2.up);
            mesh.AddVert(top + side, vertexColor, Vector2.one);
            mesh.AddVert(bottom + side, vertexColor, Vector2.right);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start + 2, start + 3, start);
        }
    }

    private static Vector2[] CreateDirections()
    {
        var directions = new Vector2[SparkCount];
        for (int i = 0; i < SparkCount; i++)
        {
            float radians = (i * 60f + 15f) * Mathf.Deg2Rad;
            directions[i] = new Vector2(-Mathf.Sin(radians), Mathf.Cos(radians));
        }
        return directions;
    }
}
