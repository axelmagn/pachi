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
    public BallVariant Variant { get; set; }

    [Export]
    public AudioStreamPlayer2D PinBounceAudioPlayer { get; set; }

    [Export]
    public AudioStreamPlayer2D BallBounceAudioPlayer { get; set; }

    [Export]
    public AudioStreamPlayer2D WallBounceAudioPlayer { get; set; }

    [Export]
    public ColliderCircleSprite PlaceholderSprite { get; set; }

    [Export]
    public MotionTrail2D MotionTrail { get; set; }

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

    [Export]
    public Timer StuckTimer { get; set; }

    [Export]
    public bool InitDetectStuck { get; set; }

    [Export]
    public bool EnableWallFollowing { get; set; } = true;

    /// Incident angle threshold relative to wall surface (in degrees).
    /// Angles shallower than this (default: 15.0 deg) will redirect velocity parallel to wall.
    [Export(PropertyHint.Range, "1,60")]
    public float WallFollowAngleThresholdDeg { get; set; } = 15.0f;

    /// Continuous factor [0.0, 1.0] for speed preservation:
    /// 0.0 = Raw tangential velocity (drops normal velocity component entirely, speed = |Vt|).
    /// 1.0 = Full speed preservation (maintains original scalar speed |V| along tangent).
    /// Intermediate values smoothly blend between the two.
    [Export(PropertyHint.Range, "0.0,1.0")]
    public float WallFollowSpeedPreservation { get; set; } = 0.5f;

    /// Group filter. Only colliders belonging to this group will trigger wall-following.
    /// Set to empty string to apply to all static colliders.
    [Export]
    public string WallGroupFilter { get; set; } = "wall_material";

    public static readonly StringName BallMaterialGroup = "ball_material";
    public static readonly StringName PinMaterialGroup = "pin_material";
    public static readonly StringName WallMaterialGroup = "wall_material";

    public TransitionState CurrentTransitionState { get; set; } = TransitionState.None;

    public enum TransitionState
    {
        None,
        FadeIn,
        FadeOut
    };


    private Tween _transitionTween = null;

    private Color _originalModulate = Colors.White;

    public Color OriginalModulate
    {
        get => _originalModulate;
        set => _originalModulate = value;
    }

    private Vector2 _previousVelocity = Vector2.Zero;

    private bool _detectStuck = false;

    public override void _Ready()
    {
        Debug.Assert(Variant != null);
        Debug.Assert(PlaceholderSprite != null);
        Debug.Assert(PinBounceAudioPlayer != null);
        Debug.Assert(BallBounceAudioPlayer != null);
        Debug.Assert(WallBounceAudioPlayer != null);
        Debug.Assert(Collider != null);
        Debug.Assert(Collider.Shape is CircleShape2D);
        Debug.Assert(FadeTimer != null);
        Debug.Assert(StuckTimer != null);
        Debug.Assert(MotionTrail != null);

        BodyEntered += OnBodyEntered;
        FadeTimer.Timeout += OnFadeTimeout;
        StuckTimer.Timeout += OnStuck;

        if (_originalModulate == Colors.White && CurrentTransitionState == TransitionState.None)
        {
            _originalModulate = Modulate;
        }

        if (InitDetectStuck)
        {
            StuckTimer.Start();
        }

        PlaceholderSprite.Color = Variant.PlaceholderColor;
        MotionTrail.SyncWithBall();
    }

    public override void _PhysicsProcess(double delta)
    {
        _previousVelocity = LinearVelocity;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);

        if (!EnableWallFollowing) return;

        int contactCount = state.GetContactCount();
        if (contactCount == 0) return;

        Vector2 currentVel = state.LinearVelocity;
        float currentSpeed = currentVel.Length();
        if (currentSpeed < 0.001f) return;

        Vector2 velDir = currentVel / currentSpeed;

        for (int i = 0; i < contactCount; i++)
        {
            // Transform contact normal from local body space to world space
            Vector2 localNormal = state.GetContactLocalNormal(i);
            Vector2 worldNormal = Transform.BasisXform(localNormal).Normalized();

            // Check if ball is moving into wall (Dot < 0)
            float normalVelComponent = currentVel.Dot(worldNormal);
            if (normalVelComponent >= 0f) continue;

            // Check group filter if specified
            if (!string.IsNullOrEmpty(WallGroupFilter))
            {
                GodotObject collider = state.GetContactColliderObject(i);
                if (collider is Node node && !node.IsInGroup(WallGroupFilter))
                {
                    continue;
                }
            }

            // Calculate incident angle with surface using world normal
            // cos(angle to normal) = -velDir.Dot(worldNormal)
            // sin(angle to surface) = cos(angle to normal)
            float cosNormalAngle = Mathf.Clamp(-velDir.Dot(worldNormal), 0f, 1f);
            float surfaceAngleRad = Mathf.Asin(cosNormalAngle);
            float surfaceAngleDeg = Mathf.RadToDeg(surfaceAngleRad);

            if (surfaceAngleDeg <= WallFollowAngleThresholdDeg)
            {
                // Tangential component of velocity in world space (normal component removed)
                Vector2 tangentVel = currentVel - normalVelComponent * worldNormal;

                if (tangentVel.LengthSquared() > 0.0001f)
                {
                    Vector2 tangentDir = tangentVel.Normalized();
                    Vector2 fullSpeedVel = tangentDir * currentSpeed;

                    // Continuously blend between raw tangential velocity and full scalar speed
                    float factor = Mathf.Clamp(WallFollowSpeedPreservation, 0.0f, 1.0f);
                    state.LinearVelocity = tangentVel.Lerp(fullSpeedVel, factor);
                }
                break; // Apply redirect for primary shallow contact
            }
        }
    }

    public float GetRadius()
    {
        Debug.Assert(Collider.Shape is CircleShape2D);
        CircleShape2D circle = (CircleShape2D)Collider.Shape;
        Debug.Assert(Mathf.IsEqualApprox(Scale.X, Scale.Y));
        return circle.Radius * Scale.X;
    }

    public void FadeIn(Vector2? globalDestination = null, bool initFadedOut = false)
    {
        CancelFade();

        if (initFadedOut)
        {
            Scale = new(FadeScale, FadeScale);
            Modulate = FadeModulate;
        }

        Freeze = true;
        CurrentTransitionState = TransitionState.FadeIn;
        _transitionTween = GetTree().CreateTween().SetParallel(true);
        _transitionTween.TweenProperty(this, (NodePath)PropertyName.Scale.ToString(), Vector2.One, FadeDuration);
        if (globalDestination != null)
        {
            // TODO: lerp shaping
            _transitionTween.TweenProperty(this, (NodePath)PropertyName.GlobalPosition.ToString(), globalDestination.Value, FadeDuration);
        }
        _transitionTween.TweenProperty(this, (NodePath)PropertyName.Modulate.ToString(), _originalModulate, FadeDuration);
        FadeTimer.Start(FadeDuration);
    }

    public void FadeOut(Vector2? globalDestination = null, Color? fadeModulateOverride = null)
    {
        CancelFade();
        Freeze = true;
        CurrentTransitionState = TransitionState.FadeOut;
        _transitionTween = GetTree().CreateTween().SetParallel(true);
        _transitionTween.TweenProperty(this, (NodePath)PropertyName.Scale.ToString(), new Vector2(FadeScale, FadeScale), FadeDuration);

        // _fadeTween.TweenProperty(this, (NodePath)PropertyName.GlobalPosition.ToString(), Vector2.Zero, FadeDuration); // DEBUG
        if (globalDestination != null)
        {
            // TODO: lerp shaping
            _transitionTween.TweenProperty(this, (NodePath)PropertyName.GlobalPosition.ToString(), globalDestination.Value, FadeDuration);
        }
        Color fadeModulate = _originalModulate * (fadeModulateOverride ?? FadeModulate);
        _transitionTween.TweenProperty(this, (NodePath)PropertyName.Modulate.ToString(), fadeModulate, FadeDuration);
        FadeTimer.Start(FadeDuration);
    }

    private void OnBodyEntered(Node body)
    {
        Debug.Assert(BallBounceAudioPlayer != null);
        Debug.Assert(PinBounceAudioPlayer != null);
        Debug.Assert(WallBounceAudioPlayer != null);

        float impactStrength = (_previousVelocity - LinearVelocity).Length();

        if (impactStrength < BounceAudioThreshold) return;

        if (body.IsInGroup(WallMaterialGroup))
        {
            PlayImpactAudio(WallBounceAudioPlayer, impactStrength);
        }
        else if (body.IsInGroup(BallMaterialGroup))
        {
            PlayImpactAudio(BallBounceAudioPlayer, impactStrength);
        }
        else if (body.IsInGroup(PinMaterialGroup))
        {
            PlayImpactAudio(PinBounceAudioPlayer, impactStrength);
        }
    }

    private void OnFadeTimeout()
    {
        switch (CurrentTransitionState)
        {
            case TransitionState.FadeIn:
                Freeze = false;
                CurrentTransitionState = TransitionState.None;
                EmitSignal(SignalName.FadeInFinished);
                break;
            case TransitionState.FadeOut:
                CurrentTransitionState = TransitionState.None;
                EmitSignal(SignalName.FadeOutFinished);
                break;
            default:
                break;
        }
    }

    private void OnStuck()
    {
        FadeOut();
        Connect(SignalName.FadeOutFinished, Callable.From(QueueFree), (uint)ConnectFlags.OneShot);
    }

    private void PlayImpactAudio(AudioStreamPlayer2D audioPlayer, float impactStrength)
    {
        float normalizedImpact = Mathf.Clamp(impactStrength / MaxExpectedVelocity, 0.01f, 1.0f);
        audioPlayer.VolumeDb = Mathf.LinearToDb(normalizedImpact);

        float targetPitch = Mathf.Remap(normalizedImpact, 0.0f, 1.0f, 0.9f, 1.3f);
        audioPlayer.PitchScale = targetPitch + (float)GD.RandRange(-0.05, 0.05);

        audioPlayer.Play();
    }

    public void CancelFade()
    {
        if (CurrentTransitionState == TransitionState.None) return;
        Debug.Assert(_transitionTween != null);
        _transitionTween.Stop();
        _transitionTween = null;
        CurrentTransitionState = TransitionState.None;
    }

    private async Task TestFadeInFadeOut()
    {
        SceneTreeTimer testTimer = GetTree().CreateTimer(1.0);
        await ToSignal(testTimer, SceneTreeTimer.SignalName.Timeout);
        FadeOut();
        testTimer = GetTree().CreateTimer(2.0);
        await ToSignal(testTimer, SceneTreeTimer.SignalName.Timeout);
        FadeIn();
    }
}
