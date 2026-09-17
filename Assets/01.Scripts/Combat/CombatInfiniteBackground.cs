using UnityEngine;

// 카메라 추적이 끝난 뒤, 두 장의 배경을 화면 좌측이 속한 구간에 맞춰 재배치한다.
[DefaultExecutionOrder(100)]
public class CombatInfiniteBackground : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer first;
    [SerializeField] private SpriteRenderer second;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private Sprite[] chapterBackgrounds;

    [Header("배경 생동감")]
    [SerializeField] private Material ambientMaterial;
    [SerializeField, Range(0f, 1f)] private float ambientStrength = 0.7f;
    [SerializeField, Range(0f, 2f)] private float ambientSpeed = 1f;
    [SerializeField, Range(0f, 0.9f)] private float cloudParallax = 0.65f;

    private static readonly int AmbientTimeId = Shader.PropertyToID("_AmbientTime");
    private static readonly int AmbientStrengthId = Shader.PropertyToID("_AmbientStrength");
    private static readonly int ChapterId = Shader.PropertyToID("_Chapter");
    private static readonly int CloudOffsetId = Shader.PropertyToID("_CloudOffset");
    private static readonly int CameraXId = Shader.PropertyToID("_CameraX");
    private static readonly int WorldHeightId = Shader.PropertyToID("_WorldHeight");
    private static readonly int TileDirectionId = Shader.PropertyToID("_TileDirection");
    private MaterialPropertyBlock ambientProperties;
    private Material originalFirstMaterial;
    private Material originalSecondMaterial;
    private float ambientTime;
    private int currentChapter;
    private float cameraOriginX;

    private Vector3 firstScale;
    private Vector3 secondScale;
    private Vector3 authoredFirstScale;
    private Vector3 authoredSecondScale;
    private Vector2 referenceSpriteSize;
    private float originalWidth;
    private float anchorLeft;
    private float previousScale = -1f;
    private int previousSegment = int.MinValue;

    private void OnEnable()
    {
        if (stageManager != null) stageManager.OnStageStarted += ApplyStageBackground;
        if (originalWidth > 0f)
        {
            ApplyAmbientMaterial();
            ApplyStageBackground();
        }
    }

    private void OnDisable()
    {
        if (stageManager != null) stageManager.OnStageStarted -= ApplyStageBackground;
        if (originalWidth > 0f && ambientMaterial != null)
        {
            if (first != null) first.sharedMaterial = originalFirstMaterial;
            if (second != null) second.sharedMaterial = originalSecondMaterial;
        }
    }

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || first == null || second == null || first == second ||
            first.sprite == null || first.sprite != second.sprite)
        {
#if UNITY_EDITOR
            Debug.LogError("[CombatInfiniteBackground] 카메라와 같은 스프라이트를 사용하는 배경 두 장을 연결하세요.", this);
#endif
            enabled = false;
            return;
        }

        firstScale = first.transform.localScale;
        ambientProperties = new MaterialPropertyBlock();
        secondScale = second.transform.localScale;
        authoredFirstScale = firstScale;
        authoredSecondScale = secondScale;
        referenceSpriteSize = first.sprite.bounds.size;
        originalWidth = first.bounds.size.x;
        anchorLeft = first.bounds.min.x;
        cameraOriginX = targetCamera.transform.position.x;
        originalFirstMaterial = first.sharedMaterial;
        originalSecondMaterial = second.sharedMaterial;
        if (originalWidth <= 0f)
        {
            enabled = false;
            return;
        }
        ApplyAmbientMaterial();
        ApplyStageBackground();
    }

    private void ApplyStageBackground()
    {
        if (originalWidth <= 0f) return;
        // 챕터별 5개 스테이지. 전환 페이드의 검은 화면에서 호출된다.
        int chapter = stageManager != null ? (Mathf.Max(1, stageManager.CurrentStageNumber) - 1) / 5 : 0;
        currentChapter = chapter;
        if (chapterBackgrounds != null && chapter < chapterBackgrounds.Length && chapterBackgrounds[chapter] != null)
        {
            Sprite sprite = chapterBackgrounds[chapter];
            Vector2 size = sprite.bounds.size;
            if (size.x > 0f && size.y > 0f)
            {
                first.sprite = second.sprite = sprite;
                // 이미지 해상도가 달라도 배경의 월드 크기와 전투 바닥 높이를 유지한다.
                firstScale = new Vector3(authoredFirstScale.x * referenceSpriteSize.x / size.x,
                    authoredFirstScale.y * referenceSpriteSize.y / size.y, authoredFirstScale.z);
                secondScale = new Vector3(authoredSecondScale.x * referenceSpriteSize.x / size.x,
                    authoredSecondScale.y * referenceSpriteSize.y / size.y, authoredSecondScale.z);
            }
        }
        previousScale = -1f;
        previousSegment = int.MinValue;
        UpdateTiles();
        UpdateAmbient();
    }

    private void LateUpdate()
    {
        UpdateTiles();
        // 배속과 일시정지를 따르며, 재도전 때 시간을 초기화하지 않아 움직임이 튀지 않는다.
        ambientTime += Time.deltaTime * ambientSpeed;
        UpdateAmbient();
    }

    private void ApplyAmbientMaterial()
    {
        if (ambientMaterial == null) return;
        first.sharedMaterial = second.sharedMaterial = ambientMaterial;
    }

    private void UpdateAmbient()
    {
        if (ambientMaterial == null || originalWidth <= 0f || targetCamera == null) return;
        float cameraX = targetCamera.transform.position.x;
        float width = Mathf.Max(0.01f, first.bounds.size.x);
        float cloudOffset = -(cameraX - cameraOriginX) * cloudParallax / width;
        UpdateAmbientTile(first, cameraX, cloudOffset);
        UpdateAmbientTile(second, cameraX, cloudOffset);
    }

    private void UpdateAmbientTile(SpriteRenderer tile, float cameraX, float cloudOffset)
    {
        // 공유 머티리얼을 복제하지 않고 두 배경의 반전 방향만 개별 전달한다.
        tile.GetPropertyBlock(ambientProperties);
        ambientProperties.SetFloat(AmbientTimeId, ambientTime);
        ambientProperties.SetFloat(AmbientStrengthId, ambientStrength);
        ambientProperties.SetFloat(ChapterId, currentChapter);
        ambientProperties.SetFloat(CloudOffsetId, cloudOffset);
        ambientProperties.SetFloat(CameraXId, cameraX);
        ambientProperties.SetFloat(WorldHeightId, tile.bounds.size.y);
        ambientProperties.SetFloat(TileDirectionId, tile.flipX ? -1f : 1f);
        tile.SetPropertyBlock(ambientProperties);
    }

    private void UpdateTiles()
    {
        if (originalWidth <= 0f || targetCamera == null) return;
        float depth = Vector3.Dot(first.transform.position - targetCamera.transform.position,
            targetCamera.transform.forward);
        float left = targetCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth)).x;
        float right = targetCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth)).x;
        float viewLeft = Mathf.Min(left, right);

        // 두 장으로 언제나 화면을 덮도록, 초광폭 화면에서는 가로 폭만 보정한다.
        float scale = Mathf.Max(1f, (Mathf.Abs(right - left) + 0.1f) / originalWidth);
        float width = originalWidth * scale;
        int segment = Mathf.FloorToInt((viewLeft - anchorLeft) / width);
        if (segment == previousSegment && Mathf.Approximately(scale, previousScale)) return;

        first.transform.localScale = new Vector3(firstScale.x * scale, firstScale.y, firstScale.z);
        second.transform.localScale = new Vector3(secondScale.x * scale, secondScale.y, secondScale.z);

        // 각 장의 반전 방향을 유지한다. 음수 이동과 여러 구간의 순간이동도 한 번에 처리한다.
        bool firstOnLeft = segment % 2 == 0;
        PlaceLeftEdge(first, anchorLeft + (firstOnLeft ? segment : segment + 1) * width);
        PlaceLeftEdge(second, anchorLeft + (firstOnLeft ? segment + 1 : segment) * width);
        previousSegment = segment;
        previousScale = scale;
    }

    private static void PlaceLeftEdge(SpriteRenderer tile, float left)
    {
        Vector3 position = tile.transform.position;
        position.x += left - tile.bounds.min.x;
        tile.transform.position = position;
    }
}
