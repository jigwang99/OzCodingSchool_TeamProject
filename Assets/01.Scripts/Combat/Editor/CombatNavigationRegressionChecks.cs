using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// 별도의 물리 씬에서 검사하므로 열려 있는 게임 씬이나 저장 데이터는 변경하지 않는다.
public static class CombatNavigationRegressionChecks
{
    private const string ResultPath = "Temp/CodexValidation/navigation-result.txt";
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [MenuItem("Tools/Combat/Check Vertical Navigation")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("전투 이동 검사는 플레이 모드 밖에서 실행하세요.");
            return;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(ResultPath));
        string result;
        try
        {
            CheckTravel(false, false);
            CheckTravel(false, true);
            CheckTravel(true, false);
            CheckTravel(false, false, true);
            CheckEnemyStates();
            CheckAttackFacing();
            result = "PASS: FSM upward/downward travel, fixed X, landing, jump interruption, enemy chase/return/patrol, attack direction through recovery.";
        }
        catch (Exception exception)
        {
            result = "FAIL: " + exception;
        }
        File.WriteAllText(ResultPath, result);
        Debug.Log(result);
    }

    private static void CheckTravel(bool descending, bool approachLeft, bool interrupt = false)
    {
        Scene scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        try
        {
            Require(scene.GetPhysicsScene2D() != Physics2D.defaultPhysicsScene, "Preview physics must be isolated.");
            var mapObject = NewObject(scene, "Map");
            var map = mapObject.AddComponent<CombatFloorMap>();
            var ground = NewObject(scene, "Ground").AddComponent<BoxCollider2D>();
            ground.transform.position = new Vector3(15f, -0.5f);
            ground.size = new Vector2(30f, 1f);
            typeof(CombatFloorMap).GetField("floors", PrivateInstance).SetValue(map, new[] { ground });
            Physics2D.SyncTransforms();
            map.ApplyStageLayout(new Vector3Int(1, 0, 2), new Vector2(0f, 30f));

            var playerObject = NewObject(scene, "Player");
            var body = playerObject.AddComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            playerObject.AddComponent<BoxCollider2D>();
            var player = playerObject.AddComponent<PlayercatController>();
            InitializeUnit(player);
            player.SetFloorMap(map);

            var targetObject = NewObject(scene, "Target");
            var enemy = targetObject.AddComponent<EnemyController>();
            InitializeUnit(enemy);
            enemy.SetFloorMap(map);
            int from = descending ? 1 : 0;
            int goal = descending ? 0 : 1;
            // 도약 지점 밖에서 걸어 접근해, 바닥 마찰 때문에 점프를 시작하지 못하는 회귀를 검사한다.
            Vector2 range = map.GetWalkableRange(1);
            float x = descending ? 5f : approachLeft ? range.x - 0.5f : range.y + 0.5f;
            body.position = new Vector2(x, map.GetStandingY(from));
            player.transform.position = body.position;
            enemy.transform.position = new Vector3(5f, map.GetStandingY(goal));
            enemy.GetComponent<Rigidbody2D>().simulated = false;
            player.SetTarget(enemy);
            player.StateMachine.ChangeState(player.MoveState);
            Physics2D.SyncTransforms();

            bool jumped = false;
            float jumpX = 0f;
            for (int step = 0; step < 600; step++)
            {
                player.StateMachine.FixedUpdate();
                if (player.IsChangingFloors)
                {
                    Require(body.simulated, "Jump disabled Rigidbody simulation.");
                    if (!jumped) jumpX = body.position.x;
                    jumped = true;
                    Require(Mathf.Abs(body.position.x - jumpX) < 0.0001f, "Jump moved horizontally.");
                    if (interrupt)
                    {
                        player.StateMachine.ChangeState(player.IdleState);
                        Require(!player.IsChangingFloors && body.simulated, "Interrupted jump did not restore physics.");
                        Require(player.CurrentFloor == from, "Interrupted jump did not restore takeoff position.");
                        for (int floor = 0; floor < map.FloorCount; floor++)
                            Require(!Physics2D.GetIgnoreCollision(player.GetComponent<Collider2D>(), map.GetFloorCollider(floor)),
                                "Interrupted jump did not restore platform collision.");
                        return;
                    }
                }
                else if (jumped)
                {
                    Require(player.CurrentFloor == goal, "Landed on the wrong floor.");
                    Require(body.simulated, "Physics was not restored on landing.");
                    Require(player.GetComponent<Collider2D>().IsTouching(map.GetFloorCollider(goal)),
                        "Landing did not use physical platform contact.");
                    return;
                }
                float verticalSpeed = body.linearVelocity.y;
                scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                if (player.IsChangingFloors && !player.GetComponent<Collider2D>().IsTouching(map.GetFloorCollider(goal)))
                    Require(body.linearVelocity.y < verticalSpeed, "Gravity did not advance jump velocity.");
            }
            throw new Exception($"No landing: descending={descending}, position={body.position:F6}, floor={player.CurrentFloor}, targetFloor={goal}, changing={player.IsChangingFloors}");
        }
        finally
        {
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
        }
    }

    private static void CheckEnemyStates()
    {
        Scene scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        try
        {
            Require(scene.GetPhysicsScene2D() != Physics2D.defaultPhysicsScene, "Preview physics must be isolated.");
            var ground = NewObject(scene, "Ground").AddComponent<BoxCollider2D>();
            ground.transform.position = new Vector3(5f, -0.5f);
            ground.size = new Vector2(30f, 1f);
            var enemyObject = NewObject(scene, "Enemy");
            enemyObject.transform.position = new Vector3(5f, 0.5f);
            enemyObject.AddComponent<BoxCollider2D>();
            var enemy = enemyObject.AddComponent<EnemyController>();
            typeof(EnemyController).GetField("patrolWaitRange", PrivateInstance).SetValue(enemy, Vector2.zero);
            InitializeUnit(enemy);
            enemy.Navigation.ResetHome();
            var player = NewObject(scene, "Player").AddComponent<PlayercatController>();
            InitializeUnit(player);
            player.transform.position = new Vector3(8f, 0.5f);
            player.GetComponent<Rigidbody2D>().simulated = false;
            enemy.SetTarget(player);
            enemy.StateMachine.ChangeState(enemy.IdleState);
            enemy.StateMachine.Update();
            Require(enemy.StateMachine.CurrentState == enemy.MoveState, "Enemy did not start chasing.");
            Physics2D.SyncTransforms();
            for (int i = 0; i < 10; i++)
            {
                enemy.StateMachine.FixedUpdate();
                scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
            }
            Require(enemy.transform.position.x > 5.1f, "Enemy chase did not move.");
            player.transform.position = new Vector3(8f, 3.1f);
            enemy.StateMachine.Update();
            Require(enemy.StateMachine.CurrentState == enemy.ReturnHomeState && enemy.IsReturningHome,
                "Enemy did not return when target changed floor.");
            for (int i = 0; i < 600 && enemy.IsReturningHome; i++)
            {
                enemy.StateMachine.FixedUpdate();
                scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
            }
            Require(!enemy.IsReturningHome && Mathf.Abs(enemy.transform.position.x - 5f) < 0.03f,
                "Enemy did not reach home.");
            enemy.StateMachine.Update();
            Require(enemy.IsPatrolling, "Enemy did not resume patrol.");
            float startX = enemy.transform.position.x;
            bool moved = false;
            for (int i = 0; i < 600; i++)
            {
                enemy.StateMachine.FixedUpdate();
                scene.GetPhysicsScene2D().Simulate(Time.fixedDeltaTime);
                moved |= Mathf.Abs(enemy.transform.position.x - startX) > 0.1f;
                Require(enemy.transform.position.x >= 2.47f && enemy.transform.position.x <= 7.53f,
                    "Enemy left patrol bounds.");
            }
            Require(moved, "Patrol never moved.");
        }
        finally { UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene); }
    }

    private sealed class RecoveryView : IUnitView, IAttackRecoveryView
    {
        public bool IsFinishingAttack { get; set; }
        public void RunAnimation(StudioNAP.AnimationTypeEnum animation) { }
    }

    private static void CheckAttackFacing()
    {
        Scene scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        try
        {
            var player = NewObject(scene, "Player").AddComponent<PlayercatController>();
            var enemy = NewObject(scene, "Enemy").AddComponent<EnemyController>();
            InitializeUnit(player);
            InitializeUnit(enemy);
            enemy.transform.position = Vector3.left;
            var visual = NewObject(scene, "Visual").transform;
            visual.SetParent(player.transform);
            player.Move.ConfigureFacing(visual, true);
            player.SetTarget(enemy);
            Require(player.Attack.BeginAttack(enemy, 0) != 0, "Could not begin facing test attack.");
            player.Move.FaceDirection(1f);
            Require(visual.localScale.x < 0f, "Attack windup lost left facing.");
            player.Attack.CancelPendingHit();
            var recovery = new RecoveryView { IsFinishingAttack = true };
            typeof(BaseUnitController).GetField("unitView", PrivateInstance).SetValue(player, recovery);
            player.ClearTarget();
            player.Move.FaceDirection(1f);
            Require(visual.localScale.x < 0f, "Final attack recovery lost left facing.");
            recovery.IsFinishingAttack = false;
            player.Move.MoveToX(2f);
            Require(visual.localScale.x > 0f, "Facing stayed locked after attack recovery.");
        }
        finally { UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene); }
    }

    private static GameObject NewObject(Scene scene, string name)
    {
        var gameObject = new GameObject(name);
        SceneManager.MoveGameObjectToScene(gameObject, scene);
        return gameObject;
    }

    private static void InitializeUnit(BaseUnitController unit)
    {
        InvokeAwake(unit.GetComponent<UnitHealth>());
        InvokeAwake(unit.GetComponent<UnitMove>());
        InvokeAwake(unit.GetComponent<UnitAttack>());
        InvokeAwake(unit);
    }

    private static void InvokeAwake(Component component) =>
        component.GetType().GetMethod("Awake", PrivateInstance).Invoke(component, null);

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
