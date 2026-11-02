using System;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public partial class AutoFireSamplerTests : TestClass
{
    public AutoFireSamplerTests(Node testScene) : base(testScene)
    {
    }

    [Test]
    public void Next_WithoutCurve_ReturnsValuesInUnitRange()
    {
        var sampler = new AutoFireSampler(null, 100, 20, new Random(1));
        for (int i = 0; i < 200; i++)
        {
            sampler.Next().ShouldBeInRange(0.0f, 1.0f);
        }
    }

    [Test]
    public void Next_WithZeroCurve_FallsBackToUniform()
    {
        using var curve = new Curve();
        curve.AddPoint(new Vector2(0, 0));
        curve.AddPoint(new Vector2(1, 0));
        var sampler = new AutoFireSampler(curve, 100, 20, new Random(2));
        for (int i = 0; i < 100; i++)
        {
            sampler.Next().ShouldBeInRange(0.0f, 1.0f);
        }
    }

    [Test]
    public void Next_WithCurveWeightedToHighEnd_OnlySamplesWeightedRegion()
    {
        // Zero weight below 0.5, full weight above.
        using var curve = new Curve();
        curve.AddPoint(new Vector2(0.0f, 0.0f));
        curve.AddPoint(new Vector2(0.5f, 0.0f), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
        curve.AddPoint(new Vector2(0.51f, 1.0f), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
        curve.AddPoint(new Vector2(1.0f, 1.0f));
        var sampler = new AutoFireSampler(curve, 100, 20, new Random(3));
        for (int i = 0; i < 200; i++)
        {
            sampler.Next().ShouldBeGreaterThanOrEqualTo(0.49f);
        }
    }

    [Test]
    public void Next_IsDeterministicForSameSeed()
    {
        var a = new AutoFireSampler(null, 100, 5, new Random(42));
        var b = new AutoFireSampler(null, 100, 5, new Random(42));
        for (int i = 0; i < 12; i++)
        {
            a.Next().ShouldBe(b.Next());
        }
    }
}
