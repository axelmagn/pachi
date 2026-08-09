using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class Launcher : Node2D
{
    [Export]
    public float ChargeTime { get; set; } = 1.0f;

    [Export]
    public float ReleaseTime { get; set; } = 0.2f;

    [Export]
    public StringName ChargeInput { get; set; } = "launcher_charge";

    [Export]
    public Node2D LauncherSprite { get; set; }

    [Export]
    public Node2D LauncherGhostSprite { get; set; }

    [Export]
    public Hopper Hopper { get; set; }

    [Export]
    public Level Level { get; set; }

    [Export]
    public float LaunchSpeed { get; set; } = 1100.0f;


    private float _startRotation;
    private float _endRotation;
    private float _rotationRate = 0.0f;
    private float _chargeRatio = 0.0f;
    private Ball _chargedBall;

    public override void _Ready()
    {
        Debug.Assert(LauncherSprite != null);
        Debug.Assert(LauncherGhostSprite != null);

        _startRotation = LauncherSprite.Rotation;
        _endRotation = LauncherGhostSprite.Rotation;

        _ = TryLoadBallFromHopper();
    }


    private bool IsLaunchPointClear()
    {
        if (Level == null || Level.BallLaunchPoint == null || Level.BallsRoot == null) return false;

        float clearanceRadius = 24.0f;
        float clearanceRadiusSq = clearanceRadius * clearanceRadius;
        Vector2 launchPos = Level.BallLaunchPoint.GlobalPosition;

        foreach (Node child in Level.BallsRoot.GetChildren())
        {
            if (child is Ball ball && !ball.Freeze && ball.CurrentTransitionState == Ball.TransitionState.None)
            {
                if ((ball.GlobalPosition - launchPos).LengthSquared() < clearanceRadiusSq)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed(ChargeInput))
        {
            ChargeStart();
        }
        if (Input.IsActionJustReleased(ChargeInput))
        {
            ChargeEnd();
        }

        if (_chargedBall == null && IsLaunchPointClear())
        {
            _ = TryLoadBallFromHopper();
        }

        // TODO: make launcher feel better to use
        RotateClamped((float)delta * _rotationRate);

        if (Mathf.IsEqualApprox(LauncherSprite.Rotation, _startRotation)
                && !Mathf.IsZeroApprox(_rotationRate))
        {
            TryLaunchBall();
            _rotationRate = 0.0f;
        }

        if (Mathf.IsEqualApprox(LauncherSprite.Rotation, _endRotation)
                && !Mathf.IsZeroApprox(_rotationRate))
        {
            // TODO: play audio cue
            _rotationRate = 0.0f;
        }


    }

    private float MinRotation()
    {
        return _startRotation < _endRotation ? _startRotation : _endRotation;
    }

    private float MaxRotation()
    {
        return _startRotation > _endRotation ? _startRotation : _endRotation;
    }

    private float RotationRatio()
    {
        float rotationRange = MaxRotation() - MinRotation();
        float rotationDelta = LauncherSprite.Rotation - MinRotation();
        float rotationRatio = rotationDelta / rotationRange;
        if (_startRotation > _endRotation)
        {
            rotationRatio = 1.0f - rotationRatio;
        }
        return (float)Mathf.Clamp(rotationRatio, 0.0, 1.0);
    }

    private void RotateClamped(float delta)
    {
        float rotation = LauncherSprite.Rotation + delta;
        LauncherSprite.Rotation = Mathf.Clamp(rotation, MinRotation(), MaxRotation());
    }

    private void ChargeStart()
    {
        _rotationRate = (_endRotation - _startRotation) / ChargeTime;
        _ = TryLoadBallFromHopper();
    }

    private void ChargeEnd()
    {
        _rotationRate = (_startRotation - _endRotation) / ReleaseTime;
        _chargeRatio = RotationRatio();
    }

    // TODO: replace fade in / out effect with zoop animation
    private async Task TryLoadBallFromHopper()
    {
        Debug.Assert(Hopper != null);
        Debug.Assert(Level != null);

        // see if we can pop a ball from the hopper
        if (_chargedBall != null) return;
        Ball poppedBall = Hopper.PopFirstContainedBall();
        if (poppedBall == null) return;

        // set up charged ball
        _chargedBall = (Ball)poppedBall.Duplicate();
        _chargedBall.CancelFade();
        _chargedBall.OriginalModulate = poppedBall.OriginalModulate;
        _chargedBall.Modulate = poppedBall.OriginalModulate;
        _chargedBall.Scale = Vector2.One;

        Level.BallsRoot.AddChild(_chargedBall);
        Level.BallsRoot.MoveChild(_chargedBall, 0);
        Debug.Assert(Level.BallsRoot != null);
        _chargedBall.Reparent(Level.BallsRoot);
        Debug.Assert(Level.BallLaunchPoint != null);
        _chargedBall.GlobalPosition = Level.BallLaunchPoint.GlobalPosition;

        // animate out popped ball
        poppedBall.FadeOut();
        poppedBall.Connect(Ball.SignalName.FadeOutFinished, Callable.From(poppedBall.QueueFree), (uint)GodotObject.ConnectFlags.OneShot);

        // animate in charged ball
        _chargedBall.FadeIn(initFadedOut: true);
        Ball ballMemo = _chargedBall;
        _chargedBall.Connect(Ball.SignalName.FadeInFinished, Callable.From(() =>
            {
                // check if ball has already been launched
                if (_chargedBall != ballMemo) return;
                _chargedBall.Freeze = true;
            }), (uint)GodotObject.ConnectFlags.OneShot);

    }

    private void TryLaunchBall()
    {
        if (_chargedBall == null) return;
        if (Level == null || Level.BallLaunchPoint == null) return;

        _chargedBall.Freeze = false;
        // Calculate local "Up" (-Y) vector of BallLaunchPoint in world space
        Vector2 launchDirection = Vector2.Up.Rotated(Level.BallLaunchPoint.GlobalRotation);
        // TODO: feed charge ratio through curve
        _chargedBall.LinearVelocity = launchDirection * LaunchSpeed * _chargeRatio;
        _chargedBall = null;
    }
}
