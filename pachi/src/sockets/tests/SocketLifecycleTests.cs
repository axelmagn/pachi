using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using static TestAssert;

public static class SocketLifecycleTests
{
    public static void RunAllTests()
    {
        TestSocketInitialization();
        TestMountingCategoryValidation();
        TestSafeFlushAndRefund();
        TestLifecycleSignalSequence();
        TestProcessModeSuppression();
        TestStarterBoardInitialization();
    }

    public static void TestSocketInitialization()
    {
        var socket = new Socket2D
        {
            Category = SocketCategory.BeetlePocket,
            SocketId = "test_pocket"
        };

        Assert(socket.CurrentComponent == null, "CurrentComponent should initially be null before adoption.");

        var starterPocket = new Pocket();
        socket.AddChild(starterPocket);
        socket._Ready();

        Assert(socket.CurrentComponent == starterPocket, "Socket2D should auto-adopt embedded ISocketComponent child on _Ready().");
        Assert(socket.Category == SocketCategory.BeetlePocket, "Socket category should match configured category.");
    }

    public static void TestMountingCategoryValidation()
    {
        var pocketSocket = new Socket2D { Category = SocketCategory.BeetlePocket };
        var spinnerSocket = new Socket2D { Category = SocketCategory.Spinner };

        var pocketScene = new PackedScene();
        // In dummy test environment, pack an empty Node2D
        var pocketRoot = new Pocket();
        pocketScene.Pack(pocketRoot);

        var pocketCard = new PackageDealCard
        {
            Category = SocketCategory.BeetlePocket,
            ComponentScene = pocketScene
        };

        var spinnerCard = new PackageDealCard
        {
            Category = SocketCategory.Spinner,
            ComponentScene = pocketScene
        };

        Assert(pocketSocket.CanMount(pocketCard), "Pocket socket should accept BeetlePocket card.");
        Assert(!pocketSocket.CanMount(spinnerCard), "Pocket socket should reject Spinner card.");
        Assert(spinnerSocket.CanMount(spinnerCard), "Spinner socket should accept Spinner card.");
        Assert(!spinnerSocket.CanMount(pocketCard), "Spinner socket should reject BeetlePocket card.");

        var invalidCard = new PackageDealCard
        {
            Category = SocketCategory.BeetlePocket,
            ComponentScene = null!
        };
        Assert(!pocketSocket.CanMount(invalidCard), "Pocket socket should reject card with null ComponentScene.");
    }

    public static void TestSafeFlushAndRefund()
    {
        var socket = new Socket2D { Category = SocketCategory.BeetlePocket };

        var starterPocket = new Pocket();
        var variant1 = new BallVariant { PlaceholderColor = Colors.Red };
        var variant2 = new BallVariant { PlaceholderColor = Colors.Blue };

        starterPocket.InputBalls = new Array<BallVariant> { variant1, variant2 };
        starterPocket.RefreshIndicatorAndSlots();

        // Simulate that slot 0 is occupied (held ball)
        starterPocket.InputBallSlotAvailable![0] = false;
        starterPocket.InputBallSlotAvailable[1] = true;

        socket.AddChild(starterPocket);
        socket._Ready();

        // Set up incoming card
        var nextPocketScene = new PackedScene();
        var nextPocketRoot = new Pocket();
        nextPocketScene.Pack(nextPocketRoot);

        var card = new PackageDealCard
        {
            Category = SocketCategory.BeetlePocket,
            ComponentScene = nextPocketScene
        };

        var hopper = new Hopper();
        var ballsRoot = new Node2D();
        var timer = new Timer();
        var hole = new Hole();
        hopper.BallsRoot = ballsRoot;
        hopper.QueuedBallDispenseTimer = timer;
        hopper.QueuedBallDispenseHoles = new Array<Hole> { hole };

        // Ensure GameConfig is available for ball instancing
        if (GameConfig.Instance == null)
        {
            var configNode = new GameConfig();
            var ballScene = new PackedScene();
            var ballNode = new Ball();
            ballScene.Pack(ballNode);
            configNode.BallScene = ballScene;
            configNode.BallTiers = new Array<BallVariant> { variant1, variant2 };
            configNode._EnterTree();
        }

        bool mountSuccess = socket.MountPackageDeal(card, hopper);
        Assert(mountSuccess, "MountPackageDeal should succeed for matching category card.");
        Assert(hopper.GetTotalBallCount() == 1, $"Hopper should receive exactly 1 refunded ball, got {hopper.GetTotalBallCount()}.");
        Assert(socket.CurrentComponent != starterPocket, "CurrentComponent should be replaced with new instance.");
        Assert(socket.CurrentComponent != null, "CurrentComponent should not be null after mounting.");
    }

