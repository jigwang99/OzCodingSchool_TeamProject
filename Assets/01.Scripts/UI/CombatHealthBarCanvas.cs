using System.Collections.Generic;
using UnityEngine;

// 카메라 추적 이후 위치만 일괄 갱신한다. 체력 값은 HealthBarUI가 이벤트로 갱신한다.
[DefaultExecutionOrder(200)]
[RequireComponent(typeof(Canvas))]
public class CombatHealthBarCanvas : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayercatController player;
    [SerializeField] private HealthBarUI barPrefab;
    [SerializeField, Min(0)] private int initialPoolSize = 8;
    [SerializeField] private Vector3 playerOffset = new Vector3(0f, 3f, 0f);
    [Tooltip("적 외형 상단에서 체력바까지의 여백. 생성 시 대기 자세를 기준으로 높이를 고정합니다.")]
    [SerializeField] private Vector3 enemyOffset = new Vector3(0f, 0.15f, 0f);

    private sealed class Entry
    {
        public UnitHealth Health;
        public Transform Target;
        public Vector3 Offset;
        public HealthBarUI Bar;
    }

    private readonly List<Entry> entries = new();
    private readonly Stack<HealthBarUI> available = new();
    private readonly Stack<Entry> spareEntries = new();
    private readonly List<SpriteRenderer> spriteBuffer = new List<SpriteRenderer>(16);
    private bool initialized;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || stageManager == null || enemySpawner == null || player == null || barPrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogError("[CombatHealthBarCanvas] 카메라, 전투 참조와 체력바 프리팹을 연결하세요.", this);
#endif
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (stageManager != null) stageManager.OnStageStarted += HandleStageStarted;
        if (enemySpawner != null) enemySpawner.OnEnemyActivated += RegisterEnemy;
        if (initialized) HandleStageStarted();
    }

    private void Start()
    {
        for (int i = 0; i < initialPoolSize; i++) available.Push(CreateBar());
        initialized = true;
        HandleStageStarted();
    }

    private void OnDisable()
    {
        if (stageManager != null) stageManager.OnStageStarted -= HandleStageStarted;
        if (enemySpawner != null) enemySpawner.OnEnemyActivated -= RegisterEnemy;
        for (int i = entries.Count - 1; i >= 0; i--) RemoveEntry(i);
    }

    private HealthBarUI CreateBar()
    {
        HealthBarUI bar = Instantiate(barPrefab, transform);
        bar.gameObject.SetActive(false);
        return bar;
    }

    private void HandleStageStarted()
    {
        if (!initialized) return;
        for (int i = entries.Count - 1; i >= 0; i--) RemoveEntry(i);
        Register(player, playerOffset);
        foreach (EnemyController enemy in enemySpawner.Spawned) RegisterEnemy(enemy);
    }

    private void RegisterEnemy(EnemyController enemy)
    {
        if (!initialized || enemy == null) return;
        Vector3 offset = enemyOffset;
        float top = float.NegativeInfinity;
        enemy.GetComponentsInChildren<SpriteRenderer>(spriteBuffer);
        foreach (SpriteRenderer sprite in spriteBuffer)
        {
            if (!sprite.enabled || sprite.sprite == null) continue;
            top = Mathf.Max(top, sprite.bounds.max.y);
        }
        // 임시 외형처럼 스프라이트가 없으면 콜라이더 상단을 기준으로 한다.
        if (float.IsNegativeInfinity(top))
        {
            Collider2D body = enemy.GetComponent<Collider2D>();
            top = body != null ? body.bounds.max.y : enemy.transform.position.y;
        }
        Vector3 anchor = enemy.transform.position;
        anchor.y = top;
        offset.y += enemy.transform.InverseTransformPoint(anchor).y;
        // 등록 시 한 번 계산하여 공격/걷기 애니메이션에 따라 체력바가 출렁이지 않게 한다.
        Register(enemy, offset);
    }

    private void Register(BaseUnitController unit, Vector3 offset)
    {
        // 전환 페이드 동안 controller.enabled가 꺼져 있어도 배치는 유지한다.
        if (unit == null || !unit.gameObject.activeInHierarchy || unit.Health == null || unit.Health.IsDead) return;
        foreach (Entry entry in entries)
            if (entry.Health == unit.Health) return;
        Entry item = spareEntries.Count > 0 ? spareEntries.Pop() : new Entry();
        item.Health = unit.Health;
        item.Target = unit.transform;
        item.Offset = offset;
        entries.Add(item);
    }

    private void LateUpdate()
    {
        if (!initialized || targetCamera == null) return;
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            Entry entry = entries[i];
            if (entry.Health == null || entry.Target == null || !entry.Health.gameObject.activeInHierarchy)
            {
                RemoveEntry(i);
                continue;
            }
            if (entry.Health.IsDead)
            {
                // 살아 있는 오브젝트의 등록은 유지해 직접 Revive()해도 다시 표시한다.
                ReleaseBar(entry);
                continue;
            }

            Vector3 position = entry.Target.TransformPoint(entry.Offset);
            Vector3 viewport = targetCamera.WorldToViewportPoint(position);
            bool visible = targetCamera.isActiveAndEnabled && viewport.z > 0f &&
                viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
            if (!visible)
            {
                ReleaseBar(entry);
                continue;
            }
            if (entry.Bar == null)
            {
                entry.Bar = available.Count > 0 ? available.Pop() : CreateBar();
                // 재활성화 전에 대상을 연결하고 초기 위치와 HP를 채운다.
                entry.Bar.Bind(entry.Health);
                entry.Bar.RectTransform.position = position;
                entry.Bar.gameObject.SetActive(true);
            }
            else if (entry.Bar.RectTransform.position != position)
            {
                entry.Bar.RectTransform.position = position;
            }
        }
    }

    private void ReleaseBar(Entry entry)
    {
        if (entry.Bar == null) return;
        entry.Bar.Unbind();
        entry.Bar.gameObject.SetActive(false);
        available.Push(entry.Bar);
        entry.Bar = null;
    }

    private void RemoveEntry(int index)
    {
        Entry entry = entries[index];
        ReleaseBar(entry);
        entry.Health = null;
        entry.Target = null;
        entries.RemoveAt(index);
        spareEntries.Push(entry);
    }
}
