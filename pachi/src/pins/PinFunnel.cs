using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class PinFunnel : Node2D
{
    // TODO: exports and updates
    [Export]
    public float InnerWidth
    {
        get => _innerWidth;
        set
        {
            float clamped = Math.Max(0.1f, value);
            if (Mathf.IsEqualApprox(_innerWidth, clamped)) return;
            _innerWidth = clamped;
            Rebuild();
        }
    }

    [Export]
    public float OuterWidth
    {
        get => _outerWidth;
        set
        {
            float clamped = Math.Max(0.1f, value);
            if (Mathf.IsEqualApprox(_outerWidth, clamped)) return;
            _outerWidth = clamped;
            Rebuild();
        }
    }

    [Export]
    public float Height
    {
        get => _height;
        set
        {
            float clamped = Math.Max(0.1f, value);
            if (Mathf.IsEqualApprox(_height, clamped)) return;
            _height = clamped;
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

    [Export]
    public PinEllipse? LeftEllipse
    {
        get => _leftEllipse;
        set
        {
            if (_leftEllipse == value) return;
            _leftEllipse = value;
            Rebuild();
        }
    }
    [Export]
    public PinEllipse? RightEllipse
    {
        get => _rightEllipse;
        set
        {
            if (_rightEllipse == value) return;
            _rightEllipse = value;
            Rebuild();
        }
    }

    private float _innerWidth = 50.0f;
    private float _outerWidth = 100.0f;
    private float _height = 100.0f;
    private float _averageSpacing = 16.0f;
    private bool _mirrorX = false;
    private bool _mirrorY = false;

    private PinEllipse? _leftEllipse;
    private PinEllipse? _rightEllipse;

    private void Rebuild()
    {
        float x = (MirrorX ? InnerWidth : OuterWidth) / 2;
        float y = MirrorY ? -Height : 0;
        float radiusX = Math.Max(0, (OuterWidth - InnerWidth) / 2);
        if (LeftEllipse != null)
        {
            LeftEllipse.Position = new(-x, y);
            LeftEllipse.Configure(
                radiusX: radiusX,
                radiusY: Height,
                startAngle: -90,
                endAngle: 0,
                averageSpacing: AverageSpacing,
                mirrorX: MirrorX,
                mirrorY: MirrorY
            );
        }
        if (RightEllipse != null)
        {
            RightEllipse.Position = new(x, y);
            RightEllipse.Configure(
                radiusX: radiusX,
                radiusY: Height,
                startAngle: -180,
                endAngle: -90,
                averageSpacing: AverageSpacing,
                mirrorX: MirrorX,
                mirrorY: MirrorY
            );
        }
    }
}
