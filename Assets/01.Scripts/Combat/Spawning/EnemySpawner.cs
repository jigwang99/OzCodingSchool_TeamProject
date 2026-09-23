using System;
using System.Collections.Generic;
using UnityEngine;

// 스폰 예약을 보관하고 플레이어가 가까워질 때 풀에서 꺼낸다.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField, Min(1f)] private float activationDistance = 12f;
    [SerializeField, Min(0.02f)] private float activationCheckInterval = 0.1f;
    [SerializeField] private Camera activationCamera;
    [SerializeField, Min(0f)] private float offscreenPadding = 3f;
    [Header("매 전투 무작위 배치 (월드 X 범위)")]
    [SerializeField] private bool randomizeSpawns = true;
    [SerializeField] private bool useFullMapWidth;
    [SerializeField] private Vector2 spawnXRange = new Vector2(0f, 20f);
    [SerializeField, Min(0.6f)] private float spawnEdgePadding = 0.8f;
    [SerializeField, Min(0f)] private float minimumSpawnSpacing = 1.25f;
    [SerializeField, Min(0f)] private float playerSpawnClearance = 3f;
    private CombatCameraFollow cameraFollow;
    private readonly List<EnemyController> active = new();
    private readonly List<int> pending = new();
    private StageData stage;
    private Vector3 origin;
    private PlayercatController player;
    private bool running;
    private float nextCheck;
    private CombatFloorMap floorMap;
    private Vector3[] spawnPositions = Array.Empty<Vector3>();

    public IReadOnlyList<EnemyController> Spawned => active;
    public bool HasPendingEnemies => pending.Count > 0;
    public event Action<EnemyController> OnEnemyActivated;

    private void Awake()
    {
        if (activationCamera == null) activationCamera = Camera.main;
        if (activationCamera != null) cameraFollow = activationCamera.GetComponent<CombatCameraFollow>();
    }

    public void PrepareStage(StageData data, Vector3 spawnOrigin, PlayercatController target, CombatFloorMap map = null)
    {
        stage = data;
        origin = spawnOrigin;
        player = target;
        floorMap = map;
        player.ConfigureNavigation(this, floorMap);
        BuildSpawnPositions();
        // 플레이어가 시작점으로 이동한 직후에는 카메라도 먼저 맞춘다.
        // 이전 스테이지 끝의 카메라 위치로 활성화 범위를 계산하지 않는다.
        if (cameraFollow != null) cameraFollow.SnapToTarget();
        pending.Clear();
        for (int i = 0; i < data.EnemyCount; i++) pending.Add(i);
        ActivateNearby();
    }

    // 예약 탐색과 실제 스폰은 한 번 뽑은 좌표를 공유한다. 원본 데이터는 수정하지 않는다.
    private void BuildSpawnPositions()
    {
        spawnPositions = new Vector3[stage.EnemyCount];
        if (!randomizeSpawns || floorMap == null || floorMap.FloorCount == 0)
        {
            for (int i = 0; i < spawnPositions.Length; i++)
                spawnPositions[i] = origin + (Vector3)stage.SpawnOffsets[i];
            return;
        }
        var ranges = new Vector2[floorMap.FloorCount];
        var excludedRanges = new Vector2[floorMap.FloorCount];
        var availableFloors = new List<int>();
        float requestedLeft = Mathf.Min(spawnXRange.x, spawnXRange.y);
        float requestedRight = Mathf.Max(spawnXRange.x, spawnXRange.y);
        if (useFullMapWidth)
        {
            Vector2 mapRange = floorMap.GetWalkableRange(0, spawnEdgePadding);
            requestedLeft = mapRange.x;
            requestedRight = mapRange.y;
        }
        for (int floor = 0; floor < floorMap.FloorCount; floor++)
        {
            Vector2 range = floorMap.GetWalkableRange(floor, spawnEdgePadding);
            range.x = Mathf.Max(range.x, requestedLeft);
            range.y = Mathf.Min(range.y, requestedRight);
            if (range.x > range.y) continue;
            if (player != null && floor == player.CurrentFloor)
            {
                float leftEnd = Mathf.Clamp(player.transform.position.x - playerSpawnClearance, range.x, range.y);
                float rightStart = Mathf.Clamp(player.transform.position.x + playerSpawnClearance, range.x, range.y);
                if (rightStart - leftEnd >= range.y - range.x &&
                    Mathf.Abs(range.x - player.transform.position.x) < playerSpawnClearance &&
                    Mathf.Abs(range.y - player.transform.position.x) < playerSpawnClearance) continue;
                excludedRanges[floor] = new Vector2(leftEnd, rightStart);
            }
            ranges[floor] = range;
            availableFloors.Add(floor);
        }
        if (availableFloors.Count == 0)
            throw new InvalidOperationException("적 소환 범위가 발판 또는 플레이어 시작 안전거리와 겹치지 않습니다.");

        int firstFloor = UnityEngine.Random.Range(0, availableFloors.Count);
        int cursor = 0;
        for (int order = 0; order < availableFloors.Count; order++)
        {
            int count = stage.EnemyCount / availableFloors.Count + (order < stage.EnemyCount % availableFloors.Count ? 1 : 0);
            if (count == 0) continue;
            int floor = availableFloors[(firstFloor + order) % availableFloors.Count];
            Vector2 range = ranges[floor];
            Vector2 excluded = excludedRanges[floor];
            float excludedWidth = excluded.y - excluded.x;
            float availableWidth = range.y - range.x - excludedWidth;
            float spacing = count > 1 ? Mathf.Min(minimumSpawnSpacing, availableWidth / (count - 1)) : 0f;
            float freeWidth = Mathf.Max(0f, availableWidth - spacing * (count - 1));
            var samples = new float[count];
            for (int i = 0; i < count; i++) samples[i] = UnityEngine.Random.value;
            Array.Sort(samples);
            for (int i = 0; i < count; i++)
            {
                // 안전 구간을 제외한 길이에서 뽑은 뒤 간격을 복원해 시작점 양쪽을 모두 사용한다.
                float x = range.x + samples[i] * freeWidth + i * spacing;
                if (excludedWidth > 0f && x >= excluded.x) x += excludedWidth;
                spawnPositions[cursor++] = new Vector3(x, floorMap.GetStandingY(floor), origin.z);
            }
        }
        // 외형과 보스의 위치도 매번 달라지도록 배정을 섞는다.
        for (int i = spawnPositions.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (spawnPositions[i], spawnPositions[j]) = (spawnPositions[j], spawnPositions[i]);
        }
    }

    private Vector3 GetSpawnPosition(int index) => spawnPositions.Length == stage.EnemyCount
        ? spawnPositions[index] : origin + (Vector3)stage.SpawnOffsets[index];

    public void SetCombatRunning(bool value)
    {
        running = value;
        nextCheck = 0f;
        foreach (EnemyController enemy in active)
            if (enemy != null && enemy.gameObject.activeSelf && !enemy.Health.IsDead)
            {
                if (!value) enemy.PrepareForPool();
                else enemy.SetTarget(player);
                enemy.enabled = value;
            }
    }

    private void Update()
    {
        if (!running || player == null || player.Health.IsDead || !HasPendingEnemies || Time.time < nextCheck)
            return;
        nextCheck = Time.time + activationCheckInterval;
        ActivateNearby();
    }

    private void ActivateNearby()
    {
        float distance = GetActivationDistance();
        for (int i = pending.Count - 1; i >= 0; i--)
        {
            int index = pending[i];
            Vector3 position = GetSpawnPosition(index);
            if (Mathf.Abs(position.x - player.transform.position.x) > distance) continue;
            EnemyController enemy = CombatObjectPoolManager.instance.GetObject<EnemyController>(stage.GetEnemyType(index), position);
            if (enemy == null) throw new InvalidOperationException("준비된 몬스터 풀을 찾을 수 없습니다.");
            enemy.Health.SetMaxHp(stage.EnemyMaxHp);
            enemy.Attack.SetAttackDamage(stage.EnemyDamage);
            enemy.SetFloorMap(floorMap);
            enemy.SetTarget(player);
            enemy.Move.IgnoreUnitCollisions(player.Move);
            foreach (EnemyController other in active)
                if (other != null && other.gameObject.activeInHierarchy)
                    enemy.Move.IgnoreUnitCollisions(other.Move);
            enemy.enabled = running;
            active.Add(enemy);
            pending.RemoveAt(i);
            player.HasPendingEnemies = HasPendingEnemies;
            OnEnemyActivated?.Invoke(enemy);
        }
        player.HasPendingEnemies = HasPendingEnemies;
    }

    private float GetActivationDistance()
    {
        // 검사 간격과 카메라 LateUpdate 사이에 전진할 거리까지 미리 확보한다.
        float movementBuffer = player.Move.MoveSpeed * (activationCheckInterval + Time.deltaTime);
        float distance = Mathf.Max(activationDistance, player.Attack.AttackRange + movementBuffer + 1f);
        if (activationCamera == null) return distance;

        float depth = Vector3.Dot(player.transform.position - activationCamera.transform.position,
            activationCamera.transform.forward);
        float left = activationCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth)).x;
        float right = activationCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth)).x;
        float visibleDistance = Mathf.Max(Mathf.Abs(left - player.transform.position.x),
            Mathf.Abs(right - player.transform.position.x));
        return Mathf.Max(distance, visibleDistance + offscreenPadding + movementBuffer);
    }

    public void Clear()
    {
        running = false;
        pending.Clear();
        spawnPositions = Array.Empty<Vector3>();
        if (player != null) player.HasPendingEnemies = false;
        foreach (EnemyController enemy in active)
            if (enemy != null) CombatObjectPoolManager.instance.ReturnObject(enemy.PoolKey, enemy.gameObject);
        active.Clear();
        player = null;
        stage = null;
    }

    public EnemyController GetNearestAlive(Vector3 from)
    {
        EnemyController nearest = null;
        float bestSqr = float.MaxValue;
        foreach (EnemyController enemy in active)
        {
            if (enemy == null || !enemy.isActiveAndEnabled || enemy.Health.IsDead) continue;
            float sqr = ((Vector2)(enemy.transform.position - from)).sqrMagnitude;
            if (sqr >= bestSqr) continue;
            bestSqr = sqr;
            nearest = enemy;
        }
        return nearest;
    }

    public bool TryGetPendingDestination(Vector3 from, out Vector3 destination)
    {
        destination = from;
        bool found = false;
        float bestSqr = float.MaxValue;
        foreach (int index in pending)
        {
            Vector3 position = GetSpawnPosition(index);
            float sqr = ((Vector2)(position - from)).sqrMagnitude;
            if (sqr >= bestSqr) continue;
            found = true;
            bestSqr = sqr;
            destination = position;
        }
        return found;
    }
}
