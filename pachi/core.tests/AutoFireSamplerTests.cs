using System;
using Pachi.Core;
using Shouldly;
using Xunit;

namespace Pachi.Core.Tests;

public class AutoFireSamplerTests
{
    [Fact]
    public void Next_WithoutWeight_ReturnsValuesInUnitRange()
    {
        var sampler = new AutoFireSampler(null, 100, 20, new Random(1));
        for (int i = 0; i < 200; i++)
        {
            sampler.Next().ShouldBeInRange(0.0f, 1.0f);
        }
    }

    [Fact]
    public void Next_WithZeroWeight_FallsBackToUniform()
    {
        var sampler = new AutoFireSampler(_ => 0.0f, 100, 20, new Random(2));
        for (int i = 0; i < 100; i++)
        {
            sampler.Next().ShouldBeInRange(0.0f, 1.0f);
        }
    }

    [Fact]
    public void Next_WithWeightOnHighEnd_OnlySamplesWeightedRegion()
    {
        // Zero weight below 0.5, ramping to full weight by 0.51.
        static float Step(float t) => t <= 0.5f ? 0.0f : Math.Min(1.0f, (t - 0.5f) / 0.01f);
        var sampler = new AutoFireSampler(Step, 100, 20, new Random(3));
        for (int i = 0; i < 200; i++)
        {
            sampler.Next().ShouldBeGreaterThanOrEqualTo(0.49f);
        }
    }

    [Fact]
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
