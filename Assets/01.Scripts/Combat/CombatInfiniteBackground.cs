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
        if (originalWidth > 0f) ApplyStageBackground();
    }

    private void OnDisable()
    {
        if (stageManager != null) stageManager.OnStageStarted -= ApplyStageBackground;
    }

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || first == null || second == null || first == second ||
            first.sprite == null || first.sprite != second.sprite)
        {
            Debug.LogError("[CombatInfiniteBackground] 카메라와 같은 스프라이트를 사용하는 배경 두 장을 연결하세요.", this);
            enabled = false;
            return;
        }

        firstScale = first.transform.localScale;
        secondScale = second.transform.localScale;
        authoredFirstScale = firstScale;
        authoredSecondScale = secondScale;
        referenceSpriteSize = first.sprite.bounds.size;
        originalWidth = first.bounds.size.x;
        anchorLeft = first.bounds.min.x;
        if (originalWidth <= 0f)
        {
            enabled = false;
            return;
        }
        ApplyStageBackground();
    }

    private void ApplyStageBackground()
    {
        if (originalWidth <= 0f) return;
        // 챕터별 5개 스테이지. 전환 페이드의 검은 화면에서 호출된다.
        int chapter = stageManager != null ? (Mathf.Max(1, stageManager.CurrentStageNumber) - 1) / 5 : 0;
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
    }

    private void LateUpdate() => UpdateTiles();

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
