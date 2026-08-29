using Godot;
using System;
using static TestAssert;

public static class BallStuckTests
{

    public static void RunAllTests()
    {
        TestDisplacementTrackingAccumulationAndReset();
        TestExemptionsDoNotAccumulateStuckTime();
        TestNudgeEscalationSequence();
        TestNudgeCountResetsOnDisplacement();
        TestBallRefundLifecycle();
    }

    public static void TestDisplacementTrackingAccumulationAndReset()
    {
        var ball = new Ball
        {
            DetectStuck = true,
            IsInPlay = true,
            Freeze = false,
            StuckDisplacementThreshold = 10.0f,
            GlobalPosition = new Vector2(100, 100)
        };

        // First step initializes anchor and accumulates delta
        ball.ProcessStuckDetection(0.5);
        Assert(ball.StuckAnchorPosition == new Vector2(100, 100), "Anchor position should be initialized to starting position.");
        Assert(Mathf.IsEqualApprox((float)ball.AccumulatedStuckTime, 0.5f), "Accumulated stuck time should be 0.5s after 0.5s stationary.");

        // Small jitter movement within threshold (e.g. 5px) should continue accumulating
        ball.GlobalPosition = new Vector2(104, 103); // distance = 5px <= 10px
        ball.ProcessStuckDetection(0.5);
        Assert(Mathf.IsEqualApprox((float)ball.AccumulatedStuckTime, 1.0f), "Accumulated stuck time should be 1.0s after small displacement within threshold.");

        // Large movement exceeding threshold (e.g. 20px) should reset timer and anchor
        ball.GlobalPosition = new Vector2(130, 100); // distance from (100,100) is 30px > 10px
        ball.ProcessStuckDetection(0.1);
        Assert(Mathf.IsEqualApprox((float)ball.AccumulatedStuckTime, 0.0f), "Accumulated stuck time should reset to 0.0s after displacement exceeding threshold.");
        Assert(ball.StuckAnchorPosition == new Vector2(130, 100), "Anchor position should update to new position after displacement reset.");
    }

    public static void TestExemptionsDoNotAccumulateStuckTime()
    {
        // 1. Freeze exemption
        var frozenBall = new Ball
        {
            DetectStuck = true,
            IsInPlay = true,
            Freeze = true,
            GlobalPosition = new Vector2(50, 50)
        };
        frozenBall.ProcessStuckDetection(1.0);
        Assert(Mathf.IsEqualApprox((float)frozenBall.AccumulatedStuckTime, 0.0f), "Frozen ball should not accumulate stuck time.");

        // 2. DetectStuck disabled exemption
        var disabledBall = new Ball
        {
            DetectStuck = false,
            IsInPlay = true,
            Freeze = false,
            GlobalPosition = new Vector2(50, 50)
        };
        disabledBall.ProcessStuckDetection(1.0);
        Assert(Mathf.IsEqualApprox((float)disabledBall.AccumulatedStuckTime, 0.0f), "Ball with DetectStuck disabled should not accumulate stuck time.");

        // 3. Hopper exemption (IsInPlay = false)
        var hopperBall = new Ball
        {
            DetectStuck = true,
            IsInPlay = false,
            Freeze = false,
            GlobalPosition = new Vector2(50, 50)
        };
        hopperBall.ProcessStuckDetection(1.0);
        Assert(Mathf.IsEqualApprox((float)hopperBall.AccumulatedStuckTime, 0.0f), "Ball in hopper (IsInPlay = false) should not accumulate stuck time.");

        // 4. FadeIn transition exemption
        var fadeInBall = new Ball
        {
            DetectStuck = true,
            IsInPlay = true,
            Freeze = false,
            CurrentTransitionState = Ball.TransitionState.FadeIn,
            GlobalPosition = new Vector2(50, 50)
        };
        fadeInBall.ProcessStuckDetection(1.0);
        Assert(Mathf.IsEqualApprox((float)fadeInBall.AccumulatedStuckTime, 0.0f), "Ball in FadeIn transition should not accumulate stuck time.");

        // 5. FadeOut transition exemption
        var fadeOutBall = new Ball
        {
            DetectStuck = true,
            IsInPlay = true,
            Freeze = false,
            CurrentTransitionState = Ball.TransitionState.FadeOut,
            GlobalPosition = new Vector2(50, 50)
        };
        fadeOutBall.ProcessStuckDetection(1.0);
        Assert(Mathf.IsEqualApprox((float)fadeOutBall.AccumulatedStuckTime, 0.0f), "Ball in FadeOut transition should not accumulate stuck time.");
    }

