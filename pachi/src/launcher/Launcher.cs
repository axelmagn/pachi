using Godot;
using System;
using System.Diagnostics;

[GlobalClass]
public partial class Launcher : Node2D
{
    [Export]
    public float ChargeTime { get; set; } = 1.0f;

    [Export]
    public float ReleaseTime { get; set; } = 0.2f;

    [Export]
    public StringName ChargeInput { get; set; } = "launcher_charge";

    [Export]
    public Node2D? LauncherSprite { get; set; }

    [Export]
    public Node2D? LauncherGhostSprite { get; set; }

    [Export]
    public Hopper? Hopper { get; set; }

    [Export]
    public Level? Level { get; set; }

    [Export]
    public float LaunchSpeed { get; set; } = 1100.0f;

    [Export]
    public bool IsAutoFiring { get; set; } = false;

    [Export]
    public float AutoFireInterval { get; set; } = 1.0f;

    [Export]
    public Curve? LaunchPowerCurve { get; set; }

    [Export]
    public Curve? AutoFireWeightCurve { get; set; }

    [Export]
    public int AutoFireBagResolution { get; set; } = 100;

    [Export]
    public int AutoFireBagSize { get; set; } = 20;

    [Export]
    public LauncherModeIndicator? ModeIndicator { get; set; }

    [Export]
    public Node2D? Bird { get; set; }

    [Export]
    public Node2D? BeakPoint { get; set; }

    [Export]
    public Node2D? BeakTouchPoint { get; set; }

    /// Radius around the launch point that must be free of in-play balls before loading the next ball.
    [Export]
    public float LaunchClearanceRadius { get; set; } = 24.0f;



    private float _startRotation;
    private float _endRotation;
    private float _rotationRate = 0.0f;
    private float _chargeRatio = 0.0f;
    private Ball? _chargedBall;
    private double _autoFireTimer = 0.0;
    private bool _isAutoCharging = false;
    private float _autoFireTargetRatio = 0.5f;
    private AutoFireSampler? _autoFireSampler;
    private float _autoFireCurrentRatio = 1.0f;

    public override void _Ready()
    {
        this.Require(LauncherSprite, nameof(LauncherSprite));
        this.Require(LauncherGhostSprite, nameof(LauncherGhostSprite));
        this.Require(Bird, nameof(Bird));
        this.Require(BeakPoint, nameof(BeakPoint));
        this.Require(BeakTouchPoint, nameof(BeakTouchPoint));
        this.Require(Hopper, nameof(Hopper));
        this.Require(Level, nameof(Level));

        _startRotation = LauncherSprite!.Rotation;
        _endRotation = LauncherGhostSprite!.Rotation;

        ModeIndicator ??= GetNodeOrNull<LauncherModeIndicator>("LauncherModeIndicator");
        if (ModeIndicator != null)
        {
            ModeIndicator.IsAutoFiring = IsAutoFiring;
            ModeIndicator.AutoFireInterval = AutoFireInterval;
            ModeIndicator.ModeChanged += OnLauncherModeChanged;
        }

        TryLoadBallFromHopper();
    }

    public override void _ExitTree()
    {
        if (ModeIndicator != null)
        {
            ModeIndicator.ModeChanged -= OnLauncherModeChanged;
        }
    }

    private void OnLauncherModeChanged(bool isAutoFiring, float interval)
    {
        IsAutoFiring = isAutoFiring;
        AutoFireInterval = interval;
        _autoFireTimer = 0.0;
    }


    private bool IsLaunchPointClear()
    {
        if (Level == null || Level.BallLaunchPoint == null || Level.BallsRoot == null) return false;

        float clearanceRadiusSq = LaunchClearanceRadius * LaunchClearanceRadius;
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
            _isAutoCharging = false;
            ChargeStart();
        }
        if (Input.IsActionJustReleased(ChargeInput))
        {
            _isAutoCharging = false;
            ChargeEnd();
        }

        if (IsAutoFiring && !_isAutoCharging)
        {
            if (LauncherSprite != null && Mathf.IsZeroApprox(_rotationRate) && Mathf.IsEqualApprox(LauncherSprite.Rotation, _startRotation))
            {
                _autoFireTimer += delta;
                if (_autoFireTimer >= AutoFireInterval && _chargedBall != null && IsLaunchPointClear())
                {
                    _autoFireTimer = 0.0;
                    _isAutoCharging = true;

                    Debug.Assert(GameConfig.Instance != null, "GameConfig.Instance must not be null");
                    _autoFireTargetRatio = GetNextAutoFireTargetRatio();
                    float jitter = ((float)GameConfig.Instance!.Rng.NextDouble() - 0.5f) * 0.08f;
                    _autoFireCurrentRatio = Mathf.Clamp(_autoFireTargetRatio + jitter, 0.0f, 1.0f);

                    ChargeStart();
                }
            }
        }

