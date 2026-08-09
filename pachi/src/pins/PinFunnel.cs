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
        set { _innerWidth = Math.Max(0.1f, value); Rebuild(); }
    }

    [Export]
    public float OuterWidth
    {
        get => _outerWidth;
        set { _outerWidth = Math.Max(0.1f, value); Rebuild(); }
    }

    [Export]
    public float Height
    {
        get => _height;
        set { _height = Math.Max(0.1f, value); Rebuild(); }
    }

    [Export]
    public float AverageSpacing
    {
        get => _averageSpacing;
        set { _averageSpacing = Math.Max(0.1f, value); Rebuild(); }
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

    [Export]
    public PinEllipse LeftEllipse
    {
        get => _leftEllipse;
        set { _leftEllipse = value; Rebuild(); }
    }
    [Export]
    public PinEllipse RightEllipse
    {
        get => _rightEllipse;
        set { _rightEllipse = value; Rebuild(); }
    }

    private float _innerWidth = 50.0f;
    private float _outerWidth = 100.0f;
    private float _height = 100.0f;
    private float _averageSpacing = 16.0f;
    private bool _mirrorX = false;
    private bool _mirrorY = false;

    private PinEllipse _leftEllipse;
    private PinEllipse _rightEllipse;

    private void Rebuild()
    {
        float x = (MirrorX ? InnerWidth : OuterWidth) / 2;
        float y = MirrorY ? -Height : 0;
        if (LeftEllipse != null)
        {
            LeftEllipse.Position = new(-x, y);
            LeftEllipse.StartAngle = -90;
            LeftEllipse.EndAngle = 0;
            LeftEllipse.RadiusX = Math.Max(0, (OuterWidth - InnerWidth) / 2);
            LeftEllipse.RadiusY = Height;
            LeftEllipse.MirrorX = MirrorX;
            LeftEllipse.MirrorY = MirrorY;
            LeftEllipse.AverageSpacing = AverageSpacing;
        }
        if (RightEllipse != null)
        {
            RightEllipse.Position = new(x, y);
            RightEllipse.StartAngle = -180;
            RightEllipse.EndAngle = -90;
            RightEllipse.RadiusX = Math.Max(0, (OuterWidth - InnerWidth) / 2);
            RightEllipse.RadiusY = Height;
            RightEllipse.MirrorX = MirrorX;
            RightEllipse.MirrorY = MirrorY;
            RightEllipse.AverageSpacing = AverageSpacing;
        }
    }
}
