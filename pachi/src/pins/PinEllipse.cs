using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class PinEllipse : PinGenerator
{

    [Export]
    public float RadiusX
    {
        get => _radiusX;
        set
        {
            float clamped = Math.Max(0.1f, value);
            if (Mathf.IsEqualApprox(_radiusX, clamped)) return;
            _radiusX = clamped;
            Rebuild();
        }
    }

    [Export]
    public float RadiusY
    {
        get => _radiusY;
        set
        {
            float clamped = Math.Max(0.1f, value);
            if (Mathf.IsEqualApprox(_radiusY, clamped)) return;
            _radiusY = clamped;
            Rebuild();
        }
    }

    [Export]
    public float AverageSpacing
    {
        get => _averageSpacing;
        set
        {
            float clamped = Math.Max(0.1f, value);
            if (Mathf.IsEqualApprox(_averageSpacing, clamped)) return;
            _averageSpacing = clamped;
            Rebuild();
        }
    }

    [Export(PropertyHint.Range, "-360,360")]
    public float StartAngle
    {
        get => _startAngle;
        set
        {
            if (Mathf.IsEqualApprox(_startAngle, value)) return;
            _startAngle = value;
            Rebuild();
        }
    }

    [Export(PropertyHint.Range, "-360,360")]
    public float EndAngle
    {
        get => _endAngle;
        set
        {
            if (Mathf.IsEqualApprox(_endAngle, value)) return;
            _endAngle = value;
            Rebuild();
        }
    }

    [Export]
    public bool MirrorX
    {
        get => _mirrorX;
        set
        {
            if (_mirrorX == value) return;
            _mirrorX = value;
            Rebuild();
        }
    }

    [Export]
    public bool MirrorY
    {
        get => _mirrorY;
        set
        {
            if (_mirrorY == value) return;
            _mirrorY = value;
            Rebuild();
        }
    }

    public void Configure(float radiusX, float radiusY, float startAngle, float endAngle, float averageSpacing, bool mirrorX, bool mirrorY)
    {
        float clampedRadiusX = Math.Max(0.1f, radiusX);
        float clampedRadiusY = Math.Max(0.1f, radiusY);
        float clampedSpacing = Math.Max(0.1f, averageSpacing);

        bool changed = !Mathf.IsEqualApprox(_radiusX, clampedRadiusX) ||
                       !Mathf.IsEqualApprox(_radiusY, clampedRadiusY) ||
                       !Mathf.IsEqualApprox(_startAngle, startAngle) ||
                       !Mathf.IsEqualApprox(_endAngle, endAngle) ||
                       !Mathf.IsEqualApprox(_averageSpacing, clampedSpacing) ||
                       _mirrorX != mirrorX ||
                       _mirrorY != mirrorY;

        _radiusX = clampedRadiusX;
        _radiusY = clampedRadiusY;
        _startAngle = startAngle;
        _endAngle = endAngle;
        _averageSpacing = clampedSpacing;
        _mirrorX = mirrorX;
        _mirrorY = mirrorY;

        if (changed)
        {
            Rebuild();
        }
    }

    private float _radiusX = 100.0f;
    private float _radiusY = 80.0f;
    private float _averageSpacing = 16.0f;
    private float _startAngle = 0.0f;
    private float _endAngle = 180.0f; // Defaulting to a half-pipe shape
    private bool _mirrorX = false;
    private bool _mirrorY = false;

    protected override void GeneratePins()
    {
        // Convert degrees to radians for math functions
        float startRad = Mathf.DegToRad(Mathf.Min(_startAngle, _endAngle));
        float endRad = Mathf.DegToRad(Mathf.Max(_startAngle, _endAngle));
        endRad = Mathf.Min(endRad, startRad + Mathf.Tau);
        Debug.Assert(startRad <= endRad);
        float angleDifference = endRad - startRad;

        if (Mathf.IsEqualApprox(angleDifference, 0)) return;

        // approximate full ellipse circumference and calculate arc length
        float a = _radiusX;
        float b = _radiusY;
        float circumference = Mathf.Pi * (3 * (a + b) - Mathf.Sqrt((3 * a + b) * (a + 3 * b)));
        float arcFraction = angleDifference / Mathf.Tau;
        float arcLength = circumference * arcFraction;

        // calculate pin count based on spacing and arc length
        int pinCount = Mathf.RoundToInt(arcLength / _averageSpacing);
        if (pinCount <= 0) return;

        // step evenly by angle
        bool isFullCircle = Mathf.IsEqualApprox(angleDifference, Mathf.Tau);
        float step = isFullCircle ?
            angleDifference / pinCount :
            angleDifference / Mathf.Max(1, pinCount - 1);

        for (int i = 0; i < pinCount; i++)
        {
            float prevAngle = startRad + (step * (i - 1));
            float angle = startRad + (step * i);
            float nextAngle = startRad + (step * (i + 1));

            Vector2 prevPosition = new(
                    _radiusX * Mathf.Cos(prevAngle) * (MirrorX ? -1.0f : 1.0f),
                    _radiusY * Mathf.Sin(prevAngle) * (MirrorY ? -1.0f : 1.0f)
            );
            Vector2 position = new(
                    _radiusX * Mathf.Cos(angle) * (MirrorX ? -1.0f : 1.0f),
                    _radiusY * Mathf.Sin(angle) * (MirrorY ? -1.0f : 1.0f)
            );
            Vector2 nextPosition = new(
                    _radiusX * Mathf.Cos(nextAngle) * (MirrorX ? -1.0f : 1.0f),
                    _radiusY * Mathf.Sin(nextAngle) * (MirrorY ? -1.0f : 1.0f)
            );

            float pinRotation = prevPosition.AngleToPoint(nextPosition);

            SpawnPin(position, pinRotation);
        }



    }
}
