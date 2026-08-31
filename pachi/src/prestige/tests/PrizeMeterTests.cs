using Godot;
using System;
using static TestAssert;

public static class PrizeMeterTests
{
    public static void RunAllTests()
    {
        TestInitialStateAndCapacity();
        TestProgressAccumulation();
        TestTokenAwardAndExponentialScaling();
        TestMultiTokenCarryover();
        TestResetRunStatePreservesTotalTokens();
        TestResetAllWipesTotalTokens();
        TestBallTierValueCalculation();
    }

    public static void TestInitialStateAndCapacity()
    {
        var meter = new PrizeMeter
        {
            BaseTarget = 100.0f,
            ScalingMultiplier = 1.50f
        };

        Assert(meter.CurrentProgress == 0.0f, "CurrentProgress should start at 0.");
        Assert(meter.TokensEarnedInRun == 0, "TokensEarnedInRun should start at 0.");
        Assert(meter.TotalTokens == 0, "TotalTokens should start at 0.");
        Assert(Mathf.IsEqualApprox(meter.CurrentTargetCapacity, 100.0f), $"Initial target should be 100, got {meter.CurrentTargetCapacity}.");
        Assert(Mathf.IsEqualApprox(meter.ProgressPercent, 0.0f), "Initial ProgressPercent should be 0.0.");
        Assert(!meter.CanPrestigeReset, "CanPrestigeReset should be false when 0 tokens held.");
    }

    public static void TestProgressAccumulation()
    {
        var meter = new PrizeMeter
        {
            BaseTarget = 100.0f,
            ScalingMultiplier = 1.50f
        };

        bool progressSignalFired = false;
        float reportedProgress = 0.0f;
        float reportedTarget = 0.0f;
        float reportedPercent = 0.0f;

        meter.ProgressChanged += (current, target, percent) =>
        {
            progressSignalFired = true;
            reportedProgress = current;
            reportedTarget = target;
            reportedPercent = percent;
        };

        meter.AddScore(40.0f);

        Assert(progressSignalFired, "ProgressChanged signal should fire upon AddScore.");
        Assert(Mathf.IsEqualApprox(meter.CurrentProgress, 40.0f), $"Progress should be 40.0, got {meter.CurrentProgress}.");
        Assert(Mathf.IsEqualApprox(reportedProgress, 40.0f), "Signal should report current progress.");
        Assert(Mathf.IsEqualApprox(reportedTarget, 100.0f), "Signal should report target capacity 100.");
        Assert(Mathf.IsEqualApprox(reportedPercent, 0.40f, 0.001f), "Signal should report percent 0.40.");
        Assert(meter.TokensEarnedInRun == 0, "No tokens should be awarded under 100 points.");
    }

    public static void TestTokenAwardAndExponentialScaling()
    {
        var meter = new PrizeMeter
        {
            BaseTarget = 100.0f,
            ScalingMultiplier = 1.50f
        };

        int awardedTotal = 0;
        int awardedRun = 0;
        meter.PrizeTokenAwarded += (total, run) =>
        {
            awardedTotal = total;
            awardedRun = run;
        };

        // Add 100 points -> exactly 1 token
        meter.AddScore(100.0f);

        Assert(meter.TokensEarnedInRun == 1, $"TokensEarnedInRun should be 1, got {meter.TokensEarnedInRun}.");
        Assert(meter.TotalTokens == 1, $"TotalTokens should be 1, got {meter.TotalTokens}.");
        Assert(awardedTotal == 1, "PrizeTokenAwarded should report total = 1.");
        Assert(awardedRun == 1, "PrizeTokenAwarded should report run = 1.");
        Assert(meter.CanPrestigeReset, "CanPrestigeReset should be true with 1 token.");
        Assert(Mathf.IsEqualApprox(meter.CurrentProgress, 0.0f), $"Progress should carry over 0.0, got {meter.CurrentProgress}.");

        // Target should scale to 100 * (1.50)^1 = 150.0
        Assert(Mathf.IsEqualApprox(meter.CurrentTargetCapacity, 150.0f, 0.001f), $"Target should scale to 150.0, got {meter.CurrentTargetCapacity}.");

        // Add 150 points -> 2nd token, target scales to 100 * (1.50)^2 = 225.0
        meter.AddScore(150.0f);
        Assert(meter.TokensEarnedInRun == 2, $"TokensEarnedInRun should be 2, got {meter.TokensEarnedInRun}.");
        Assert(meter.TotalTokens == 2, $"TotalTokens should be 2, got {meter.TotalTokens}.");
        Assert(Mathf.IsEqualApprox(meter.CurrentTargetCapacity, 225.0f, 0.001f), $"Target should scale to 225.0, got {meter.CurrentTargetCapacity}.");
    }

