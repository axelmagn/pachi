using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Hopper : Node2D
{
    public static readonly StringName GroupHoppers = new("hoppers");

    [Signal]
    public delegate void InventoryChangedEventHandler();

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

        AddToGroup(GroupHoppers);
        EmitSignal(SignalName.InventoryChanged);
    }

    private void EnsureContainedBallsSynced()
    {
        if (BallsRoot != null && _containedBalls.Count == 0 && BallsRoot.GetChildCount() > 0)
        {
            foreach (Node child in BallsRoot.GetChildren())
            {
                if (child is Ball ball && !_containedBalls.Contains(ball))
                {
                    ball.IsInPlay = false;
                    _containedBalls.AddLast(ball);
                }
            }
        }
    }

    public int GetTotalBallCount()
    {
        EnsureContainedBallsSynced();
        return _containedBalls.Count + _queuedBalls.Count;
    }

    public int GetTierCount(int tier)
    {
        EnsureContainedBallsSynced();
        int count = 0;
        foreach (Ball ball in _containedBalls)
        {
            int ballTier = ball.Variant?.Tier ?? 1;
            if (ballTier == tier)
            {
                count++;
            }
        }
        foreach (Ball ball in _queuedBalls)
        {
            int ballTier = ball.Variant?.Tier ?? 1;
            if (ballTier == tier)
            {
                count++;
            }
        }
        return count;
    }

    public bool HasBallCost(int tier, int count)
    {
        if (count <= 0)
        {
            return true;
        }
        return GetTierCount(tier) >= count;
    }

    public bool DeductBallCost(int tier, int count)
    {
        if (count <= 0)
        {
            return true;
        }
        if (!HasBallCost(tier, count))
        {
            return false;
        }

        int remainingToDeduct = count;

        // Front-to-back scan of contained balls
        var currentContained = _containedBalls.First;
        while (currentContained != null && remainingToDeduct > 0)
        {
            var next = currentContained.Next;
            Ball ball = currentContained.Value;
            int ballTier = ball.Variant?.Tier ?? 1;
            if (ballTier == tier)
            {
                _containedBalls.Remove(currentContained);
                ball.QueueFree();
                remainingToDeduct--;
            }
            currentContained = next;
        }

        // Front-to-back scan of queued balls if still needed
        var currentQueued = _queuedBalls.First;
        while (currentQueued != null && remainingToDeduct > 0)
        {
            var next = currentQueued.Next;
            Ball ball = currentQueued.Value;
            int ballTier = ball.Variant?.Tier ?? 1;
            if (ballTier == tier)
            {
                _queuedBalls.Remove(currentQueued);
                ball.QueueFree();
                remainingToDeduct--;
            }
            currentQueued = next;
        }

        EmitSignal(SignalName.InventoryChanged);
        return true;
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

        bool added = false;
        foreach (BallVariant variant in variants)
        {
            if (variant == null) continue;
            Ball ball = GameConfig.Instance.BallScene.Instantiate<Ball>();
            ball.Variant = variant;
            _queuedBalls.AddLast(ball);
            added = true;
        }

        if (added)
        {
            EmitSignal(SignalName.InventoryChanged);
        }
    }

    public int BallCount()
    {
        EnsureContainedBallsSynced();
        return _containedBalls.Count;
    }

    public Ball? PopFirstContainedBall()
    {
        EnsureContainedBallsSynced();
        if (_containedBalls.Count == 0)
        {
            return null;
        }
        Ball first = _containedBalls.First();
        _containedBalls.RemoveFirst();
        EmitSignal(SignalName.InventoryChanged);
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
        EmitSignal(SignalName.InventoryChanged);
    }

    private void OnDispenseTimeout()
    {
        if (_queuedBalls.Count == 0) return;
        Ball ball = _queuedBalls.First();
        _queuedBalls.RemoveFirst();
        DispenseBall(ball);
    }

    public void ResetToStarterBalls(int count = 50, BallVariant? starterVariant = null)
    {
        // Free and clear contained balls
        foreach (Ball ball in _containedBalls)
        {
            ball.QueueFree();
        }
        _containedBalls.Clear();

        // Free and clear queued balls
        foreach (Ball ball in _queuedBalls)
        {
            ball.QueueFree();
        }
        _queuedBalls.Clear();

        // Clear any orphan balls under BallsRoot
        if (BallsRoot != null)
        {
            foreach (Node child in BallsRoot.GetChildren())
            {
                if (child is Ball orphanBall)
                {
                    orphanBall.QueueFree();
                }
            }
        }

        starterVariant ??= GameConfig.Instance?.BallTiers?[0]
            ?? ResourceLoader.Load<BallVariant>("res://src/balls/tiers/tier_1.tres")
            ?? new BallVariant { Tier = 1, BasePrice = 2 };

        PackedScene? ballScene = GameConfig.Instance?.BallScene
            ?? ResourceLoader.Load<PackedScene>("res://src/balls/ball.tscn");

        for (int i = 0; i < count; i++)
        {
            Ball ball = ballScene != null ? ballScene.Instantiate<Ball>() : new Ball();
            ball.Variant = starterVariant;
            ball.IsInPlay = false;
            if (BallsRoot != null)
            {
                BallsRoot.AddChild(ball);
            }
            _containedBalls.AddLast(ball);
        }

        EmitSignal(SignalName.InventoryChanged);
    }

    private void OnBallAwarded(BallVariant variant)
    {
        if (variant != null)
        {
            AddQueuedBalls(new[] { variant });
        }
    }
}
