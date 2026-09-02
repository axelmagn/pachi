using Godot;
using System.Diagnostics;

[GlobalClass]
public partial class Drain : Node2D
{
    [Signal]
    public delegate void BallConsumedEventHandler(Ball ball);

    [Export]
    public Hole? Hole { get; set; }

    public override void _Ready()
    {
        Debug.Assert(Hole != null);

        Hole!.BallOverlapped += OnBallOverlap;
    }

    public override void _ExitTree()
    {
        if (Hole != null)
        {
            Hole.BallOverlapped -= OnBallOverlap;
        }
    }

    private void OnBallOverlap(Ball ball)
    {
        ball.FadeOut(Hole!.GlobalPosition);
        ball.Connect(Ball.SignalName.FadeOutFinished, Callable.From(ball.QueueFree), (uint)GodotObject.ConnectFlags.OneShot);
    }
}
