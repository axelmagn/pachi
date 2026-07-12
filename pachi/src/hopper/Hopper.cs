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
    private int _nextDispenseHoleIndex = 0;

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

        QueuedBallDispenseTimer.Timeout += OnDispenseTimeout;
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

    public void DispenseBall(Ball ball)
    {
        ball.Freeze = true;
        if (ball.GetParent() != null)
        {
            ball.Reparent(BallsRoot);
        }
        else
        {
            BallsRoot.AddChild(ball);
        }

        int numHoles = QueuedBallDispenseHoles.Count();
        Debug.Assert(_nextDispenseHoleIndex < numHoles);
        Hole hole = QueuedBallDispenseHoles[_nextDispenseHoleIndex];
        ball.GlobalPosition = hole.GlobalPosition;
        _nextDispenseHoleIndex += 1;
        _nextDispenseHoleIndex %= numHoles;
        ball.FadeIn(initFadedOut: true);
    }

    private void OnDispenseTimeout() {
        if (_queuedBalls.Count() == 0) return;
        Ball ball = _queuedBalls.First();
        _queuedBalls.RemoveFirst();
        DispenseBall(ball);
    }

}
