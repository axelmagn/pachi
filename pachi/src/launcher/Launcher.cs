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
        _chargedBall.Freeze = false;
        // TODO: retrieve velocity vector from launch point
        // TODO: feed charge ratio through curve
        _chargedBall.LinearVelocity = new Vector2(100.0f, -200.0f) * 5.0f * _chargeRatio;
        _chargedBall = null;
    }
}
