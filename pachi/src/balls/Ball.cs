using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class Ball : RigidBody2D
{
    [Signal]
    public delegate void FadeInFinishedEventHandler();

    [Signal]
    public delegate void FadeOutFinishedEventHandler();

    [Export]
    public AudioStreamPlayer2D BallPinBounceAudioPlayer { get; set; }
    [Export]
    public AudioStreamPlayer2D BallBallBounceAudioPlayer { get; set; }

    /// impact strength threshold at which bounce audio will play
    [Export]
    public float BounceAudioThreshold { get; set; } = 10.0f;

    [Export]
    public float FadeScale { get; set; } = 0.5f;

    [Export]
    public Color FadeModulate { get; set; } = new(0.5f, 0.5f, 0.5f, 0.5f);

    [Export]
    public float FadeDuration { get; set; } = 0.5f;

    [Export]
    public float MaxExpectedVelocity = 800.0f;

    [Export]
    public CollisionShape2D Collider { get; set; }

    [Export]
    public Timer FadeTimer { get; set; }

    public FadeState CurrentFadeState { get; set; } = FadeState.None;

    public enum FadeState
    {
        None,
        FadeIn,
        FadeOut
    };


    private Tween _fadeTween = null;

    private Color _originalModulate = Colors.White;

    private Vector2 _previousVelocity = Vector2.Zero;

    public override void _Ready()
    {
        Debug.Assert(BallPinBounceAudioPlayer != null);
        Debug.Assert(BallBallBounceAudioPlayer != null);
        Debug.Assert(Collider != null);
        Debug.Assert(Collider.Shape is CircleShape2D);
        Debug.Assert(FadeTimer != null);

        BodyEntered += OnBodyEntered;
        FadeTimer.Timeout += OnFadeTimeout;

        _originalModulate = Modulate;

        // TEST - DEBUG
        // TestFadeInFadeOut();
    }

    public override void _PhysicsProcess(double delta)
    {
        _previousVelocity = LinearVelocity;
    }

    public float GetRadius()
    {
        Debug.Assert(Collider.Shape is CircleShape2D);
        CircleShape2D circle = (CircleShape2D)Collider.Shape;
        Debug.Assert(Mathf.IsEqualApprox(Scale.X, Scale.Y));
        return circle.Radius * Scale.X;
    }

    public void FadeIn(Vector2? globalDestination = null)
    {
        CancelFade();
        Freeze = true;
        CurrentFadeState = FadeState.FadeIn;
        _fadeTween = GetTree().CreateTween();
        _fadeTween.TweenProperty(this, "scale", Vector2.One, FadeDuration);
        if (globalDestination != null)
        {
            // TODO: lerp shaping
            _fadeTween.TweenProperty(this, "global_position", globalDestination.Value, FadeDuration / 2);
        }
        _fadeTween.TweenProperty(this, "modulate", _originalModulate, FadeDuration);
        FadeTimer.Start(FadeDuration);
    }

    public void FadeOut(Vector2? globalDestination = null, Color? fadeModulateOverride = null)
    {
        CancelFade();
        Freeze = true;
        CurrentFadeState = FadeState.FadeOut;
        _fadeTween = GetTree().CreateTween();
        _fadeTween.TweenProperty(this, "scale", new Vector2(FadeScale, FadeScale), FadeDuration);
        if (globalDestination != null)
        {
            // TODO: lerp shaping
            _fadeTween.TweenProperty(this, "global_position", globalDestination.Value, FadeDuration / 2);
        }
        Color fadeModulate = _originalModulate * (fadeModulateOverride ?? FadeModulate);
        _fadeTween.TweenProperty(this, "modulate", fadeModulate, FadeDuration);
        FadeTimer.Start(FadeDuration);
    }

    private void OnBodyEntered(Node body)
    {
        Debug.Assert(BallPinBounceAudioPlayer != null);

        float impactStrength = (_previousVelocity - LinearVelocity).Length();

        if (impactStrength < BounceAudioThreshold) return;

        // TODO: move group names to a constant
        if (body.IsInGroup("ball_material"))
        {
            PlayImpactAudio(BallBallBounceAudioPlayer, impactStrength);
        }
        else if (body.IsInGroup("pin_material"))
        {
            PlayImpactAudio(BallPinBounceAudioPlayer, impactStrength);
        }
    }

    private void PlayImpactAudio(AudioStreamPlayer2D audioPlayer, float impactStrength)
    {
        float normalizedImpact = Mathf.Clamp(impactStrength / MaxExpectedVelocity, 0.01f, 1.0f);
        audioPlayer.VolumeDb = Mathf.LinearToDb(normalizedImpact);

        float targetPitch = Mathf.Remap(normalizedImpact, 0.0f, 1.0f, 0.9f, 1.3f);
        audioPlayer.PitchScale = targetPitch + (float)GD.RandRange(-0.05, 0.05);

        audioPlayer.Play();
    }

    private void OnFadeTimeout()
    {
        switch (CurrentFadeState)
        {
            case FadeState.FadeIn:
                Freeze = false;
                CurrentFadeState = FadeState.None;
                EmitSignal(SignalName.FadeInFinished);
                break;
            case FadeState.FadeOut:
                CurrentFadeState = FadeState.None;
                EmitSignal(SignalName.FadeOutFinished);
                break;
            default:
                break;
        }
    }

    private void CancelFade()
    {
        if (CurrentFadeState == FadeState.None) return;
        Debug.Assert(_fadeTween != null);
        _fadeTween.Stop();
        _fadeTween = null;
        CurrentFadeState = FadeState.None;
    }

    async private void TestFadeInFadeOut()
    {
        SceneTreeTimer testTimer = GetTree().CreateTimer(1.0);
        await ToSignal(testTimer, SceneTreeTimer.SignalName.Timeout);
        FadeOut();
        testTimer = GetTree().CreateTimer(2.0);
        await ToSignal(testTimer, SceneTreeTimer.SignalName.Timeout);
        FadeIn();
    }

}
