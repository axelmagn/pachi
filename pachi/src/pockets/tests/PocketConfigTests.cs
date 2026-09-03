using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public partial class PocketConfigTests : TestClass
{
    public PocketConfigTests(Node testScene) : base(testScene)
    {
    }

    [Test]
    public void PocketConfig_HasStandardDefaultValues()
    {
        using var config = new PocketConfig();

        config.HasArms.ShouldBeTrue();
        config.ArmOpenRotation.ShouldBe(60.0f);
        config.ArmOpenDuration.ShouldBe(5.0f);
        config.ArmTweenDuration.ShouldBe(0.3f);
        config.UsePitchScaleFallback.ShouldBeTrue();
        config.SemitonesPerStep.ShouldBe(2.0f);
    }

    [Test]
    public void CalculatePitchScale_WithinStreamCount_ReturnsUnityPitch()
    {
        using var config = new PocketConfig { UsePitchScaleFallback = true, SemitonesPerStep = 2.0f };

        // Slots below stream count should not pitch shift.
        config.CalculatePitchScale(0, 4).ShouldBe(1.0f);
        config.CalculatePitchScale(2, 4).ShouldBe(1.0f);
        config.CalculatePitchScale(3, 4).ShouldBe(1.0f);
    }

    [Test]
    public void CalculatePitchScale_ExceedingStreamCount_CalculatesPitchEscalation()
    {
        using var config = new PocketConfig { UsePitchScaleFallback = true, SemitonesPerStep = 2.0f };

        // Slot 4 on a 4-stream config is 1 step above (extraSteps = 1).
        float expectedPitch = Mathf.Pow(2.0f, 2.0f / 12.0f);
        config.CalculatePitchScale(4, 4).ShouldBe(expectedPitch, 0.001f);

        // Slot 5 is 2 steps above (extraSteps = 2 -> 4 semitones).
        float expectedPitch2 = Mathf.Pow(2.0f, 4.0f / 12.0f);
        config.CalculatePitchScale(5, 4).ShouldBe(expectedPitch2, 0.001f);
    }

    [Test]
    public void CalculatePitchScale_WithFallbackDisabled_ReturnsUnityPitch()
    {
        using var config = new PocketConfig { UsePitchScaleFallback = false };

        config.CalculatePitchScale(5, 4).ShouldBe(1.0f);
    }
}
