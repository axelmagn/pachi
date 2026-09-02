using Godot;
using System;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class Hole : Area2D
{
    [Signal]
    public delegate void BallOverlappedEventHandler(Ball ball);

    [Export]
    public CollisionShape2D? Collider { get; set; }

    [Export]
    public bool MonitorBallOverlap { get; set; } = false;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        Debug.Assert(Collider != null);

        // we only need to tick physics if we are monitoring for ball overlaps
        SetPhysicsProcess(MonitorBallOverlap);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;
        CheckBallOverlaps();
    }

    public float GetRadius()
    {
        Debug.Assert(Collider?.Shape is CircleShape2D);
        CircleShape2D circle = (CircleShape2D)Collider!.Shape;
        Debug.Assert(Mathf.IsEqualApprox(Scale.X, Scale.Y));
        return circle.Radius * Scale.X;
    }

    private void CheckBallOverlaps()
    {
        float radius = GetRadius();
        foreach (Node body in GetOverlappingBodies())
        {
            // ASSERT: hole collision is already set up to only detect balls
            Debug.Assert(body is Ball);
            Ball ball = (Ball)body;
            if (ball.CurrentTransitionState != Ball.TransitionState.None) continue;

            float ballRadius = ball.GetRadius();
            if (ballRadius > radius) continue;

            float distanceSq = (ball.GlobalPosition - GlobalPosition).LengthSquared();
            float maxDistance = radius - ballRadius;
            if (distanceSq <= maxDistance * maxDistance)
            {
                EmitSignal(SignalName.BallOverlapped, ball);
            }
        }

    }
}
