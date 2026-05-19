using Godot;
using System.Collections.Generic;
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

    private int _nextLaunchSource = 0;
    private List<Ball> _pendingBalls = new List<Ball>();

    public override void _Ready()
    {
        if (LaunchTimer == null) GD.PushError("Hopper: LaunchTimer is not assigned!");
        if (DefaultBallScn == null) GD.PushError("Hopper: DefaultBallScn is not assigned!");

        LaunchTimer?.Connect(Timer.SignalName.Timeout, Callable.From(TryLaunchNextBall));

        AddDefaultBalls((int)InitialBalls);

        Game.Instance.Events.AddDefaultBalls += OnAddDefaultBalls;
    }

    private void OnAddDefaultBalls(int numBalls)
    {
        AddDefaultBalls(numBalls);
    }

    private void TryLaunchNextBall()
    {
        if (_pendingBalls.Count == 0) return;

        Ball nextBall = _pendingBalls[_pendingBalls.Count - 1];
        _pendingBalls.RemoveAt(_pendingBalls.Count - 1);

        if (_nextLaunchSource < LaunchSources.Count)
        {
            BallSource source = LaunchSources[_nextLaunchSource];
            source.LaunchExistingBall(nextBall, 1.0f);
            _nextLaunchSource = (_nextLaunchSource + 1) % LaunchSources.Count;
        }
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
        _pendingBalls.Add(ball);
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
}
