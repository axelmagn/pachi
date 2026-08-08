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
        set { _radiusX = Math.Max(0.1f, value); Rebuild(); }
    }

    [Export]
    public float RadiusY
    {
        get => _radiusY;
        set { _radiusY = Math.Max(0.1f, value); Rebuild(); }
    }

    [Export]
    public float AverageSpacing
    {
        get => _averageSpacing;
        set { _averageSpacing = Math.Max(0.1f, value); Rebuild(); }
    }

    [Export(PropertyHint.Range, "-360,360")]
    public float StartAngle
    {
        get => _startAngle;
        set { _startAngle = value; Rebuild(); }
    }

    [Export(PropertyHint.Range, "-360,360")]
    public float EndAngle
    {
        get => _endAngle;
        set { _endAngle = value; Rebuild(); }
    }

    [Export]
    public bool MirrorX
    {
        get => _mirrorX;
        set { _mirrorX = value; Rebuild(); }
    }

    [Export]
    public bool MirrorY
    {
        get => _mirrorY;
        set { _mirrorY = value; Rebuild(); }
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
