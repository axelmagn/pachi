using Godot;
using System;
using System.Diagnostics;

public partial class Drain : Node2D
{
    [Signal]
    public delegate void BallConsumedEventHandler(Ball ball);

    [Export]
    public Hole Hole { get; set; }

    public override void _Ready() {
        Debug.Assert(Hole != null);
    }

    public override void _PhysicsProcess(double delta)
    {
        Debug.Assert(Hole != null);
        float holeRadius = Hole.GetRadius();
        foreach (Node body in Hole.GetOverlappingBodies()) {
            if (body is Ball ball) {
                if (ball == Hole.HeldBall) continue;
                float ballRadius = ball.GetRadius();
                float distance = (Hole.GlobalPosition - ball.GlobalPosition).Length();

                // if ball completely overlaps hole
                if (distance <= holeRadius - ballRadius) {
                    EmitSignal(SignalName.BallConsumed, ball);
                    Hole.AddOutgoingBall(ball);
                    // TODO: play audio
                }
            }
        }
    }
}