    public static void TestMultiTokenCarryover()
    {
        var meter = new PrizeMeter
        {
            BaseTarget = 100.0f,
            ScalingMultiplier = 1.50f
        };

        // Level 0: 100 pts. Level 1: 150 pts. Total for 2 tokens = 250 pts.
        // Adding 280 pts should award 2 tokens with 30 pts carryover into Level 2 (target 225).
        meter.AddScore(280.0f);

        Assert(meter.TokensEarnedInRun == 2, $"Expected 2 tokens in run, got {meter.TokensEarnedInRun}.");
        Assert(meter.TotalTokens == 2, $"Expected 2 total tokens, got {meter.TotalTokens}.");
        Assert(Mathf.IsEqualApprox(meter.CurrentProgress, 30.0f, 0.001f), $"Expected 30.0 carryover, got {meter.CurrentProgress}.");
        Assert(Mathf.IsEqualApprox(meter.CurrentTargetCapacity, 225.0f, 0.001f), $"Target capacity for level 2 should be 225.0, got {meter.CurrentTargetCapacity}.");
        Assert(Mathf.IsEqualApprox(meter.ProgressPercent, 30.0f / 225.0f, 0.001f), $"Expected percent ~0.1333, got {meter.ProgressPercent}.");
    }

    public static void TestResetRunStatePreservesTotalTokens()
    {
        var meter = new PrizeMeter
        {
            BaseTarget = 100.0f,
            ScalingMultiplier = 1.50f
        };

        meter.AddScore(100.0f); // 1 token
        Assert(meter.TotalTokens == 1, "Total tokens = 1.");

        meter.ResetRunState();

        Assert(meter.CurrentProgress == 0.0f, "CurrentProgress should reset to 0.");
        Assert(meter.TokensEarnedInRun == 0, "TokensEarnedInRun should reset to 0.");
        Assert(meter.TotalTokens == 1, "TotalTokens should be preserved on run reset.");
        Assert(Mathf.IsEqualApprox(meter.CurrentTargetCapacity, 100.0f), "Target capacity should reset to BaseTarget 100.");
        Assert(meter.CanPrestigeReset, "CanPrestigeReset should still be true since TotalTokens >= 1.");
    }

    public static void TestResetAllWipesTotalTokens()
    {
        var meter = new PrizeMeter
        {
            BaseTarget = 100.0f,
            ScalingMultiplier = 1.50f
        };

        meter.AddScore(250.0f);
        Assert(meter.TotalTokens == 2, "Total tokens should be 2.");

        meter.ResetAll();

        Assert(meter.CurrentProgress == 0.0f, "CurrentProgress should be 0.");
        Assert(meter.TokensEarnedInRun == 0, "TokensEarnedInRun should be 0.");
        Assert(meter.TotalTokens == 0, "TotalTokens should be reset to 0.");
        Assert(!meter.CanPrestigeReset, "CanPrestigeReset should be false.");
    }

    public static void TestBallTierValueCalculation()
    {
        var meter = new PrizeMeter();

        var t1 = new BallVariant { Tier = 1, BasePrice = 2 };
        var t2 = new BallVariant { Tier = 2, BasePrice = 5 };
        var t3 = new BallVariant { Tier = 3, BasePrice = 15 };
        var t4 = new BallVariant { Tier = 4, BasePrice = 50 };

        Assert(meter.GetTierScoreValue(t1) == 1.0f, "Tier 1 should be 1 point.");
        Assert(meter.GetTierScoreValue(t2) == 3.0f, "Tier 2 should be 3 points.");
        Assert(meter.GetTierScoreValue(t3) == 10.0f, "Tier 3 should be 10 points.");
        Assert(meter.GetTierScoreValue(t4) == 50.0f, "Tier 4 should be 50 points.");
    }
}
