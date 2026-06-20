using System.Diagnostics;
using Godot;

public partial class BallSource : Marker2D
{
    [Export]
    public Vector2 LaunchVelocity { get; set; }

    [Export]
    public Vector2 LaunchJitter { get; set; } = new Vector2(10, 10);

    public void LaunchExistingBall(Ball ball, float strength)
    {
        Debug.Assert(ball != null);
        Debug.Assert(ball.Tier != null);

        Node parent = GetParent();
        if (parent == null) return;

        Vector2 position = Position;
        PhysicsInterpolationModeEnum priorInterpolationMode = ball.PhysicsInterpolationMode;

        float jitterX = (float)GD.RandRange(-LaunchJitter.X, LaunchJitter.X);
        float jitterY = (float)GD.RandRange(-LaunchJitter.Y, LaunchJitter.Y);
        Vector2 jitter = new Vector2(jitterX, jitterY);

        ball.PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;

        if (ball.GetParent() != null)
        {
            ball.Reparent(parent);
        }
        else
        {
            parent.AddChild(ball);
        }

        ball.Position = position;
        ball.LinearVelocity = LaunchVelocity * strength + jitter;
        ball.PhysicsInterpolationMode = priorInterpolationMode;
    }
}
