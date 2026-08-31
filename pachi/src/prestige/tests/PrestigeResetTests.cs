using Godot;
using System;
using System.Collections.Generic;
using static TestAssert;

public static class PrestigeResetTests
{
    public static void RunAllTests()
    {
        TestHopperResetToStarterBalls();
        TestSocketResetToStarter();
        TestFullPrestigeResetExecution();
    }

    public static void TestHopperResetToStarterBalls()
    {
        var hopper = new Hopper();
        var ballsRoot = new Node2D();
        hopper.AddChild(ballsRoot);
        hopper.BallsRoot = ballsRoot;

        var timer = new Timer();
        hopper.AddChild(timer);
        hopper.QueuedBallDispenseTimer = timer;

        var hole = new Hole();
        hopper.AddChild(hole);
        hopper.QueuedBallDispenseHoles = new Godot.Collections.Array<Hole> { hole };

        var t1 = new BallVariant { Tier = 1, BasePrice = 2 };
        var t2 = new BallVariant { Tier = 2, BasePrice = 5 };

        // Add some balls
        var ball1 = new Ball { Variant = t1 };
        ballsRoot.AddChild(ball1);
        var ball2 = new Ball { Variant = t2 };
        ballsRoot.AddChild(ball2);

        Assert(hopper.GetTotalBallCount() == 2, "Hopper should have 2 balls initially.");

        // Reset with 5 starter balls of Tier 1
        hopper.ResetToStarterBalls(5, t1);

        Assert(hopper.GetTotalBallCount() == 5, $"Expected 5 balls after reset, got {hopper.GetTotalBallCount()}.");
        Assert(hopper.GetTierCount(1) == 5, $"Expected 5 Tier 1 balls, got {hopper.GetTierCount(1)}.");
        Assert(hopper.GetTierCount(2) == 0, $"Expected 0 Tier 2 balls, got {hopper.GetTierCount(2)}.");
    }

    public static void TestSocketResetToStarter()
    {
        var socket = new Socket2D
        {
            Category = SocketCategory.BeetlePocket
        };

        var starterPocket = new Pocket { Name = "StarterPocket" };
        socket.AddChild(starterPocket);
        socket.AdoptChildComponent();

        var starterScene = new PackedScene();
        var templateNode = new Pocket { Name = "StarterPocketTemplate" };
        starterScene.Pack(templateNode);
        socket.DefaultStarterScene = starterScene;

        Assert(socket.CurrentComponent == starterPocket, "CurrentComponent should be starterPocket.");

        // Mount a custom card component
        var customPocketScene = new PackedScene();
        // Pack a new Pocket
        var customPocketNode = new Pocket { Name = "UpgradedPocket" };
        customPocketScene.Pack(customPocketNode);

        var card = new PackageDealCard
        {
            Category = SocketCategory.BeetlePocket,
            ComponentScene = customPocketScene,
            BallCostTier = 1,
            BallCostCount = 1
        };

        bool mounted = socket.MountPackageDeal(card);
        Assert(mounted, "Should mount upgraded card.");
        Assert(socket.CurrentComponent != starterPocket, "Component should be upgraded.");

        // Reset to starter
        bool resetSuccess = socket.ResetToStarter();
        Assert(resetSuccess, "ResetToStarter should succeed.");
        Assert(socket.CurrentComponent != null, "Socket should have restored starter component.");
    }

    public static void TestFullPrestigeResetExecution()
    {
        var controller = new MainGameController();
        var shop = new CardShop { Name = "CardShop" };
        var meter = new DealMeter { Name = "DealMeter" };
        var prizeMeter = new PrizeMeter { Name = "PrizeMeter" };
        var hopper = new Hopper { Name = "Hopper" };
        var level = new Level { Name = "Level" };

        var ballsRoot = new Node2D { Name = "BallsRoot" };
        level.AddChild(ballsRoot);
        level.BallsRoot = ballsRoot;

        var timer = new Timer { Name = "Timer" };
        hopper.AddChild(timer);
        hopper.QueuedBallDispenseTimer = timer;

        var hopperBallsRoot = new Node2D { Name = "HopperBallsRoot" };
        hopper.AddChild(hopperBallsRoot);
        hopper.BallsRoot = hopperBallsRoot;

        var hole = new Hole { Name = "Hole" };
        hopper.AddChild(hole);
        hopper.QueuedBallDispenseHoles = new Godot.Collections.Array<Hole> { hole };

        controller.Shop = shop;
        controller.Meter = meter;
        controller.PrizeMeter = prizeMeter;
        controller.Hopper = hopper;
        controller.Level = level;

        // Earn 2 tokens in PrizeMeter
        prizeMeter.AddScore(250.0f);
        Assert(prizeMeter.TotalTokens == 2, "PrizeMeter should have 2 total tokens.");
        Assert(prizeMeter.TokensEarnedInRun == 2, "Tokens earned in run should be 2.");

        // Advance DealMeter
        meter.AddProgress(0.75f);
        Assert(meter.Progress > 0.5f, "DealMeter should have progress.");

        // Execute Prestige Reset
        bool resetExecuted = controller.ExecutePrestigeReset();

        Assert(resetExecuted, "ExecutePrestigeReset should return true.");
        Assert(prizeMeter.TotalTokens == 2, "Total tokens must be preserved after reset.");
        Assert(prizeMeter.TokensEarnedInRun == 0, "Tokens earned in run must be reset to 0.");
        Assert(prizeMeter.CurrentProgress == 0.0f, "Prize meter progress must be reset to 0.");
        Assert(Mathf.IsEqualApprox(prizeMeter.CurrentTargetCapacity, 100.0f), "Prize target capacity must reset to 100.");
        Assert(meter.Progress == 0.0f, "DealMeter progress must reset to 0.");
        Assert(hopper.GetTotalBallCount() == 50, $"Hopper should be reloaded with 50 starter balls, got {hopper.GetTotalBallCount()}.");
    }
}
