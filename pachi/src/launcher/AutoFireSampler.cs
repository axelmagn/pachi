using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Draws auto-fire charge ratios in [0, 1], distributed according to a weight curve.
/// Samples are drawn in bags of a fixed size; the curve's CDF is computed once up front.
/// </summary>
public sealed class AutoFireSampler
{
    private const int MinResolution = 16;

    private readonly Random _rng;
    private readonly int _bagSize;
    private readonly float[]? _cdf;
    private readonly float _totalArea;
    private readonly List<float> _bag = [];
    private int _bagIndex;

    public AutoFireSampler(Curve? weightCurve, int resolution, int bagSize, Random rng)
    {
        _rng = rng;
        _bagSize = Math.Max(1, bagSize);
        if (weightCurve != null)
        {
            (_cdf, _totalArea) = BuildCdf(weightCurve, Math.Max(MinResolution, resolution));
        }
    }

    /// Returns the next ratio from the current bag, refilling it when exhausted.
    public float Next()
    {
        if (_bagIndex >= _bag.Count)
        {
            RefillBag();
        }
        return _bag[_bagIndex++];
    }

    private void RefillBag()
    {
        _bag.Clear();
        for (int i = 0; i < _bagSize; i++)
        {
            _bag.Add(Sample());
        }
        _bagIndex = 0;
    }

    private float Sample()
    {
        if (_cdf == null || _totalArea <= 0.00001f)
        {
            return (float)_rng.NextDouble();
        }

        int segments = _cdf.Length;
        float target = (float)_rng.NextDouble() * _totalArea;
        int index = Array.BinarySearch(_cdf, target);
        if (index < 0)
        {
            index = ~index;
        }
        index = Math.Clamp(index, 0, segments - 1);

        float prevCdf = index > 0 ? _cdf[index - 1] : 0.0f;
        float nextCdf = _cdf[index];
        float fraction = nextCdf > prevCdf ? (target - prevCdf) / (nextCdf - prevCdf) : 0.0f;

        float t0 = (float)index / segments;
        float t1 = (float)(index + 1) / segments;
        return Math.Clamp(Mathf.Lerp(t0, t1, fraction), 0.0f, 1.0f);
    }

    /// Trapezoidal cumulative area under the (non-negative) curve over [0, 1].
    private static (float[] Cdf, float TotalArea) BuildCdf(Curve curve, int segments)
    {
        float[] cdf = new float[segments];
        float total = 0.0f;
        float prevWeight = Math.Max(0.0f, curve.Sample(0.0f));
        for (int k = 0; k < segments; k++)
        {
            float nextWeight = Math.Max(0.0f, curve.Sample((float)(k + 1) / segments));
            total += 0.5f * (prevWeight + nextWeight);
            cdf[k] = total;
            prevWeight = nextWeight;
        }
        return (cdf, total);
    }
}
