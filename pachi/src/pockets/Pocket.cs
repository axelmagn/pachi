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

    public Array<bool> InputBallSlotAvailable;

    [Export]
    public Array<BallVariant> OutputBalls;

    [Export]
    public PocketBallsIndicator InputsIndicator;

    [Export]
    public PocketBallsIndicator OutputsIndicator;

    [Export]
    public bool RandomizeInputBalls = false;

    [Export]
    public bool RandomizeOutputBalls = false;


    public override void _Ready()
    {
        Debug.Assert(CatchHole != null);
        Debug.Assert(RejectHole != null);
        Debug.Assert(LeftArm != null);
        Debug.Assert(RightArm != null);
        Debug.Assert(InputsIndicator != null);
        Debug.Assert(OutputsIndicator != null);

        InputBalls = InputBalls == null ? [] : InputBalls.Duplicate();
        OutputBalls = OutputBalls == null ? [] : OutputBalls.Duplicate();

        var tiers = GameConfig.Instance.BallTiers;
        var random = GameConfig.Instance.Rng;
        if (RandomizeInputBalls && InputBalls != null && tiers != null)
        {
            for (int i = 0; i < InputBalls.Count; i++)
            {
                var idx = random.Next(0, tiers.Count);
                InputBalls[i] = tiers[idx];
            }
        }
        if (RandomizeOutputBalls && OutputBalls != null && tiers != null)
        {
            for (int i = 0; i < OutputBalls.Count; i++)
            {
                var idx = random.Next(0, tiers.Count);
                OutputBalls[i] = tiers[idx];
            }
        }

        CatchHole.BallOverlapped += OnBallCatch;
        InputsIndicator.Balls = InputBalls;
        OutputsIndicator.Balls = OutputBalls;

        // initialize held balls tracker
        InputBallSlotAvailable = [];
        if (InputBalls != null)
        {
            for (int i = 0; i < InputBalls.Count; i++)
            {
                InputBallSlotAvailable.Add(true);
            }
            Debug.Assert(InputBalls.Count == InputBallSlotAvailable.Count);
        }
    }

    private void OnBallCatch(Ball ball)
    {

        // accumulate ball
        bool reject = true;
        Debug.Assert(InputBalls.Count == InputBallSlotAvailable.Count);
        for (int i = 0; i < InputBalls.Count; i++)
        {
            ball.FadeOut(CatchHole.GlobalPosition);
            if (InputBalls[i] == ball.Variant && InputBallSlotAvailable[i])
            {
                InputBallSlotAvailable[i] = false;
                reject = false;
                ball.Connect(Ball.SignalName.FadeOutFinished, Callable.From(ball.QueueFree),
                        (uint)ConnectFlags.OneShot);
                break;
            }
        }

        // reject ball
        if (reject)
        {
            ball.Connect(Ball.SignalName.FadeOutFinished,
                    Callable.From(() => { ball.FadeIn(RejectHole.GlobalPosition, true); }),
                    (uint)ConnectFlags.OneShot);
        }

        // emit reward signal if accumulated
        bool accumulationFilled = true;
        foreach (bool available in InputBallSlotAvailable)
        {
            if (available)
            {
                accumulationFilled = false;
                break;
            }
        }

        if (accumulationFilled)
        {
            // pay out rewards
            foreach (BallVariant variant in OutputBalls)
            {
                GlobalEvents.Instance.NotifyBallAwarded(variant);
            }

            // reset input ball slots
            for (int i = 0; i < InputBallSlotAvailable.Count; i++)
            {
                InputBallSlotAvailable[i] = true;
            }
        }


    }
}