    public static void TestNudgeEscalationSequence()
    {
        var ball = new Ball
        {
            DetectStuck = true,
            IsInPlay = true,
            Freeze = false,
            InitialNudgeDuration = 2.0f,
            NudgeRetryInterval = 1.0f,
            MaxNudgeRetries = 2,
            NudgeImpulseStrength = 300.0f,
            NudgeAngleSpreadDeg = 30.0f,
            GlobalPosition = new Vector2(100, 100)
        };

        int nudgeSignalsFired = 0;
        Vector2 lastImpulse = Vector2.Zero;
        ball.Nudged += (Vector2 impulse) =>
        {
            nudgeSignalsFired++;
            lastImpulse = impulse;
        };

        // Advance 1.9 seconds: no nudge yet
        ball.ProcessStuckDetection(1.9);
        Assert(ball.NudgeCount == 0, "Nudge count should be 0 before 2.0s threshold.");
        Assert(nudgeSignalsFired == 0, "No nudge signal should fire before 2.0s threshold.");

        // Advance 0.2 seconds (total 2.1s): first nudge fires
        ball.ProcessStuckDetection(0.2);
        Assert(ball.NudgeCount == 1, "First nudge should fire at 2.0s threshold.");
        Assert(nudgeSignalsFired == 1, "Nudge signal should fire once on first nudge.");
        Assert(lastImpulse.Y < 0.0f, "Nudge impulse should be directed upwards (negative Y in 2D).");

        // Advance 0.8 seconds (total 2.9s): no second nudge yet (retry interval is 1.0s, so target is 3.0s)
        ball.ProcessStuckDetection(0.8);
        Assert(ball.NudgeCount == 1, "Second nudge should not fire before 3.0s.");
        Assert(nudgeSignalsFired == 1, "No additional nudge signal before 3.0s.");

        // Advance 0.2 seconds (total 3.1s): second nudge fires
        ball.ProcessStuckDetection(0.2);
        Assert(ball.NudgeCount == 2, "Second nudge should fire at 3.0s (2.0s + 1.0s).");
        Assert(nudgeSignalsFired == 2, "Second nudge signal should have fired.");

        // Advance another 1.0s (total 4.1s): max nudges (2) reached, so no third nudge
        ball.ProcessStuckDetection(1.0);
        Assert(ball.NudgeCount == 2, "Nudge count should not exceed MaxNudgeRetries (2).");
        Assert(nudgeSignalsFired == 2, "No third nudge signal should fire when MaxNudgeRetries is 2.");
    }

    public static void TestNudgeCountResetsOnDisplacement()
    {
        var ball = new Ball
        {
            DetectStuck = true,
            IsInPlay = true,
            Freeze = false,
            InitialNudgeDuration = 2.0f,
            StuckDisplacementThreshold = 10.0f,
            GlobalPosition = new Vector2(100, 100)
        };

        // Advance past initial nudge
        ball.ProcessStuckDetection(2.1);
        Assert(ball.NudgeCount == 1, "Ball should have received 1 nudge.");

        // Ball moves > threshold (e.g. 50px away)
        ball.GlobalPosition = new Vector2(150, 100);
        ball.ProcessStuckDetection(0.1);

        Assert(ball.NudgeCount == 0, "Nudge count should reset to 0 upon displacement.");
        Assert(Mathf.IsEqualApprox((float)ball.AccumulatedStuckTime, 0.0f), "Stuck time should reset to 0 upon displacement.");
    }

    public static void TestBallRefundLifecycle()
    {
        var globalEventsObj = new GlobalEvents();
        globalEventsObj._EnterTree();

        var variant = new BallVariant { BasePrice = 10, PlaceholderColor = Colors.Red };
        var ball = new Ball
        {
            DetectStuck = true,
            IsInPlay = true,
            Freeze = false,
            InitialNudgeDuration = 2.0f,
            NudgeRetryInterval = 1.0f,
            MaxNudgeRetries = 2,
            RefundTimeout = 4.5f,
            Variant = variant,
            GlobalPosition = new Vector2(100, 100)
        };

        BallVariant? awardedVariant = null;
        globalEventsObj.BallAwarded += (BallVariant v) =>
        {
            awardedVariant = v;
        };

        // Advance to 4.4s (no refund yet)
        ball.ProcessStuckDetection(4.4);
        Assert(ball.CurrentTransitionState == Ball.TransitionState.None, "Ball should not be in FadeOut before RefundTimeout.");

        // Advance past 4.5s (refund triggers)
        ball.ProcessStuckDetection(0.2);
        Assert(ball.CurrentTransitionState == Ball.TransitionState.FadeOut, "Ball should transition to FadeOut upon refund.");
        Assert(ball.Freeze, "Ball should be frozen upon refund.");

        // Simulate fade out completion
        ball.EmitSignal(Ball.SignalName.FadeOutFinished);
        Assert(awardedVariant == variant, "GlobalEvents.BallAwarded should be emitted with ball's variant upon refund.");
        Assert(ball.IsQueuedForDeletion(), "Ball should be queued for deletion upon refund.");
    }
}
