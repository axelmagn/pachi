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

    [Export]
    public bool IsAutoFiring { get; set; } = false;

    [Export]
    public float AutoFireInterval { get; set; } = 1.0f;

    [Export]
    public Curve LaunchPowerCurve { get; set; }

    [Export]
    public Curve AutoFireWeightCurve { get; set; }

    [Export]
    public int AutoFireBagResolution { get; set; } = 10;

    [Export]
    public float AutoFireWeightScale { get; set; } = 10.0f;

    [Export]
    public LauncherModeIndicator ModeIndicator { get; set; }


    private float _startRotation;
    private float _endRotation;
    private float _rotationRate = 0.0f;
    private float _chargeRatio = 0.0f;
    private Ball _chargedBall;
    private double _autoFireTimer = 0.0;
    private bool _isAutoCharging = false;
    private float _autoFireTargetRatio = 0.5f;
    private readonly System.Collections.Generic.List<float> _autoFireBag = new();
    private int _autoFireBagIndex = 0;
    private float _autoFireCurrentRatio = 1.0f;

    public override void _Ready()
    {
        Debug.Assert(LauncherSprite != null);
        Debug.Assert(LauncherGhostSprite != null);

        _startRotation = LauncherSprite.Rotation;
        _endRotation = LauncherGhostSprite.Rotation;

        ModeIndicator ??= GetNodeOrNull<LauncherModeIndicator>("LauncherModeIndicator");
        if (ModeIndicator != null)
        {
            ModeIndicator.IsAutoFiring = IsAutoFiring;
            ModeIndicator.AutoFireInterval = AutoFireInterval;
            ModeIndicator.ModeChanged += OnLauncherModeChanged;
        }

        _ = TryLoadBallFromHopper();
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
            if (Mathf.IsZeroApprox(_rotationRate) && Mathf.IsEqualApprox(LauncherSprite.Rotation, _startRotation))
            {
                _autoFireTimer += delta;
                if (_autoFireTimer >= AutoFireInterval && _chargedBall != null && IsLaunchPointClear())
                {
                    _autoFireTimer = 0.0;
                    _isAutoCharging = true;

                    Debug.Assert(GameConfig.Instance != null && GameConfig.Instance.Rng != null, "GameConfig.Instance and Rng must not be null");
                    float jitter = ((float)GameConfig.Instance.Rng.NextDouble() - 0.5f) * 0.08f;
                    _autoFireCurrentRatio = Mathf.Clamp(_autoFireTargetRatio + jitter, 0.0f, 1.0f);

                    ChargeStart();
                }
            }
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

        if (_isAutoCharging && !Mathf.IsZeroApprox(_rotationRate))
        {
            float targetRotation = Mathf.Lerp(_startRotation, _endRotation, _autoFireCurrentRatio);
            bool reachedTarget = (_endRotation > _startRotation)
                ? LauncherSprite.Rotation >= targetRotation
                : LauncherSprite.Rotation <= targetRotation;

            if (reachedTarget)
            {
                _isAutoCharging = false;
                ChargeEnd();
            }
        }
        else if (Mathf.IsEqualApprox(LauncherSprite.Rotation, _endRotation)
                && !Mathf.IsZeroApprox(_rotationRate))
        {
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
        
        float evaluatedPower = LaunchPowerCurve != null ? LaunchPowerCurve.Sample(_chargeRatio) : _chargeRatio;
        _chargedBall.LinearVelocity = launchDirection * LaunchSpeed * evaluatedPower;
        _chargedBall = null;
    }

    private float GetNextAutoFireTargetRatio()
    {
        if (_autoFireBag.Count == 0 || _autoFireBagIndex >= _autoFireBag.Count)
        {
            RebuildAndShuffleAutoFireBag();
        }
        if (_autoFireBag.Count == 0) return 0.5f;

        return _autoFireBag[_autoFireBagIndex++];
    }

    private void RebuildAndShuffleAutoFireBag()
    {
        _autoFireBag.Clear();
        int steps = Mathf.Max(2, AutoFireBagResolution);
        float minRatio = 0.0f;
        float maxRatio = 1.0f;

        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / (steps - 1);
            float ratio = Mathf.Lerp(minRatio, maxRatio, t);
            float weight = AutoFireWeightCurve != null ? Mathf.Max(0.0f, AutoFireWeightCurve.Sample(t)) : 1.0f;
            int count = Mathf.RoundToInt(weight * AutoFireWeightScale);

            for (int c = 0; c < count; c++)
            {
                _autoFireBag.Add(ratio);
            }
        }

        if (_autoFireBag.Count == 0)
        {
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / (steps - 1);
                _autoFireBag.Add(Mathf.Lerp(minRatio, maxRatio, t));
            }
        }

        Debug.Assert(GameConfig.Instance != null && GameConfig.Instance.Rng != null, "GameConfig.Instance and Rng must not be null");
        var rng = GameConfig.Instance.Rng;
        for (int i = _autoFireBag.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (_autoFireBag[i], _autoFireBag[j]) = (_autoFireBag[j], _autoFireBag[i]);
        }

        _autoFireBagIndex = 0;
    }
}