    public static void TestLifecycleSignalSequence()
    {
        var socket = new Socket2D { Category = SocketCategory.Spinner };

        var initialSpinner = new Spinner();
        socket.AddChild(initialSpinner);
        socket._Ready();

        var signalsEmitted = new List<string>();

        socket.ComponentUnmounting += (s, node) => signalsEmitted.Add("ComponentUnmounting");
        socket.ComponentUnmounted += (s, node) => signalsEmitted.Add("ComponentUnmounted");
        Node2D? inspectedDuringMounting = null;
        socket.ComponentMounting += (s, node) =>
        {
            signalsEmitted.Add("ComponentMounting");
            // Test that querying CurrentComponent during ComponentMounting returns the incoming component without duplicate OnMounted triggers
            inspectedDuringMounting = s.CurrentComponent;
        };
        socket.ComponentMounted += (s, node) => signalsEmitted.Add("ComponentMounted");

        var scene = new PackedScene();
        var newSpinner = new Spinner();
        scene.Pack(newSpinner);

        var card = new PackageDealCard
        {
            Category = SocketCategory.Spinner,
            ComponentScene = scene
        };

        socket.MountPackageDeal(card);

        Assert(inspectedDuringMounting != null, "CurrentComponent should be valid when queried during ComponentMounting.");
        Assert(signalsEmitted.Count == 4, $"Expected 4 signals, got {signalsEmitted.Count}.");
        Assert(signalsEmitted[0] == "ComponentUnmounting", $"Signal 0 should be ComponentUnmounting, got {signalsEmitted[0]}.");
        Assert(signalsEmitted[1] == "ComponentUnmounted", $"Signal 1 should be ComponentUnmounted, got {signalsEmitted[1]}.");
        Assert(signalsEmitted[2] == "ComponentMounting", $"Signal 2 should be ComponentMounting, got {signalsEmitted[2]}.");
        Assert(signalsEmitted[3] == "ComponentMounted", $"Signal 3 should be ComponentMounted, got {signalsEmitted[3]}.");
    }

    public static void TestProcessModeSuppression()
    {
        var socket = new Socket2D { Category = SocketCategory.BeetlePocket };

        var pocket = new Pocket();
        var colShape = new CollisionShape2D { Shape = new CircleShape2D() };
        pocket.AddChild(colShape);
        socket.AddChild(pocket);
        socket._Ready();

        Assert(pocket.ProcessMode == Node.ProcessModeEnum.Inherit, "ProcessMode should initially be Inherit.");
        Assert(!colShape.Disabled, "Collider should initially be enabled.");

        var scene = new PackedScene();
        var newPocket = new Pocket();
        scene.Pack(newPocket);

        var card = new PackageDealCard
        {
            Category = SocketCategory.BeetlePocket,
            ComponentScene = scene
        };

        socket.MountPackageDeal(card);

        Assert(pocket.ProcessMode == Node.ProcessModeEnum.Disabled, "Outgoing component ProcessMode should be set to Disabled.");
        Assert(colShape.Disabled, "Outgoing component colliders should be disabled.");
    }

    public static void TestStarterBoardInitialization()
    {
        var levelScene = ResourceLoader.Load<PackedScene>("res://src/levels/level.tscn");
        Assert(levelScene != null, "level.tscn should load successfully.");

        var level = levelScene!.Instantiate<Level>();
        var tree = Engine.GetMainLoop() as SceneTree;
        Assert(tree != null, "SceneTree should not be null.");

        tree!.Root.AddChild(level);

        var sockets = level.GetNodeOrNull<Node2D>("Sockets");
        Assert(sockets != null, "Sockets container should exist in level.tscn.");

        var yakCenter = level.GetNodeOrNull<Socket2D>("Sockets/Yakumono/SocketYakumonoCenter");
        Assert(yakCenter != null, "SocketYakumonoCenter should exist.");
        Assert(yakCenter!.Category == SocketCategory.Yakumono, "SocketYakumonoCenter category should be Yakumono.");
        Assert(yakCenter.CurrentComponent is Yakumono, "SocketYakumonoCenter should adopt StarterYakumono.");

        var spinL = level.GetNodeOrNull<Socket2D>("Sockets/Spinners/SocketSpinnerLeft");
        Assert(spinL != null && spinL.Category == SocketCategory.Spinner, "SocketSpinnerLeft should exist and be Spinner.");
        Assert(spinL!.CurrentComponent is Spinner, "SocketSpinnerLeft should adopt StarterSpinnerLeft.");

        var spinR = level.GetNodeOrNull<Socket2D>("Sockets/Spinners/SocketSpinnerRight");
        Assert(spinR != null && spinR.Category == SocketCategory.Spinner, "SocketSpinnerRight should exist and be Spinner.");
        Assert(spinR!.CurrentComponent is Spinner, "SocketSpinnerRight should adopt StarterSpinnerRight.");

        var pockL = level.GetNodeOrNull<Socket2D>("Sockets/Pockets/SocketPocketLeft");
        Assert(pockL != null && pockL.Category == SocketCategory.BeetlePocket, "SocketPocketLeft should exist and be BeetlePocket.");
        Assert(pockL!.CurrentComponent is Pocket, "SocketPocketLeft should adopt StarterPocketLeft.");

        var pockC = level.GetNodeOrNull<Socket2D>("Sockets/Pockets/SocketPocketCenter");
        Assert(pockC != null && pockC.Category == SocketCategory.BeetlePocket, "SocketPocketCenter should exist and be BeetlePocket.");
        Assert(pockC!.CurrentComponent is Pocket, "SocketPocketCenter should adopt StarterPocketCenter.");

        var pockR = level.GetNodeOrNull<Socket2D>("Sockets/Pockets/SocketPocketRight");
        Assert(pockR != null && pockR.Category == SocketCategory.BeetlePocket, "SocketPocketRight should exist and be BeetlePocket.");
        Assert(pockR!.CurrentComponent is Pocket, "SocketPocketRight should adopt StarterPocketRight.");

        tree.Root.RemoveChild(level);
        level.QueueFree();
    }
}
