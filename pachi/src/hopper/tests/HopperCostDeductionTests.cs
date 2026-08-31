using Godot;
using System.Collections.Generic;
using System.Linq;
using static TestAssert;

public static class HopperCostDeductionTests
{
    public static void RunAllTests()
    {
        TestTierCounting();
        TestHasBallCost();
        TestAtomicDeductionFailure();
        TestFifoDeductionOrderPreservation();
    }

    private static Ball CreateMockBall(int tier)
    {
        var ball = new Ball();
        ball.Variant = new BallVariant { Tier = tier, PlaceholderColor = Colors.White };
        return ball;
    }

    public static void TestTierCounting()
    {
        var hopper = new Hopper();
        var ballsRoot = new Node2D();
        hopper.AddChild(ballsRoot);
        hopper.BallsRoot = ballsRoot;

        // Manually populate contained balls: 2x Tier 1, 1x Tier 2
        var b1 = CreateMockBall(1);
        var b2 = CreateMockBall(1);
        var b3 = CreateMockBall(2);
        ballsRoot.AddChild(b1);
        ballsRoot.AddChild(b2);
        ballsRoot.AddChild(b3);

        // Call _Ready to register contained balls
        var timer = new Timer();
        hopper.AddChild(timer);
        hopper.QueuedBallDispenseTimer = timer;
        hopper.QueuedBallDispenseHoles = new Godot.Collections.Array<Hole> { new Hole() };

        // Test tier counting
        Assert(hopper.GetTierCount(1) == 2, $"Expected 2 Tier-1 balls, got {hopper.GetTierCount(1)}.");
        Assert(hopper.GetTierCount(2) == 1, $"Expected 1 Tier-2 ball, got {hopper.GetTierCount(2)}.");
        Assert(hopper.GetTierCount(3) == 0, $"Expected 0 Tier-3 balls, got {hopper.GetTierCount(3)}.");
        Assert(hopper.GetTotalBallCount() == 3, $"Expected 3 total balls, got {hopper.GetTotalBallCount()}.");

        hopper.QueueFree();
    }

    public static void TestHasBallCost()
    {
        var hopper = new Hopper();
        var ballsRoot = new Node2D();
        hopper.AddChild(ballsRoot);
        hopper.BallsRoot = ballsRoot;

        var b1 = CreateMockBall(1);
        var b2 = CreateMockBall(1);
        var b3 = CreateMockBall(2);
        ballsRoot.AddChild(b1);
        ballsRoot.AddChild(b2);
        ballsRoot.AddChild(b3);

        Assert(hopper.HasBallCost(1, 2), "Should afford 2x Tier-1.");
        Assert(!hopper.HasBallCost(1, 3), "Should not afford 3x Tier-1.");
        Assert(hopper.HasBallCost(2, 1), "Should afford 1x Tier-2.");
        Assert(!hopper.HasBallCost(2, 2), "Should not afford 2x Tier-2.");
        Assert(!hopper.HasBallCost(3, 1), "Should not afford 1x Tier-3.");

        hopper.QueueFree();
    }

    public static void TestAtomicDeductionFailure()
    {
        var hopper = new Hopper();
        var ballsRoot = new Node2D();
        hopper.AddChild(ballsRoot);
        hopper.BallsRoot = ballsRoot;

        var b1 = CreateMockBall(1);
        ballsRoot.AddChild(b1);

        bool result = hopper.DeductBallCost(1, 2);
        Assert(!result, "DeductBallCost should return false when insufficient balls.");
        Assert(hopper.GetTierCount(1) == 1, "Inventory should remain unchanged on failed deduction.");

        hopper.QueueFree();
    }

    public static void TestFifoDeductionOrderPreservation()
    {
        var hopper = new Hopper();
        var ballsRoot = new Node2D();
        hopper.AddChild(ballsRoot);
        hopper.BallsRoot = ballsRoot;

        // Sequence of contained balls: T1 (id: A), T2 (id: B), T1 (id: C), T3 (id: D)
        var ballA = CreateMockBall(1);
        ballA.Name = "BallA";
        var ballB = CreateMockBall(2);
        ballB.Name = "BallB";
        var ballC = CreateMockBall(1);
        ballC.Name = "BallC";
        var ballD = CreateMockBall(3);
        ballD.Name = "BallD";

        ballsRoot.AddChild(ballA);
        ballsRoot.AddChild(ballB);
        ballsRoot.AddChild(ballC);
        ballsRoot.AddChild(ballD);

        // Deduct 1x Tier 1 -> should remove BallA, leaving BallB (T2), BallC (T1), BallD (T3)
        bool success = hopper.DeductBallCost(1, 1);
        Assert(success, "Deduction of 1x Tier-1 should succeed.");
        Assert(hopper.GetTierCount(1) == 1, "Should have 1 Tier-1 ball remaining.");
        Assert(hopper.GetTierCount(2) == 1, "Should still have 1 Tier-2 ball.");
        Assert(hopper.GetTierCount(3) == 1, "Should still have 1 Tier-3 ball.");

        // First contained ball popped should now be BallB (T2)
        Ball? popped = hopper.PopFirstContainedBall();
        Assert(popped != null && popped.Name == "BallB", $"Next popped ball should be BallB (T2), got {popped?.Name}.");

        // Next popped should be BallC (T1)
        popped = hopper.PopFirstContainedBall();
        Assert(popped != null && popped.Name == "BallC", $"Next popped ball should be BallC (T1), got {popped?.Name}.");

        // Next popped should be BallD (T3)
        popped = hopper.PopFirstContainedBall();
        Assert(popped != null && popped.Name == "BallD", $"Next popped ball should be BallD (T3), got {popped?.Name}.");

        hopper.QueueFree();
    }
}
