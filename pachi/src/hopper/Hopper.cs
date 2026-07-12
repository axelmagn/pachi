using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Hopper : Node2D
{
    [Export]
    public Node2D BallsRoot { get; set; }

    [Export]
    public int InitQueuedBalls { get; set; } = 0;

    [Export]
    public PackedScene InitQueuedBallsScene { get; set; }

    [Export]
    public Timer QueuedBallDispenseTimer { get; set; }

    [Export]
    public Array<Hole> QueuedBallDispenseHoles { get; set; }

    private readonly LinkedList<Ball> _containedBalls = new();
    private readonly LinkedList<Ball> _queuedBalls = new();

    public override void _Ready()
    {
        Debug.Assert(BallsRoot != null);
        Debug.Assert(QueuedBallDispenseTimer != null);
        Debug.Assert(QueuedBallDispenseHoles != null);
        Debug.Assert(QueuedBallDispenseHoles.Count > 0);

        foreach (Node child in BallsRoot.GetChildren())
        {
            if (child is Ball ball)
            {
                _containedBalls.AddLast(ball);
            }
        }

        if (InitQueuedBalls > 0)
        {
            Debug.Assert(InitQueuedBallsScene != null);
            for (int i = 0; i < InitQueuedBalls; i++)
            {
                Ball ball = InitQueuedBallsScene.Instantiate<Ball>();
                _queuedBalls.AddLast(ball);
            }
        }
    }

    public int BallCount()
    {
        return _containedBalls.Count();
    }

    public Ball PopFirstContainedBall()
    {
        if (_containedBalls.Count() == 0)
        {
            return null;
        }
        Ball first = _containedBalls.First();
        _containedBalls.RemoveFirst();
        return first;
    }

}
