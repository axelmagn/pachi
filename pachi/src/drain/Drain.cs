using Godot;
using System;
using System.Diagnostics;

public partial class Drain : Node2D
{
    [Signal]
    public delegate void BallConsumedEventHandler(Ball ball);

    [Export]
    public Hole Hole { get; set; }

    public override void _Ready()
    {
        Debug.Assert(Hole != null);
    }

    public override void _PhysicsProcess(double delta)
    {
        Debug.Assert(Hole != null);
        float holeRadius = Hole.GetRadius();
        foreach (Node body in Hole.GetOverlappingBodies())
        {
            if (body is Ball ball)
            {
                if (ball.CurrentFadeState != Ball.FadeState.None) continue;

                float ballRadius = ball.GetRadius();
                if (ballRadius > holeRadius) continue;

                float distanceSq = (ball.GlobalPosition - Hole.GlobalPosition).LengthSquared();
                float maxDrainDistanceSq = (holeRadius - ballRadius) * (holeRadius - ballRadius);
                if (distanceSq <= maxDrainDistanceSq)
                {
                    GD.Print("Drain Triggered"); // DEBUG
                    ball.FadeOut(Hole.GlobalPosition);
                    ball.FadeOutFinished += ball.QueueFree;
                }
            }
        }
    }
}
