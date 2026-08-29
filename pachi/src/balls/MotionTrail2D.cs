using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Dynamic motion trail component for balls using Line2D.
/// Tracks global movement positions and renders a smooth tapered and faded trail.
/// </summary>
[Tool]
[GlobalClass]
public partial class MotionTrail2D : Line2D
{
    [Export]
    public Ball? TargetBall { get; set; }

    [Export]
    public int MaxPoints { get; set; } = 20;

    [Export]
    public float MinDistance { get; set; } = 2.0f;

    [Export]
    public float MaxLifetime { get; set; } = 0.25f;

    [Export]
    public float MinSpeedThreshold { get; set; } = 15.0f;

    [Export]
    public float HeadAlpha { get; set; } = 0.7f;

    [Export]
    public float WidthScale { get; set; } = 1.0f;

    [Export]
    public bool AutoSyncColor { get; set; } = true;

    [Export]
    public bool AutoSyncRadius { get; set; } = true;

    private readonly Queue<(Vector2 Position, double Time)> _pointHistory = new();
    private Vector2 _lastAddedPosition = Vector2.Zero;
    private Color _lastSyncedColor = Colors.Transparent;

    public override void _Ready()
    {
        TopLevel = true;
        ShowBehindParent = true;
        GlobalPosition = Vector2.Zero;
        JointMode = LineJointMode.Round;
        EndCapMode = LineCapMode.Round;
        Antialiased = true;

        if (TargetBall == null && GetParent() is Ball ball)
        {
            TargetBall = ball;
        }

        InitDefaultCurve();

        if (TargetBall != null)
        {
            SyncWithBall();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;

        if (TargetBall == null)
        {
            if (GetParent() is Ball parentBall)
            {
                TargetBall = parentBall;
                SyncWithBall();
            }
            else
            {
                return;
            }
        }

        // Keep Line2D origin at world (0,0) when TopLevel is true
        GlobalPosition = Vector2.Zero;

        SyncWithBall();

        double currentTime = Time.GetTicksMsec() / 1000.0;
        Vector2 currentPos = TargetBall.GlobalPosition;
        float currentSpeed = TargetBall.LinearVelocity.Length();

        // Prune points exceeding max lifetime
        while (_pointHistory.Count > 0 && (currentTime - _pointHistory.Peek().Time) > MaxLifetime)
        {
            _pointHistory.Dequeue();
        }

        // Record new trail points when ball is moving
        if (currentSpeed >= MinSpeedThreshold || TargetBall.CurrentTransitionState != Ball.TransitionState.None)
        {
            if (_pointHistory.Count == 0)
            {
                _pointHistory.Enqueue((currentPos, currentTime));
                _lastAddedPosition = currentPos;
            }
            else if (currentPos.DistanceTo(_lastAddedPosition) >= MinDistance)
            {
                _pointHistory.Enqueue((currentPos, currentTime));
                _lastAddedPosition = currentPos;
            }
        }

        // Limit queue size to MaxPoints
        while (_pointHistory.Count > MaxPoints)
        {
            _pointHistory.Dequeue();
        }

        // Update Line2D points
        if (_pointHistory.Count < 2)
        {
            ClearPoints();
            return;
        }

        Vector2[] points = new Vector2[_pointHistory.Count];
        var historyArray = _pointHistory.ToArray();
        int idx = 0;
        // Head (current position) at index 0, tail (oldest position) at last index
        for (int i = historyArray.Length - 1; i >= 0; i--)
        {
            points[idx++] = historyArray[i].Position;
        }

        Points = points;

        // Modulate alpha during ball fade transitions
        Modulate = TargetBall.Modulate;
    }

    public void SyncWithBall()
    {
        if (TargetBall == null) return;

        if (AutoSyncRadius)
        {
            Width = TargetBall.GetRadius() * 2.0f * WidthScale;
        }

        if (AutoSyncColor)
        {
            Color baseColor = Colors.White;
            if (TargetBall.PlaceholderSprite != null)
            {
                baseColor = TargetBall.PlaceholderSprite.Color;
            }
            else if (TargetBall.Variant != null)
            {
                baseColor = TargetBall.Variant.PlaceholderColor;
            }

            if (baseColor != _lastSyncedColor || Gradient == null)
            {
                _lastSyncedColor = baseColor;
                Gradient gradient = new Gradient();
                gradient.SetColor(0, new Color(baseColor.R, baseColor.G, baseColor.B, HeadAlpha));
                gradient.SetColor(1, new Color(baseColor.R, baseColor.G, baseColor.B, 0.0f));
                Gradient = gradient;
            }
        }
    }

    private void InitDefaultCurve()
    {
        if (WidthCurve != null) return;

        Curve curve = new Curve();
        curve.AddPoint(new Vector2(0.0f, 1.0f)); // Head width = 100%
        curve.AddPoint(new Vector2(1.0f, 0.0f)); // Tail width = 0%
        WidthCurve = curve;
    }
}
