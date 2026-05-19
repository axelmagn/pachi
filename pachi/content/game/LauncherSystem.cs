using Godot;

public partial class LauncherSystem : Node
{
    [Export]
    public float MinLaunchStrength { get; set; } = 0.2f;

    [Export]
    public bool ContinuousLaunch { get; set; } = true;

    [Export]
    public Timer LaunchChargeTimer { get; set; }

    [Export]
    public Timer AutoFireTimer { get; set; }

    [Export]
    public bool LaunchOnRelease { get; set; } = false;

    private bool _maxLaunch = false;

    public override void _Ready()
    {
        if (LaunchChargeTimer == null) GD.PushError("LauncherSystem: LaunchChargeTimer is not assigned!");
        if (AutoFireTimer == null) GD.PushError("LauncherSystem: AutoFireTimer is not assigned!");

        LaunchChargeTimer?.Connect(Timer.SignalName.Timeout, Callable.From(HandleLaunchTimeout));
        AutoFireTimer?.Connect(Timer.SignalName.Timeout, Callable.From(HandleAutoFireTimeout));
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("ball_launch"))
        {
            LaunchChargeTimer?.Start();
            LaunchOnRelease = true;
        }

        if (Input.IsActionJustReleased("ball_launch"))
        {
            if (LaunchChargeTimer != null && !LaunchChargeTimer.Paused && LaunchOnRelease)
            {
                HandleLaunchInput();
            }
            LaunchChargeTimer?.Stop();
        }
    }

    private void HandleLaunchTimeout()
    {
        if (Input.IsActionPressed("ball_launch"))
        {
            _maxLaunch = true;
            HandleLaunchInput();
            _maxLaunch = false;
            LaunchOnRelease = false;
        }

        if (ContinuousLaunch)
        {
            LaunchChargeTimer?.Start();
        }
    }

    private void HandleAutoFireTimeout()
    {
        float launchStrength = (float)GD.RandRange(0.3, 0.9);
        Launch(launchStrength);
    }

    private void HandleLaunchInput()
    {
        Hopper hopper = Game.Instance.GetSceneHopper();
        BallSource ballSource = Game.Instance.GetSceneBallSource();

        if (hopper == null || ballSource == null) return;

        float timerProgress = GetProgress();
        float launchStrength = _maxLaunch ? 1.0f : MinLaunchStrength + (1.0f - MinLaunchStrength) * Mathf.Clamp(timerProgress, 0.0f, 1.0f);

        Launch(launchStrength);
    }

    private void Launch(float launchStrength)
    {
        Hopper hopper = Game.Instance.GetSceneHopper();
        if (hopper == null) return;

        Ball ball = RecursiveFindBall(hopper);
        if (ball != null)
        {
            BallSource ballSource = Game.Instance.GetSceneBallSource();
            ballSource?.LaunchExistingBall(ball, launchStrength);
        }
    }

    private Ball RecursiveFindBall(Node node)
    {
        if (node is Ball ball) return ball;

        foreach (Node child in node.GetChildren())
        {
            Ball result = RecursiveFindBall(child);
            if (result != null) return result;
        }

        return null;
    }

    public float GetProgress()
    {
        if (LaunchChargeTimer == null || LaunchChargeTimer.IsStopped() || LaunchChargeTimer.Paused)
        {
            return 0.0f;
        }

        return 1.0f - (float)(LaunchChargeTimer.TimeLeft / LaunchChargeTimer.WaitTime);
    }
}
