using Godot;
using System;
using static TestAssert;

public static class PrizeMeterUITests
{
    public static void RunAllTests()
    {
        TestUIInitialization();
        TestUIBindingAndProgressUpdates();
        TestResetButtonEnabledWithTokens();
        TestResetButtonPressedEmitsSignal();
    }

    public static void TestUIInitialization()
    {
        var ui = new PrizeMeterUI();
        ui.InitControls();

        Assert(ui.ProgressBar != null, "ProgressBar should not be null.");
        Assert(ui.ProgressLabel != null, "ProgressLabel should not be null.");
        Assert(ui.TokenLabel != null, "TokenLabel should not be null.");
        Assert(ui.ResetButton != null, "ResetButton should not be null.");
        Assert(ui.ResetButton!.Disabled, "ResetButton should start disabled with 0 tokens.");
    }

    public static void TestUIBindingAndProgressUpdates()
    {
        var meter = new PrizeMeter
        {
            BaseTarget = 100.0f,
            ScalingMultiplier = 1.50f
        };

        var ui = new PrizeMeterUI();
        ui.Bind(meter);

        Assert(ui.ProgressBar!.Value == 0, "Progress bar should show 0.");
        Assert(ui.ResetButton!.Disabled, "Reset button should be disabled.");

        // Add 50 points
        meter.AddScore(50.0f);
        Assert(Mathf.IsEqualApprox((float)ui.ProgressBar.Value, 50.0f, 0.01f), $"Expected 50% on progress bar, got {ui.ProgressBar.Value}.");
        Assert(ui.ProgressLabel!.Text.Contains("50") && ui.ProgressLabel.Text.Contains("100"), $"Progress label should show 50 / 100, got '{ui.ProgressLabel.Text}'.");
    }

    public static void TestResetButtonEnabledWithTokens()
    {
        var meter = new PrizeMeter
        {
            BaseTarget = 100.0f,
            ScalingMultiplier = 1.50f
        };

        var ui = new PrizeMeterUI();
        ui.Bind(meter);

        Assert(ui.ResetButton!.Disabled, "Reset button should be disabled before token award.");

        // Award 1 token
        meter.AddScore(100.0f);

        Assert(!ui.ResetButton.Disabled, "Reset button should be enabled after token is awarded.");
        Assert(ui.TokenLabel!.Text.Contains('1'), $"Token label should show 1 token, got '{ui.TokenLabel.Text}'.");
    }

    public static void TestResetButtonPressedEmitsSignal()
    {
        var meter = new PrizeMeter();
        meter.AddScore(100.0f);

        var ui = new PrizeMeterUI();
        ui.Bind(meter);

        bool resetClicked = false;
        ui.ResetRequested += () => resetClicked = true;

        ui.OnResetButtonPressed();

        Assert(resetClicked, "ResetRequested signal should fire when reset button is pressed.");
    }
}
