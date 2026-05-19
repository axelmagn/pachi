using Godot;

public partial class BallSink : Marker2D
{
    [Signal]
    public delegate void BallSunkEventHandler(Ball ball);

    [Export]
    public Area2D Area { get; set; }

    public override void _Ready()
    {
        if (Area == null)
        {
            GD.PushError("BallSink: Area is not assigned!");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Area == null) return;

        foreach (Node body in Area.GetOverlappingBodies())
        {
            if (body is Ball ball)
            {
                // TODO: read the correct fields, don't just hardcode
                float sinkRadius = 16.0f;
                float ballRadius = 10.0f;
                float distance = (GlobalPosition - ball.GlobalPosition).Length();

                if (distance <= sinkRadius - ballRadius)
                {
                    EmitSignal(SignalName.BallSunk, ball);
                    ball.QueueFree();
                }
            }
        }
    }
}
