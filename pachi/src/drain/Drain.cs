using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class Drain : Node2D
{
    [Signal]
    public delegate void BallConsumedEventHandler(Ball ball);

    [Export]
    public Hole Hole { get; set; }

    [Export]
    public float AttractorImpulse { get; set; }

    [Export]
    public Area2D AttractorArea { get; set; }

    [Export]
    public CollisionShape2D AttractorAreaShape { get; set; }

    [Export]
    public Curve AttractorImpulseByDistance { get; set; }


    public override void _Ready()
    {
        Debug.Assert(AttractorArea != null);
        Debug.Assert(AttractorAreaShape != null);
        Debug.Assert(AttractorAreaShape.Shape is CircleShape2D);
        Debug.Assert(AttractorImpulseByDistance != null);
        Debug.Assert(Hole != null);
    }

    public override void _PhysicsProcess(double delta)
    {
        Debug.Assert(Hole != null);
        float holeRadius = Hole.GetRadius();

        // apply attractor to closest ball
        Ball closestBall = null;
        float closestBallDistance = 0.0f;
        foreach (Node body in AttractorArea.GetOverlappingBodies())
        {
            if (body is Ball ball)
            {
                float distance = (ball.GlobalPosition - Hole.GlobalPosition).Length();
                if (closestBall == null || distance < closestBallDistance)
                {
                    closestBall = ball;
                    closestBallDistance = distance;
                }
            }
        }
        if (closestBall != null)
        {
            Debug.Assert(AttractorAreaShape != null);
            Debug.Assert(AttractorAreaShape.Shape is CircleShape2D);
            float attractorRadius = ((CircleShape2D)AttractorAreaShape.Shape).Radius;
            float impulseMagnitude = AttractorImpulse * AttractorImpulseByDistance.SampleBaked(closestBallDistance / attractorRadius);
            Vector2 impulseDirection = (Hole.GlobalPosition - closestBall.GlobalPosition).Normalized();
            closestBall.ApplyCentralImpulse(impulseMagnitude * impulseDirection);
        }


        // drain any overlapping balls
        foreach (Node body in Hole.GetOverlappingBodies())
        {
            if (body is Ball ball)
            {
                if (ball.CurrentTransitionState != Ball.TransitionState.None) continue;

                float ballRadius = ball.GetRadius();
                if (ballRadius > holeRadius) continue;

                float distanceSq = (ball.GlobalPosition - Hole.GlobalPosition).LengthSquared();
                float maxDrainDistanceSq = (holeRadius - ballRadius) * (holeRadius - ballRadius);
                if (distanceSq <= maxDrainDistanceSq)
                {
                    ball.FadeOut(Hole.GlobalPosition);
                    ball.FadeOutFinished += ball.QueueFree;
                }
            }
        }
    }
}
