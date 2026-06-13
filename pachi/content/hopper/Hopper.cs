using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Hopper : Node2D
{
    [Export]
    public Godot.Collections.Array<BallSource> LaunchSources { get; set; } = new Godot.Collections.Array<BallSource>();

    [Export]
    public Timer LaunchTimer { get; set; }

    [Export]
    public PackedScene DefaultBallScn { get; set; }

    [Export]
    public uint InitialBalls { get; set; } = 20;

    [Export]
    public uint MaxBalls { get; set; } = 256;

    private int _nextLaunchSource = 0;
    private Queue<Ball> _pendingBalls = new Queue<Ball>();
    private Queue<Ball> _containedBalls = new Queue<Ball>();

    public override void _Ready()
    {
        if (LaunchTimer == null) GD.PushError("Hopper: LaunchTimer is not assigned!");
        if (DefaultBallScn == null) GD.PushError("Hopper: DefaultBallScn is not assigned!");

        LaunchTimer?.Connect(Timer.SignalName.Timeout, Callable.From(TryLaunchNextBall));

        AddDefaultBalls((int)InitialBalls);

        Game.Instance.Events.AddDefaultBalls += OnAddDefaultBalls;
        Game.Instance.PhaseExited += OnGamePhaseExited;
    }

    private void OnAddDefaultBalls(int numBalls)
    {
        AddDefaultBalls(numBalls);
    }

    private void TryLaunchNextBall()
    {
        if (_pendingBalls.Count == 0) return;
        if (_containedBalls.Count >= MaxBalls) return;

        _nextLaunchSource = (_nextLaunchSource + 1) % LaunchSources.Count;
        // Ball nextBall = _pendingBalls[_pendingBalls.Count - 1];
        Ball nextBall = _pendingBalls.Dequeue();
        BallSource source = LaunchSources[_nextLaunchSource];
        _containedBalls.Enqueue(nextBall);
        source.LaunchExistingBall(nextBall, 1.0f);
    }

    public void AddDefaultBalls(int numBalls)
    {
        if (DefaultBallScn == null) return;

        for (int i = 0; i < numBalls; i++)
        {
            AddBall(DefaultBallScn.Instantiate<Ball>());
        }
    }

    public void AddBall(Ball ball)
    {
        // We expect ball.Tier to be null when a ball is freshly instantiated programmatically
        // without a pre-configured tier. We assign it to a random tier from the available ball tiers.
        if (ball.Tier == null)
        {
            Debug.Assert(Game.Instance != null);
            Debug.Assert(Game.Instance.BallTiers != null);
            Debug.Assert(Game.Instance.BallTiers.Count > 0);

            int randomIndex = (int)(GD.Randi() % (uint)Game.Instance.BallTiers.Count);
            ball.Tier = Game.Instance.BallTiers[randomIndex];
        }

        _pendingBalls.Enqueue(ball);
    }

    public uint GetBallCount()
    {
        uint count = 0;
        foreach (Node child in GetChildren())
        {
            if (child is Ball) count++;
        }
        return count;
    }

    public void DestroyBalls(int numBalls)
    {
        var balls = GetChildren().OfType<Ball>().Take(numBalls).ToList();
        foreach (var ball in balls)
        {
            ball.QueueFree();
        }
    }

    public Ball DequeueNextBall()
    {
        if (_containedBalls.Count == 0) return null;
        return _containedBalls.Dequeue();
    }

    private void BankContainedBalls() {
        while(_containedBalls.Count > 0) {
            Ball ball = _containedBalls.Dequeue();
            // TODO: add ball to resources
            Game.Instance.Cash += 5;
            ball.QueueFree();
        }
    }

    private void OnGamePhaseExited(Game.GamePhase oldPhase) {
        switch(oldPhase) {
            case Game.GamePhase.Play:
                BankContainedBalls();
                break;
            default:
                break;
        }
    }
}
