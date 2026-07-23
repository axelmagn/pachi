using Godot;
using Godot.Collections;
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

    [Export]
    public Array<BallVariant> InputBalls;

    public Array<bool> InputBallsHeld;

    [Export]
    public Array<BallVariant> OutputBalls;

    [Export]
    public PocketBallsIndicator InputsIndicator;

    [Export]
    public PocketBallsIndicator OutputsIndicator;

    public override void _Ready()
    {
        Debug.Assert(CatchHole != null);
        Debug.Assert(RejectHole != null);
        Debug.Assert(LeftArm != null);
        Debug.Assert(RightArm != null);
        Debug.Assert(InputsIndicator != null);
        Debug.Assert(OutputsIndicator != null);

        CatchHole.BallOverlapped += OnBallCatch;
        InputsIndicator.Balls = InputBalls;
        OutputsIndicator.Balls = OutputBalls;

        // initialize held balls tracker
        InputBallsHeld = [];
        for(int i = 0; i < InputBalls.Count; i++) {
            InputBallsHeld.Add(false);

        }
    }

    private void OnBallCatch(Ball ball)
    {

        // TODO: accumulate or reject ball
        // TODO: emit reward signal if accumulated
        
        // TODO: only delete ball if it is being accumulated
        ball.FadeOut(CatchHole.GlobalPosition);
        ball.Connect(Ball.SignalName.FadeOutFinished,
                Callable.From(ball.QueueFree),
                (uint)GodotObject.ConnectFlags.OneShot);


    }
}