        if (_chargedBall == null && IsLaunchPointClear())
        {
            TryLoadBallFromHopper();
        }

        // TODO: make launcher feel better to use
        RotateClamped((float)delta * _rotationRate);

        if (LauncherSprite != null && Mathf.IsEqualApprox(LauncherSprite.Rotation, _startRotation)
                && !Mathf.IsZeroApprox(_rotationRate))
        {
            TryLaunchBall();
            _rotationRate = 0.0f;
        }

        if (_isAutoCharging && !Mathf.IsZeroApprox(_rotationRate))
        {
            float targetRotation = Mathf.Lerp(_startRotation, _endRotation, _autoFireCurrentRatio);
            bool reachedTarget = (_endRotation > _startRotation)
                ? (LauncherSprite?.Rotation ?? 0f) >= targetRotation
                : (LauncherSprite?.Rotation ?? 0f) <= targetRotation;

            if (reachedTarget)
            {
                _isAutoCharging = false;
                ChargeEnd();
            }
        }
        else if (LauncherSprite != null && Mathf.IsEqualApprox(LauncherSprite.Rotation, _endRotation)
                && !Mathf.IsZeroApprox(_rotationRate))
        {
            _rotationRate = 0.0f;
        }
    }

    private float MinRotation() => _startRotation < _endRotation ? _startRotation : _endRotation;

    private float MaxRotation() => _startRotation > _endRotation ? _startRotation : _endRotation;

    private float RotationRatio()
    {
        float rotationRange = MaxRotation() - MinRotation();
        float rotationDelta = (LauncherSprite?.Rotation ?? 0f) - MinRotation();
        float rotationRatio = rotationDelta / rotationRange;
        if (_startRotation > _endRotation)
        {
            rotationRatio = 1.0f - rotationRatio;
        }
        return (float)Mathf.Clamp(rotationRatio, 0.0, 1.0);
    }

    private void RotateClamped(float delta)
    {
        if (LauncherSprite == null) return;
        float rotation = LauncherSprite.Rotation + delta;
        LauncherSprite.Rotation = Mathf.Clamp(rotation, MinRotation(), MaxRotation());
        RotateBirdToTouchLever();
    }

    private void ChargeStart()
    {
        _rotationRate = (_endRotation - _startRotation) / ChargeTime;
        TryLoadBallFromHopper();
    }

    private void ChargeEnd()
    {
        _rotationRate = (_startRotation - _endRotation) / ReleaseTime;
        _chargeRatio = RotationRatio();
    }

    // TODO: replace fade in / out effect with zoop animation
    private void TryLoadBallFromHopper()
    {
        // see if we can pop a ball from the hopper
        if (_chargedBall != null) return;
        if (Hopper == null) return;
        Ball? poppedBall = Hopper.PopFirstContainedBall();
        if (poppedBall == null) return;

        // set up charged ball
        _chargedBall = (Ball)poppedBall.Duplicate();
        _chargedBall.IsInPlay = true;
        _chargedBall.CancelFade();
        _chargedBall.OriginalModulate = poppedBall.OriginalModulate;
        _chargedBall.Modulate = poppedBall.OriginalModulate;
        _chargedBall.Scale = Vector2.One;

        if (Level?.BallsRoot != null)
        {
            Level.BallsRoot.AddChild(_chargedBall);
            Level.BallsRoot.MoveChild(_chargedBall, 0);
        }
        if (Level?.BallLaunchPoint != null)
        {
            _chargedBall.GlobalPosition = Level.BallLaunchPoint.GlobalPosition;
        }

        // animate out popped ball
        poppedBall.FadeOutAndFree();

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

        float evaluatedPower = LaunchPowerCurve != null ? LaunchPowerCurve.Sample(_chargeRatio) : _chargeRatio;
        _chargedBall.LinearVelocity = launchDirection * LaunchSpeed * evaluatedPower;
        _chargedBall = null;
    }

    private float GetNextAutoFireTargetRatio()
    {
        Debug.Assert(GameConfig.Instance != null, "GameConfig.Instance must not be null");
        _autoFireSampler ??= new AutoFireSampler(
            AutoFireWeightCurve, AutoFireBagResolution, AutoFireBagSize, GameConfig.Instance!.Rng);
        return _autoFireSampler.Next();
    }

    /// rotate bird to keep contact between beak and lever
    ///
    /// approximates this by pointing the bird at the lever
    private void RotateBirdToTouchLever()
    {
        Vector2 birdToBeak = BeakPoint!.GlobalPosition - Bird!.GlobalPosition;
        Vector2 birdToTouch = BeakTouchPoint!.GlobalPosition - Bird!.GlobalPosition;

        float rotDelta = birdToTouch.Angle() - birdToBeak.Angle();
        Bird.Rotate(rotDelta);
    }
}
