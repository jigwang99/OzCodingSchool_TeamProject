#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class CombatFloorTests
{
    private Scene scene;
    private CombatFloorMap map;
    private PlayercatController player;
    private EnemyController enemy;

    [SetUp]
    public void SetUp()
    {
        scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        map = CreateObject("Map", Vector3.zero).AddComponent<CombatFloorMap>();
        var floors = new BoxCollider2D[3];
        for (int i = 0; i < floors.Length; i++)
        {
            floors[i] = CreateObject("Floor", new Vector3(9f, -3.68f + i * 2.6f)).AddComponent<BoxCollider2D>();
            floors[i].size = new Vector2(30f, 0.36f);
        }
        SetField(map, "floors", floors);
        SetField(map, "connectionX", new[] { 20f, -2f });
        Physics2D.SyncTransforms();
        player = CreateUnit<PlayercatController>(new Vector3(0f, -3f));
        enemy = CreateUnit<EnemyController>(new Vector3(3f, -3f));
        enemy.Init();
        player.SetTarget(enemy);
        enemy.SetTarget(player);
    }

    [TearDown]
    public void TearDown()
    {
        if (!scene.IsValid()) return;
        foreach (GameObject root in scene.GetRootGameObjects()) Object.DestroyImmediate(root);
        UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
    }

    private GameObject CreateObject(string name, Vector3 position)
    {
        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        go.transform.position = position;
        return go;
    }

    private T CreateUnit<T>(Vector3 position) where T : BaseUnitController
    {
        T unit = CreateObject(typeof(T).Name, position).AddComponent<T>();
        // EditMode에서는 일반 MonoBehaviour의 Awake가 실행되지 않는다.
        if (unit.Health == null)
        {
            Invoke(unit.GetComponent<UnitHealth>(), "Awake");
            Invoke(unit.GetComponent<UnitMove>(), "Awake");
            Invoke(unit.GetComponent<UnitAttack>(), "Awake");
            Invoke(unit, "Awake");
        }
        unit.GetComponent<Rigidbody2D>().gravityScale = 0f;
        unit.SetFloorMap(map);
        unit.Revive();
        return unit;
    }

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    private static T GetField<T>(object target, string name) =>
        (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

    private static void Invoke(object target, string name) =>
        target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);

    private static void Place(BaseUnitController unit, Vector3 position)
    {
        unit.GetComponent<Rigidbody2D>().position = position;
        unit.transform.position = position;
        Physics2D.SyncTransforms();
    }

    [Test]
    public void FloorRoutesUseAdjacentFloorsInBothDirections()
    {
        Assert.That(map.GetFloorIndex(new Vector3(0, -3)), Is.EqualTo(0));
        Assert.That(map.GetFloorIndex(new Vector3(0, -0.4f)), Is.EqualTo(1));
        Assert.That(map.GetFloorIndex(new Vector3(0, 2.2f)), Is.EqualTo(2));
        Assert.That(map.GetFloorIndex(new Vector3(0, 0.9f)), Is.EqualTo(-1));
        Assert.That(map.TryGetConnection(0, 2, out int next, out float x), Is.True);
        Assert.That(next, Is.EqualTo(1));
        Assert.That(x, Is.EqualTo(20));
        Assert.That(map.TryGetConnection(2, 0, out next, out x), Is.True);
        Assert.That(next, Is.EqualTo(1));
        Assert.That(x, Is.EqualTo(-2));
    }

    [Test]
    public void ChangingFloorInvalidatesPreparedHit()
    {
        Place(enemy, new Vector3(1, -3));
        int attackId = player.Attack.BeginAttack(enemy, 0);
        Assert.That(attackId, Is.Not.Zero);
        Place(enemy, new Vector3(1, -0.4f));
        Assert.That(player.IsTargetInAttackRange, Is.False);
        Assert.That(enemy.IsTargetDetected, Is.False);
        Assert.That(player.Attack.TryResolveHit(attackId), Is.False);
        Assert.That(enemy.Health.CurrentHp, Is.EqualTo(enemy.Health.MaxHp));
    }

    [Test]
    public void EnemyKeepsChasingOnSameFloorThenReturnsHome()
    {
        Invoke(enemy, "Update");
        Place(player, new Vector3(-5, -3));
        Assert.That(enemy.IsTargetDetected, Is.True, "Acquired target may leave detection radius on the same floor.");
        Place(enemy, new Vector3(1, -3));
        Place(player, new Vector3(1, -0.4f));
        Invoke(enemy, "Update");
        Assert.That(enemy.IsReturningHome, Is.True);
        Assert.That(enemy.IsTargetInAttackRange, Is.False);
        for (int i = 0; i < 100 && enemy.IsReturningHome; i++)
        {
            enemy.StateMachine.FixedUpdate();
            scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
        }
        Assert.That(enemy.IsReturningHome, Is.False);
        Assert.That(enemy.transform.position.x, Is.EqualTo(3).Within(0.04f));
        Assert.That(enemy.StateMachine.CurrentState, Is.SameAs(enemy.IdleState));
    }

    [Test]
    public void PoolReuseReplacesHomeAndChaseState()
    {
        Invoke(enemy, "Update");
        Place(player, new Vector3(0, -0.4f));
        Invoke(enemy, "Update");
        Assert.That(enemy.IsReturningHome, Is.True);
        enemy.PrepareForPool();
        Place(enemy, new Vector3(12, 2.2f));
        enemy.Init();
        Assert.That(enemy.IsReturningHome, Is.False);
        Assert.That(enemy.HomePosition, Is.EqualTo(new Vector3(12, 2.2f)));
        Assert.That(enemy.HasTarget, Is.False);
    }

    [Test]
    public void PlayerCompletesJumpAndDropWithoutCrossFloorCombat()
    {
        Place(player, new Vector3(20, -3));
        Place(enemy, new Vector3(20, -0.4f));
        player.PerformMove();
        Assert.That(player.IsChangingFloors, Is.True);
        Assert.That(enemy.IsTargetDetected, Is.False);
        Assert.That(player.IsTargetInAttackRange, Is.False);
        for (int i = 0; i < 40 && player.IsChangingFloors; i++) Invoke(player, "FixedUpdate");
        Assert.That(player.CurrentFloor, Is.EqualTo(1));
        Assert.That(player.GetComponent<Rigidbody2D>().simulated, Is.True);
        Place(enemy, new Vector3(20, -3));
        player.PerformMove();
        for (int i = 0; i < 40 && player.IsChangingFloors; i++) Invoke(player, "FixedUpdate");
        Assert.That(player.CurrentFloor, Is.EqualTo(0));
        Assert.That(player.IsChangingFloors, Is.False);
    }

    [Test]
    public void DisablingMidJumpRestoresPhysicsAndStandingPosition()
    {
        Place(player, new Vector3(20, -3));
        Place(enemy, new Vector3(20, -0.4f));
        player.PerformMove();
        Invoke(player, "FixedUpdate");
        Invoke(player, "OnDisable");
        Assert.That(player.IsChangingFloors, Is.False);
        Assert.That(player.GetComponent<Rigidbody2D>().simulated, Is.True);
        Assert.That(player.transform.position.y, Is.EqualTo(-3).Within(0.01f));
    }

    [Test]
    public void DetectionPrefersSameFloorAndFindsOffscreenReservations()
    {
        var spawner = CreateObject("Spawner", Vector3.zero).AddComponent<EnemySpawner>();
        var upper = CreateUnit<EnemyController>(new Vector3(0, -0.4f));
        var active = GetField<List<EnemyController>>(spawner, "active");
        active.Add(upper);
        active.Add(enemy);
        Assert.That(spawner.GetNearestAlive(player.transform.position, 0), Is.SameAs(enemy));
        SetField(spawner, "stage", new StageData("Test", new[] { new Vector2(0, -0.4f), new Vector2(20, -3) }, 10, 1, null));
        SetField(spawner, "floorMap", map);
        GetField<List<int>>(spawner, "pending").AddRange(new[] { 0, 1 });
        Assert.That(spawner.TryGetPendingDestination(player.transform.position, 0, out Vector3 point), Is.True);
        Assert.That(point, Is.EqualTo(new Vector3(20, -3)));
        active.Remove(enemy);
        player.ConfigureNavigation(spawner, map);
        Invoke(player, "Update");
        Assert.That(player.HasTarget, Is.False, "A pending same-floor spawn takes priority over an active upper-floor enemy.");
        Assert.That(player.IsTargetDetected, Is.True);
        player.PerformMove();
        Assert.That(player.GetComponent<Rigidbody2D>().linearVelocity.x, Is.GreaterThan(0));
    }

    [Test]
    public void ReturnPathIsNotBlockedByUnitsButStillCollidesWithPlatforms()
    {
        Place(enemy, new Vector3(5, -3));
        enemy.Init();
        enemy.SetTarget(player);
        Place(player, new Vector3(2, -3));
        Invoke(enemy, "Update");
        Place(enemy, new Vector3(1, -3));
        Place(player, new Vector3(2, -0.4f));
        Invoke(enemy, "Update");
        Assert.That(enemy.IsReturningHome, Is.True);

        Place(player, new Vector3(3, -3));
        var blocker = CreateUnit<EnemyController>(new Vector3(2, -3));
        enemy.gameObject.AddComponent<BoxCollider2D>();
        player.gameObject.AddComponent<BoxCollider2D>();
        blocker.gameObject.AddComponent<BoxCollider2D>();
        player.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        blocker.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        enemy.Move.IgnoreUnitCollisions(player.Move);
        enemy.Move.IgnoreUnitCollisions(blocker.Move);
        Physics2D.SyncTransforms();
        for (int i = 0; i < 160 && enemy.IsReturningHome; i++)
        {
            Invoke(enemy, "Update");
            enemy.StateMachine.FixedUpdate();
            scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
        }
        Assert.That(enemy.IsReturningHome, Is.False);
        Assert.That(enemy.transform.position.x, Is.EqualTo(5).Within(0.04f));
        BoxCollider2D floor = GetField<BoxCollider2D[]>(map, "floors")[0];
        Assert.That(Physics2D.GetIgnoreCollision(enemy.GetComponent<Collider2D>(), floor), Is.False);
    }

    [Test]
    public void HorizontalMovementPreservesGravityAndStopsAtDestination()
    {
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        body.linearVelocity = new Vector2(0, -2);
        player.Move.MoveToX(0.01f);
        Assert.That(body.linearVelocity.y, Is.EqualTo(-2));
        Assert.That(body.linearVelocity.x * Time.fixedDeltaTime, Is.EqualTo(0.01f).Within(0.0001f));
    }

    // 별도 배치 검증 프로젝트에서도 같은 테스트를 실행할 수 있는 진입점.
    public static void RunBatch()
    {
        var results = new List<string>();
        int failed = 0;
        foreach (MethodInfo method in typeof(CombatFloorTests).GetMethods())
        {
            if (!Attribute.IsDefined(method, typeof(TestAttribute))) continue;
            var fixture = new CombatFloorTests();
            try
            {
                fixture.SetUp();
                method.Invoke(fixture, null);
                results.Add("PASS " + method.Name);
            }
            catch (Exception error)
            {
                failed++;
                results.Add("FAIL " + method.Name + ": " + (error.InnerException ?? error));
            }
            finally { fixture.TearDown(); }
        }
        System.IO.File.WriteAllLines("combat-floor-results.txt", results);
        foreach (string result in results) Debug.Log(result);
        EditorApplication.Exit(failed == 0 ? 0 : 1);
    }
}
#endif
