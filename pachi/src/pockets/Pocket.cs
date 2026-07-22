using Godot;
using System;
using System.Diagnostics;

public partial class Pocket : Node2D
{
    [Export]
    public Hole CatchHole { get; set; }

    [Export]
    public Hole RejectHole { get; set; }

    [Export]
    public CharacterBody2D LeftArm { get; set; }

    [Export]
    public CharacterBody2D RightArm { get; set; }

    public override void _Ready()
    {
        Debug.Assert(CatchHole != null);
        Debug.Assert(RejectHole != null);
        Debug.Assert(LeftArm != null);
        Debug.Assert(RightArm != null);

        CatchHole.BallOverlapped += OnBallCatch;
    }

    private void OnBallCatch(Ball ball)
    {
        ball.FadeOut(CatchHole.GlobalPosition);
        ball.Connect(Ball.SignalName.FadeOutFinished, Callable.From(() =>
        {
            ball.QueueFree();
            GD.Print("ball caught by pocket");
        }), (uint)GodotObject.ConnectFlags.OneShot);
    }
}
