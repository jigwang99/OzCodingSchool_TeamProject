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
    private CombatCameraFollow cameraFollow;
    private readonly List<EnemyController> active = new();
    private readonly List<int> pending = new();
    private StageData stage;
    private Vector3 origin;
    private PlayercatController player;
    private bool running;
    private float nextCheck;
    private CombatFloorMap floorMap;

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
        // 플레이어가 시작점으로 이동한 직후에는 카메라도 먼저 맞춘다.
        // 이전 스테이지 끝의 카메라 위치로 활성화 범위를 계산하지 않는다.
        if (cameraFollow != null) cameraFollow.SnapToTarget();
        pending.Clear();
        for (int i = 0; i < data.EnemyCount; i++) pending.Add(i);
        ActivateNearby();
    }

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
            Vector3 position = origin + (Vector3)stage.SpawnOffsets[index];
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
        if (player != null) player.HasPendingEnemies = false;
        foreach (EnemyController enemy in active)
            if (enemy != null) CombatObjectPoolManager.instance.ReturnObject(enemy.PoolKey, enemy.gameObject);
        active.Clear();
        player = null;
        stage = null;
    }

    public EnemyController GetNearestAlive(Vector3 from, int preferredFloor = -1)
    {
        EnemyController nearest = null;
        float bestSqr = float.MaxValue;
        bool bestOnFloor = false;
        foreach (EnemyController enemy in active)
        {
            if (enemy == null || !enemy.gameObject.activeSelf || enemy.Health.IsDead) continue;
            bool onFloor = preferredFloor >= 0 && enemy.CurrentFloor == preferredFloor;
            float sqr = ((Vector2)(enemy.transform.position - from)).sqrMagnitude;
            if (nearest != null && (bestOnFloor && !onFloor || bestOnFloor == onFloor && sqr >= bestSqr)) continue;
            bestSqr = sqr;
            bestOnFloor = onFloor;
            nearest = enemy;
        }
        return nearest;
    }

    public bool TryGetPendingDestination(Vector3 from, int preferredFloor, out Vector3 destination)
    {
        destination = from;
        bool found = false;
        bool bestOnFloor = false;
        float bestSqr = float.MaxValue;
        foreach (int index in pending)
        {
            Vector3 position = origin + (Vector3)stage.SpawnOffsets[index];
            bool onFloor = floorMap == null || floorMap.GetFloorIndex(position) == preferredFloor;
            float sqr = ((Vector2)(position - from)).sqrMagnitude;
            if (found && (bestOnFloor && !onFloor || bestOnFloor == onFloor && sqr >= bestSqr)) continue;
            found = true;
            bestOnFloor = onFloor;
            bestSqr = sqr;
            destination = position;
        }
        return found;
    }
}
