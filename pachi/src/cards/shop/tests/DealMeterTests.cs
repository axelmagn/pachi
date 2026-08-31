using Godot;
using System;
using static TestAssert;

public static class DealMeterTests
{
    public static void RunAllTests()
    {
        TestPassiveAccumulation();
        TestFlatScoreChunks();
        TestSpeedMultipliersAndStacking();
        TestDealThresholdReachedAndReset();
        TestSpeedMultiplierExpiration();
    }

    public static void TestPassiveAccumulation()
    {
        var meter = new DealMeter
        {
            BaselinePeriod = 20.0f
        };

        Assert(meter.Progress == 0.0f, "Progress should start at 0.");
        Assert(Mathf.IsEqualApprox(meter.EffectiveRateMultiplier, 1.0f), "Initial rate multiplier should be 1.0.");

        // Advance 2.0 seconds -> 2.0s / 20.0s = 0.10 (10%)
        meter.Advance(2.0);
        Assert(Mathf.IsEqualApprox(meter.Progress, 0.10f, 0.001f), $"Expected 0.10 progress after 2s, got {meter.Progress}.");

        // Advance another 8.0 seconds -> total 10.0s = 0.50 (50%)
        meter.Advance(8.0);
        Assert(Mathf.IsEqualApprox(meter.Progress, 0.50f, 0.001f), $"Expected 0.50 progress after 10s, got {meter.Progress}.");
    }

    public static void TestFlatScoreChunks()
    {
        var meter = new DealMeter
        {
            BaselinePeriod = 20.0f,
            PocketBoostChunk = 0.10f,
            YakumonoBoostChunk = 0.35f
        };

        meter.AddPocketHit();
        Assert(meter.Progress >= 0.10f, $"Pocket hit should add at least 0.10 flat chunk, got {meter.Progress}.");

        float beforeYaku = meter.Progress;
        meter.AddYakumonoHit();
        Assert(meter.Progress >= beforeYaku + 0.35f - 0.001f, $"Yakumono hit should add 0.35 flat chunk, got {meter.Progress}.");
    }

    public static void TestSpeedMultipliersAndStacking()
    {
        var meter = new DealMeter
        {
            BaselinePeriod = 20.0f,
            PocketSpeedMultiplier = 0.5f,
            YakumonoSpeedMultiplier = 2.0f,
            BoostDuration = 5.0f
        };

        // Base rate = 1.0x
        Assert(Mathf.IsEqualApprox(meter.EffectiveRateMultiplier, 1.0f), "Base rate should be 1.0.");

        // Pocket hit adds +0.5x -> 1.5x
        meter.AddSpeedMultiplier(0.5f, 5.0f);
        Assert(Mathf.IsEqualApprox(meter.EffectiveRateMultiplier, 1.5f, 0.001f), $"Expected 1.5x, got {meter.EffectiveRateMultiplier}.");

        // Yakumono hit adds +2.0x -> 1.5 + 2.0 = 3.5x
        meter.AddSpeedMultiplier(2.0f, 5.0f);
        Assert(Mathf.IsEqualApprox(meter.EffectiveRateMultiplier, 3.5f, 0.001f), $"Expected 3.5x stacked, got {meter.EffectiveRateMultiplier}.");

        // Advance 1.0s at 3.5x -> base rate is 0.05/s * 3.5 = 0.175
        float progressBefore = meter.Progress;
        meter.Advance(1.0);
        float expectedDelta = (1.0f / 20.0f) * 3.5f * 1.0f;
        Assert(Mathf.IsEqualApprox(meter.Progress - progressBefore, expectedDelta, 0.001f), $"Expected progress delta {expectedDelta}, got {meter.Progress - progressBefore}.");
    }

    public static void TestSpeedMultiplierExpiration()
    {
        var meter = new DealMeter
        {
            BaselinePeriod = 20.0f,
            BoostDuration = 3.0f
        };

        meter.AddSpeedMultiplier(1.0f, 3.0f); // 2.0x effective
        Assert(Mathf.IsEqualApprox(meter.EffectiveRateMultiplier, 2.0f), "Should be 2.0x.");

        meter.Advance(2.0); // 1.0s remaining
        Assert(Mathf.IsEqualApprox(meter.EffectiveRateMultiplier, 2.0f), "Should still be 2.0x after 2s.");

        meter.Advance(1.5); // expired
        Assert(Mathf.IsEqualApprox(meter.EffectiveRateMultiplier, 1.0f), "Should return to 1.0x after boost duration expires.");
    }

    public static void TestDealThresholdReachedAndReset()
    {
        var meter = new DealMeter
        {
            BaselinePeriod = 20.0f
        };

        bool thresholdTriggered = false;
        meter.DealThresholdReached += () =>
        {
            thresholdTriggered = true;
        };

        // Advance to 95%
        meter.Advance(19.0);
        Assert(!thresholdTriggered, "Threshold should not trigger before 100%.");
        Assert(Mathf.IsEqualApprox(meter.Progress, 0.95f, 0.001f), "Progress should be 0.95.");

        // Advance past 100%
        meter.Advance(2.0);
        Assert(thresholdTriggered, "Threshold signal should trigger when reaching 100%.");
        Assert(meter.Progress == 0.0f, $"Progress should reset cleanly to 0.0f upon reaching threshold, got {meter.Progress}.");
    }
}
