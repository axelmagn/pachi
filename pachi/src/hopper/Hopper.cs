using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[GlobalClass]
public partial class Hopper : Node2D
{
    public static readonly StringName GroupHoppers = new("hoppers");

    [Export]
    public Node2D? BallsRoot { get; set; }

    [Export]
    public int InitQueuedBalls { get; set; } = 0;

    [Export]
    public PackedScene? InitQueuedBallsScene { get; set; }

    [Export]
    public Timer? QueuedBallDispenseTimer { get; set; }

    [Export]
    public Array<Hole>? QueuedBallDispenseHoles { get; set; }

    private readonly LinkedList<Ball> _containedBalls = new();
    private readonly LinkedList<Ball> _queuedBalls = new();
    private int _nextDispenseHoleIndex = 0;

    public override void _Ready()
    {
        this.Require(BallsRoot, nameof(BallsRoot));
        this.Require(QueuedBallDispenseTimer, nameof(QueuedBallDispenseTimer));
        this.Require(QueuedBallDispenseHoles, nameof(QueuedBallDispenseHoles));
        Debug.Assert(QueuedBallDispenseHoles!.Count > 0);

        foreach (Node child in BallsRoot!.GetChildren())
        {
            if (child is Ball ball)
            {
                ball.IsInPlay = false;
                _containedBalls.AddLast(ball);
            }
        }

        if (InitQueuedBalls > 0)
        {
            this.Require(InitQueuedBallsScene, nameof(InitQueuedBallsScene));
            for (int i = 0; i < InitQueuedBalls; i++)
            {
                Ball ball = InitQueuedBallsScene!.Instantiate<Ball>();
                _queuedBalls.AddLast(ball);
            }
        }

        QueuedBallDispenseTimer!.Timeout += OnDispenseTimeout;
        Debug.Assert(GlobalEvents.Instance != null, "GlobalEvents.Instance must not be null");
        GlobalEvents.Instance.BallAwarded += OnBallAwarded;

        AddToGroup(GroupHoppers);
    }

    public override void _ExitTree()
    {
        if (QueuedBallDispenseTimer != null)
        {
            QueuedBallDispenseTimer.Timeout -= OnDispenseTimeout;
        }

        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.BallAwarded -= OnBallAwarded;
        }
    }

    public Ball? PopFirstContainedBall()
    {
        if (_containedBalls.Count == 0)
        {
            return null;
        }
        Ball first = _containedBalls.First();
        _containedBalls.RemoveFirst();
        return first;
    }

    public void DispenseBall(Ball ball)
    {
        ball.IsInPlay = false;
        ball.Freeze = true;
        if (ball.GetParent() != null)
        {
            ball.Reparent(BallsRoot!);
        }
        else
        {
            BallsRoot!.AddChild(ball);
        }
        _containedBalls.AddLast(ball);

        int numHoles = QueuedBallDispenseHoles!.Count;
        Debug.Assert(_nextDispenseHoleIndex < numHoles);
        Hole hole = QueuedBallDispenseHoles[_nextDispenseHoleIndex];
        ball.GlobalPosition = hole.GlobalPosition;
        _nextDispenseHoleIndex += 1;
        _nextDispenseHoleIndex %= numHoles;
        ball.FadeIn(initFadedOut: true);
    }

    private void OnDispenseTimeout()
    {
        if (_queuedBalls.Count == 0) return;
        Ball ball = _queuedBalls.First();
        _queuedBalls.RemoveFirst();
        DispenseBall(ball);
    }

    private void OnBallAwarded(BallVariant variant) => EnqueueBall(variant);

    private void EnqueueBall(BallVariant variant)
    {
        Debug.Assert(GameConfig.Instance?.BallScene != null, "GameConfig.Instance.BallScene must not be null");
        Ball ball = GameConfig.Instance!.BallScene!.Instantiate<Ball>();
        ball.Variant = variant;
        _queuedBalls.AddLast(ball);
    }
}
