using Godot;
using System;
using System.Diagnostics;

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
            GD.Print("launcher charge start");
            ChargeStart();
        }
        if (Input.IsActionJustReleased(ChargeInput))
        {
            GD.Print("launcher charge end");
            ChargeEnd();
        }

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

    private void RotateClamped(float delta)
    {
        float rotation = LauncherSprite.Rotation + delta;
        LauncherSprite.Rotation = Mathf.Clamp(rotation, MinRotation(), MaxRotation());
    }

    private void ChargeStart()
    {
        _rotationRate = (_endRotation - _startRotation) / ChargeTime;
        TryLoadBallFromHopper();
    }

    private void ChargeEnd()
    {
        _rotationRate = (_startRotation - _endRotation) / ReleaseTime;
    }

    // TODO: replace fade in / out effect with zoop animation
    async private void TryLoadBallFromHopper()
    {
        Debug.Assert(Hopper != null);
        Debug.Assert(Level != null);

        // see if we can pop a ball from the hopper
        if (_chargedBall != null) return;
        Ball poppedBall = Hopper.PopFirstBall();
        if (poppedBall == null) return;

        // set up charged ball
        _chargedBall = (Ball)poppedBall.Duplicate();
        Level.AddChild(_chargedBall);
        Debug.Assert(Level.BallsRoot != null);
        _chargedBall.Reparent(Level.BallsRoot);
        Debug.Assert(Level.BallLaunchPoint != null);
        _chargedBall.GlobalPosition = Level.BallLaunchPoint.GlobalPosition;

        // animate out popped ball
        poppedBall.FadeOut();
        poppedBall.FadeOutFinished += poppedBall.QueueFree;

        // animate in charged ball
        _chargedBall.FadeIn();
        Ball ballMemo = _chargedBall;
        _chargedBall.FadeInFinished += () =>
            {
                // check if ball has already been launched
                if (_chargedBall != ballMemo) return;
                _chargedBall.Freeze = true;
            };

    }

    private void TryLaunchBall()
    {
        if (_chargedBall == null) return;
        _chargedBall.Freeze = false;
        // TODO: add velocity
        _chargedBall = null;
    }
}
