using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Hopper : Node2D
{
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
        Debug.Assert(BallsRoot != null);
        Debug.Assert(QueuedBallDispenseTimer != null);
        Debug.Assert(QueuedBallDispenseHoles != null);
        Debug.Assert(QueuedBallDispenseHoles.Count > 0);

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
            Debug.Assert(InitQueuedBallsScene != null);
            Random random = new();
            for (int i = 0; i < InitQueuedBalls; i++)
            {
                Ball ball = InitQueuedBallsScene!.Instantiate<Ball>();
                _queuedBalls.AddLast(ball);
            }
        }

        QueuedBallDispenseTimer!.Timeout += OnDispenseTimeout;
        Debug.Assert(GlobalEvents.Instance != null, "GlobalEvents.Instance must not be null");
        GlobalEvents.Instance.BallAwarded += OnBallAwarded;

        AddToGroup("hoppers");
    }

    public int GetTotalBallCount()
    {
        return _containedBalls.Count + _queuedBalls.Count;
    }

    public override void _ExitTree()
    {
        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.BallAwarded -= OnBallAwarded;
        }
    }

    public void AddQueuedBalls(IEnumerable<BallVariant> variants)
    {
        if (variants == null) return;
        Debug.Assert(GameConfig.Instance != null, "GameConfig.Instance must not be null");
        Debug.Assert(GameConfig.Instance.BallScene != null, "GameConfig.Instance.BallScene must not be null");

        foreach (BallVariant variant in variants)
        {
            if (variant == null) continue;
            Ball ball = GameConfig.Instance.BallScene.Instantiate<Ball>();
            ball.Variant = variant;
            _queuedBalls.AddLast(ball);
        }
    }

    public int BallCount()
    {
        return _containedBalls.Count;
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

    private void OnBallAwarded(BallVariant variant)
    {
        Debug.Assert(GameConfig.Instance != null, "GameConfig.Instance must not be null");
        Debug.Assert(GameConfig.Instance.BallScene != null, "GameConfig.Instance.BallScene must not be null");

        Ball ball = GameConfig.Instance.BallScene.Instantiate<Ball>();
        ball.Variant = variant;
        _queuedBalls.AddLast(ball);
    }
}
