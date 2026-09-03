using Godot;
using System.Diagnostics;

/// <summary>
/// Controls tulip arm rotation animations, physics collider states, and auto-close timers.
/// Configuration properties are resolved from PocketConfig.
/// </summary>
[Tool]
[GlobalClass]
public partial class PocketArmsController : Node2D
{
    public enum ArmState
    {
        Open,
        Closed,
        Opening,
        Closing,
    }

    [Export]
    public PocketConfig? Config { get; set; }

    [Export]
    public CharacterBody2D? LeftArm { get; set; }

    [Export]
    public CharacterBody2D? RightArm { get; set; }

    [Export]
    public CollisionShape2D? LeftArmCollider { get; set; }

    [Export]
    public CollisionShape2D? RightArmCollider { get; set; }

    [Export]
    public Sprite2D? LeftArmSprite { get; set; }

    [Export]
    public Sprite2D? RightArmSprite { get; set; }

    public bool HasArms => Config?.HasArms ?? true;
    public float ArmOpenRotation => Config?.ArmOpenRotation ?? 60.0f;
    public float ArmOpenDuration => Config?.ArmOpenDuration ?? 5.0f;
    public float ArmTweenDuration => Config?.ArmTweenDuration ?? 0.3f;
    public Tween.TransitionType ArmTweenTransition => Config?.ArmTweenTransition ?? Tween.TransitionType.Cubic;
    public Tween.EaseType ArmTweenEase => Config?.ArmTweenEase ?? Tween.EaseType.Out;

    public ArmState CurrentArmState { get; private set; } = ArmState.Closed;

    public bool IsOpen => CurrentArmState == ArmState.Open || CurrentArmState == ArmState.Opening;

    private Tween? _activeArmTween = null;
    private double _openTimerRemaining = 0.0;

    public override void _ExitTree()
    {
        // Cancel active tweens during scene unload or test teardown to eliminate orphan animation handles.
        if (_activeArmTween != null && _activeArmTween.IsValid())
        {
            _activeArmTween.Kill();
            _activeArmTween = null;
        }
    }

    public override void _Ready()
    {
        ApplyArmVisibility();

        if (Engine.IsEditorHint()) return;

        if (HasArms)
        {
            Debug.Assert(LeftArm != null, "PocketArmsController requires LeftArm reference when HasArms is true.");
            Debug.Assert(RightArm != null, "PocketArmsController requires RightArm reference when HasArms is true.");
        }
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

        // Auto-close arms once open window expires.
        if (_openTimerRemaining > 0.0)
        {
            _openTimerRemaining -= delta;
            if (_openTimerRemaining <= 0.0)
            {
                _openTimerRemaining = 0.0;
                CloseArms();
            }
        }
    }

    /// <summary>
    /// Synchronizes arm visibility and process mode based on HasArms configuration.
    /// </summary>
    public void ApplyArmVisibility()
    {
        if (LeftArm != null)
        {
            LeftArm.Visible = HasArms;
            LeftArm.ProcessMode = HasArms ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        }
        if (RightArm != null)
        {
            RightArm.Visible = HasArms;
            RightArm.ProcessMode = HasArms ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        }
    }

    /// <summary>
    /// Opens both tulip arms outward to widen the pocket's ball capture area.
    /// </summary>
    public void OpenArms(float? duration = null)
    {
        if (!HasArms || LeftArm == null || RightArm == null) return;

        _openTimerRemaining = duration ?? ArmOpenDuration;

        if (CurrentArmState == ArmState.Open)
        {
            return;
        }

        // Cancel opposing or stale tweens to keep rotation animations deterministic.
        if (_activeArmTween != null && _activeArmTween.IsValid())
        {
            _activeArmTween.Kill();
        }

        // Headless tests or unparented nodes cannot create SceneTree tweens; apply rotations directly.
        if (!IsInsideTree())
        {
            CurrentArmState = ArmState.Open;
            LeftArm.RotationDegrees = -ArmOpenRotation;
            RightArm.RotationDegrees = ArmOpenRotation;
            return;
        }

        _activeArmTween = CreateTween();
        // Physics mode prevents deflection tunneling by updating Rapier body transforms on physics ticks.
        _activeArmTween.SetProcessMode(Tween.TweenProcessMode.Physics);
        _activeArmTween.SetParallel(true);

        CurrentArmState = ArmState.Opening;

        _activeArmTween.TweenProperty(LeftArm, (NodePath)Node2D.PropertyName.RotationDegrees.ToString(), -ArmOpenRotation, ArmTweenDuration)
            .SetTrans(ArmTweenTransition)
            .SetEase(ArmTweenEase);

        _activeArmTween.TweenProperty(RightArm, (NodePath)Node2D.PropertyName.RotationDegrees.ToString(), ArmOpenRotation, ArmTweenDuration)
            .SetTrans(ArmTweenTransition)
            .SetEase(ArmTweenEase);

        _activeArmTween.Connect(Tween.SignalName.Finished, Callable.From(() =>
        {
            if (CurrentArmState == ArmState.Opening)
            {
                CurrentArmState = ArmState.Open;
            }
        }), (uint)ConnectFlags.OneShot);
    }

    /// <summary>
    /// Closes both tulip arms inward back to default resting alignment.
    /// </summary>
    public void CloseArms()
    {
        if (!HasArms || LeftArm == null || RightArm == null) return;

        _openTimerRemaining = 0.0;

        if (CurrentArmState == ArmState.Closed)
        {
            return;
        }

        if (_activeArmTween != null && _activeArmTween.IsValid())
        {
            _activeArmTween.Kill();
        }

        if (!IsInsideTree())
        {
            CurrentArmState = ArmState.Closed;
            LeftArm.RotationDegrees = 0.0f;
            RightArm.RotationDegrees = 0.0f;
            return;
        }

        _activeArmTween = CreateTween();
        _activeArmTween.SetProcessMode(Tween.TweenProcessMode.Physics);
        _activeArmTween.SetParallel(true);

        CurrentArmState = ArmState.Closing;

        _activeArmTween.TweenProperty(LeftArm, (NodePath)Node2D.PropertyName.RotationDegrees.ToString(), 0.0f, ArmTweenDuration)
            .SetTrans(ArmTweenTransition)
            .SetEase(ArmTweenEase);

        _activeArmTween.TweenProperty(RightArm, (NodePath)Node2D.PropertyName.RotationDegrees.ToString(), 0.0f, ArmTweenDuration)
            .SetTrans(ArmTweenTransition)
            .SetEase(ArmTweenEase);

        _activeArmTween.Connect(Tween.SignalName.Finished, Callable.From(() =>
        {
            if (CurrentArmState == ArmState.Closing)
            {
                CurrentArmState = ArmState.Closed;
            }
        }), (uint)ConnectFlags.OneShot);
    }

    /// <summary>
    /// Toggles between open and closed arm states.
    /// </summary>
    public void ToggleArms(float? duration = null)
    {
        if (IsOpen)
        {
            CloseArms();
        }
        else
        {
            OpenArms(duration);
        }
    }
}
