using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class Pocket : Node2D
{
    public static readonly StringName GroupPockets = new("pockets");

    public enum ArmBehavior
    {
        None,
        Open,
        Close,
        Toggle,
    }

    public enum ArmState
    {
        Open,
        Closed,
        Opening,
        Closing,
    }

    private readonly VisualConfigBinding _binding;
    private VisualConfig? _configOverride;

    public Pocket()
    {
        _binding = new VisualConfigBinding(ApplyVisualConfig);
    }

    [Export]
    public VisualConfig? ConfigOverride
    {
        get => _configOverride;
        set
        {
            _configOverride = value;
            if (InputsIndicator != null) InputsIndicator.ConfigOverride = value;
            if (OutputsIndicator != null) OutputsIndicator.ConfigOverride = value;
            if (IsInsideTree())
            {
                _binding.Bind(_configOverride);
            }
        }
    }

    [Export]
    public Hole? CatchHole { get; set; }

    [Export]
    public Hole? RejectHole { get; set; }

    [Export]
    public CharacterBody2D? LeftArm { get; set; }

    [Export]
    public CharacterBody2D? RightArm { get; set; }

    [Export]
    public CollisionShape2D? LeftArmCollider;

    [Export]
    public CollisionShape2D? RightArmCollider;

    [Export]
    public Sprite2D? LeftArmSprite { get; set; }

    [Export]
    public Sprite2D? RightArmSprite { get; set; }

    [Export]
    public Node2D? LeftArmProcedural { get; set; }

    [Export]
    public Node2D? RightArmProcedural { get; set; }

    [Export]
    public Array<BallVariant>? InputBalls;

    public Array<bool>? InputBallSlotAvailable;

    [Export]
    public Array<BallVariant>? OutputBalls;

    [Export]
    public PocketBallsIndicator? InputsIndicator;

    [Export]
    public PocketBallsIndicator? OutputsIndicator;

    [Export]
    public AudioStreamPlayer2D? AcceptAudioPlayer { get; set; }

    [Export]
    public AudioStreamPlayer2D? RejectAudioPlayer { get; set; }

    [Export]
    public AudioStreamPlayer2D? PayoutAudioPlayer { get; set; }

    [Export]
    public Array<AudioStream>? AcceptAudioStreams { get; set; }

    [Export]
    public AudioStream? RejectAudioStream { get; set; }

    [Export]
    public AudioStream? PayoutAudioStream { get; set; }

    [Export]
    public bool UsePitchScaleFallback { get; set; } = true;

    [Export]
    public float SemitonesPerStep { get; set; } = 2.0f;

    [Export]
    public ArmBehavior CatchArmBehavior = ArmBehavior.None;

    [Export]
    public ArmBehavior AccumulateArmBehavior = ArmBehavior.Toggle;

    [Export]
    public ArmBehavior RejectArmBehavior = ArmBehavior.Close;

    [Export]
    public ArmBehavior PayoutArmBehavior = ArmBehavior.Open;

    [Export]
    public float ArmOpenRotation = 60.0f;

    [Export]
    public float ArmRotationSpeed = Mathf.Pi;

    [Export]
    public bool IsCentralPocket { get; set; } = false;

    [Export]
    public float ArmOpenDuration { get; set; } = 5.0f;

    [Export]
    public float ArmTweenDuration { get; set; } = 0.3f;

    [Export]
    public Tween.TransitionType ArmTweenTransition { get; set; } = Tween.TransitionType.Cubic;

    [Export]
    public Tween.EaseType ArmTweenEase { get; set; } = Tween.EaseType.Out;

    [Export]
    public bool RandomizeInputBalls = false;

    [Export]
    public bool RandomizeOutputBalls = false;

    [Export]
    public bool HasArms
    {
        get => _hasArms;
        set { _hasArms = value; Rebuild(); }
    }

    [Export]
    public float ArmLength
    {
        get => _armLength;
        set { _armLength = value; Rebuild(); }
    }

    [Export]
    public float ArmRadius
    {
        get => _armRadius;
        set { _armRadius = value; Rebuild(); }
    }

    public ArmState CurrentArmState { get; private set; } = ArmState.Closed;

    public bool IsOpen => CurrentArmState == ArmState.Open || CurrentArmState == ArmState.Opening;

    private Tween? _activeArmTween = null;
    private double _openTimerRemaining = 0.0;
    private bool _hasArms = true;
    private float _armLength = 24;
    private float _armRadius = 2;

    public override void _EnterTree()
    {
        _binding.Bind(_configOverride);
    }

    public override void _ExitTree()
    {
        _binding.Unbind();

        if (Engine.IsEditorHint()) return;

        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.CentralPocketPaidOut -= OnCentralPocketPaidOut;
        }

        CardDragController.Instance?.UnregisterTarget(this);
    }

    public override void _Ready()
    {
        Rebuild();

        if (InputsIndicator != null && InputBalls != null)
        {
            InputsIndicator.Balls = InputBalls;
        }
        if (OutputsIndicator != null && OutputBalls != null)
        {
            OutputsIndicator.Balls = OutputBalls;
        }

        if (_binding.ActiveConfig != null)
        {
            ApplyVisualConfig(_binding.ActiveConfig);
        }

        if (Engine.IsEditorHint()) return;

        Debug.Assert(CatchHole != null, "Pocket requires CatchHole reference.");
        Debug.Assert(RejectHole != null, "Pocket requires RejectHole reference.");
        Debug.Assert(LeftArm != null, "Pocket requires LeftArm reference.");
        Debug.Assert(RightArm != null, "Pocket requires RightArm reference.");
        Debug.Assert(InputsIndicator != null, "Pocket requires InputsIndicator reference.");
        Debug.Assert(OutputsIndicator != null, "Pocket requires OutputsIndicator reference.");

        InputBalls = InputBalls == null ? [] : InputBalls.Duplicate();
        OutputBalls = OutputBalls == null ? [] : OutputBalls.Duplicate();

        var tiers = GameConfig.Instance?.BallTiers;
        var random = GameConfig.Instance?.Rng;
        if (RandomizeInputBalls && InputBalls != null && tiers != null && random != null)
        {
            for (int i = 0; i < InputBalls.Count; i++)
            {
                var idx = random.Next(0, tiers.Count);
                InputBalls[i] = tiers[idx];
            }
        }
        if (RandomizeOutputBalls && OutputBalls != null && tiers != null && random != null)
        {
            for (int i = 0; i < OutputBalls.Count; i++)
            {
                var idx = random.Next(0, tiers.Count);
                OutputBalls[i] = tiers[idx];
            }
        }

        CatchHole!.BallOverlapped += OnBallCatch;
        InputsIndicator!.Balls = InputBalls;
        OutputsIndicator!.Balls = OutputBalls;

        // initialize held balls tracker
        InputBallSlotAvailable = [];
        if (InputBalls != null)
        {
            for (int i = 0; i < InputBalls.Count; i++)
            {
                InputBallSlotAvailable.Add(true);
            }
            Debug.Assert(InputBalls.Count == InputBallSlotAvailable.Count);
        }

        AcceptAudioPlayer ??= GetNodeOrNull<AudioStreamPlayer2D>(nameof(AcceptAudioPlayer));
        RejectAudioPlayer ??= GetNodeOrNull<AudioStreamPlayer2D>(nameof(RejectAudioPlayer));
        PayoutAudioPlayer ??= GetNodeOrNull<AudioStreamPlayer2D>(nameof(PayoutAudioPlayer));

        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.CentralPocketPaidOut += OnCentralPocketPaidOut;
        }

        AddToGroup(GroupPockets);
        CardDragController.Instance?.RegisterTarget(this, 40.0f);
    }

    public virtual void ApplyVisualConfig(VisualConfig? config)
    {
        if (config == null) return;

        ApplyArmVisual(LeftArmSprite, LeftArmProcedural, config, isLeft: true);
        ApplyArmVisual(RightArmSprite, RightArmProcedural, config, isLeft: false);

        InputsIndicator?.ApplyVisualConfig(config);
        OutputsIndicator?.ApplyVisualConfig(config);
    }

    private static void ApplyArmVisual(Sprite2D? sprite, Node2D? procedural, VisualConfig config, bool isLeft)
    {
        if (config.ArmTexture != null)
        {
            if (sprite != null)
            {
                sprite.Texture = config.ArmTexture;
                sprite.Scale = Vector2.One * config.ArmTextureScale;
                sprite.Position = isLeft
                    ? new Vector2(-config.ArmTextureOffset.X, config.ArmTextureOffset.Y)
                    : config.ArmTextureOffset;
                sprite.FlipH = isLeft;
                sprite.Visible = true;
            }
            if (procedural != null)
            {
                procedural.Visible = false;
            }
        }
        else
        {
            if (sprite != null)
            {
                sprite.Visible = false;
            }
            if (procedural != null)
            {
                procedural.Visible = true;
                if (procedural is CapsuleSprite cs)
                {
                    cs.Color = config.ArmColor;
                }
                else
                {
                    procedural.Modulate = config.ArmColor;
                }
            }
        }
    }

    public void RefreshIndicatorAndSlots()
    {
        InputBallSlotAvailable = [];
        if (InputBalls != null)
        {
            for (int i = 0; i < InputBalls.Count; i++)
            {
                InputBallSlotAvailable.Add(true);
            }
        }

        if (InputsIndicator != null)
        {
            InputsIndicator.Balls = InputBalls;
            InputsIndicator.QueueRedraw();
        }
        if (OutputsIndicator != null)
        {
            OutputsIndicator.Balls = OutputBalls;
            OutputsIndicator.QueueRedraw();
        }
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

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

    public void OpenArms(float duration = 5.0f)
    {
        if (!HasArms || LeftArm == null || RightArm == null) return;

        _openTimerRemaining = duration;

        if (CurrentArmState == ArmState.Open)
        {
            return;
        }

        _activeArmTween?.Kill();
        _activeArmTween = CreateTween();
        _activeArmTween.SetProcessMode(Tween.TweenProcessMode.Physics);
        _activeArmTween.SetParallel(true);

        CurrentArmState = ArmState.Opening;

        _activeArmTween.TweenProperty(LeftArm, Node2D.PropertyName.RotationDegrees.ToString(), -ArmOpenRotation, ArmTweenDuration)
            .SetTrans(ArmTweenTransition)
            .SetEase(ArmTweenEase);

        _activeArmTween.TweenProperty(RightArm, Node2D.PropertyName.RotationDegrees.ToString(), ArmOpenRotation, ArmTweenDuration)
            .SetTrans(ArmTweenTransition)
            .SetEase(ArmTweenEase);

        _activeArmTween.Finished += () =>
        {
            if (CurrentArmState == ArmState.Opening)
            {
                CurrentArmState = ArmState.Open;
            }
        };
    }

    public void CloseArms()
    {
        if (!HasArms || LeftArm == null || RightArm == null) return;

        _openTimerRemaining = 0.0;

        if (CurrentArmState == ArmState.Closed)
        {
            return;
        }

        _activeArmTween?.Kill();
        _activeArmTween = CreateTween();
        _activeArmTween.SetProcessMode(Tween.TweenProcessMode.Physics);
        _activeArmTween.SetParallel(true);

        CurrentArmState = ArmState.Closing;

        _activeArmTween.TweenProperty(LeftArm, Node2D.PropertyName.RotationDegrees.ToString(), 0.0f, ArmTweenDuration)
            .SetTrans(ArmTweenTransition)
            .SetEase(ArmTweenEase);

        _activeArmTween.TweenProperty(RightArm, Node2D.PropertyName.RotationDegrees.ToString(), 0.0f, ArmTweenDuration)
            .SetTrans(ArmTweenTransition)
            .SetEase(ArmTweenEase);

        _activeArmTween.Finished += () =>
        {
            if (CurrentArmState == ArmState.Closing)
            {
                CurrentArmState = ArmState.Closed;
            }
        };
    }

    protected virtual void OnCentralPocketPaidOut()
    {
        OpenArms(ArmOpenDuration);
    }

    private void Rebuild()
    {
        float armColliderY = (ArmRadius - ArmLength) / 2;
        if (LeftArm != null)
        {
            LeftArm.Visible = HasArms;
            LeftArm.ProcessMode = HasArms ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

            if (LeftArmCollider != null && LeftArmCollider.Shape != null)
            {
                Debug.Assert(LeftArmCollider.Shape is CapsuleShape2D);
                LeftArmCollider.Position = new(0, armColliderY);
                CapsuleShape2D leftArmShape = (CapsuleShape2D)LeftArmCollider.Shape;
                leftArmShape.Radius = ArmRadius;
                leftArmShape.Height = ArmLength;
            }

            if (LeftArmProcedural is CapsuleSprite csLeft)
            {
                csLeft.Radius = ArmRadius;
                csLeft.Height = ArmLength;
            }
        }
        if (RightArm != null)
        {
            RightArm.Visible = HasArms;
            RightArm.ProcessMode = HasArms ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;

            if (RightArmCollider != null && RightArmCollider.Shape != null)
            {
                Debug.Assert(RightArmCollider.Shape is CapsuleShape2D);
                RightArmCollider.Position = new(0, armColliderY);
                CapsuleShape2D rightArmShape = (CapsuleShape2D)RightArmCollider.Shape;
                rightArmShape.Radius = ArmRadius;
                rightArmShape.Height = ArmLength;
            }

            if (RightArmProcedural is CapsuleSprite csRight)
            {
                csRight.Radius = ArmRadius;
                csRight.Height = ArmLength;
            }
        }
    }

    private void PlayAcceptSound(int filledSlotIndex)
    {
        if (AcceptAudioPlayer == null) return;

        if (AcceptAudioStreams != null && AcceptAudioStreams.Count > 0)
        {
            int streamIndex = Mathf.Clamp(filledSlotIndex, 0, AcceptAudioStreams.Count - 1);
            AcceptAudioPlayer.Stream = AcceptAudioStreams[streamIndex];

            if (filledSlotIndex >= AcceptAudioStreams.Count && UsePitchScaleFallback)
            {
                int extraSteps = filledSlotIndex - (AcceptAudioStreams.Count - 1);
                AcceptAudioPlayer.PitchScale = Mathf.Pow(2.0f, (extraSteps * SemitonesPerStep) / 12.0f);
            }
            else
            {
                AcceptAudioPlayer.PitchScale = 1.0f;
            }
        }
        else if (AcceptAudioPlayer.Stream != null)
        {
            AcceptAudioPlayer.PitchScale = Mathf.Pow(2.0f, (filledSlotIndex * SemitonesPerStep) / 12.0f);
        }

        AcceptAudioPlayer.Play();
    }

    private void PlayRejectSound()
    {
        if (RejectAudioPlayer == null) return;

        if (RejectAudioStream != null)
        {
            RejectAudioPlayer.Stream = RejectAudioStream;
        }
        RejectAudioPlayer.PitchScale = 1.0f;
        RejectAudioPlayer.Play();
    }

    private void PlayPayoutSound()
    {
        var player = PayoutAudioPlayer ?? AcceptAudioPlayer;
        if (player == null) return;

        if (PayoutAudioStream != null)
        {
            player.Stream = PayoutAudioStream;
        }
        player.PitchScale = 1.0f;
        player.Play();
    }

    protected virtual void OnBallCatch(Ball ball)
    {
        Debug.Assert(GlobalEvents.Instance != null, "GlobalEvents.Instance must not be null");
        GlobalEvents.Instance.NotifyBallEnteredPocket(this, ball);

        int filledBefore = 0;
        if (InputBallSlotAvailable != null)
        {
            foreach (bool available in InputBallSlotAvailable)
            {
                if (!available) filledBefore++;
            }
        }

        // accumulate ball
        bool reject = true;
        Debug.Assert(InputBalls != null && InputBallSlotAvailable != null && CatchHole != null && RejectHole != null);
        Debug.Assert(InputBalls!.Count == InputBallSlotAvailable!.Count);
        ball.FadeOut(CatchHole!.GlobalPosition);
        for (int i = 0; i < InputBalls.Count; i++)
        {
            if (InputBalls[i] == ball.Variant && InputBallSlotAvailable[i])
            {
                InputBallSlotAvailable[i] = false;
                reject = false;
                ball.Connect(Ball.SignalName.FadeOutFinished, Callable.From(ball.QueueFree),
                        (uint)ConnectFlags.OneShot);
                break;
            }
        }

        // reject ball
        if (reject)
        {
            PlayRejectSound();
            ball.Connect(Ball.SignalName.FadeOutFinished,
                    Callable.From(() => { ball.FadeIn(RejectHole!.GlobalPosition, true); }),
                    (uint)ConnectFlags.OneShot);
            return;
        }

        // emit reward signal if accumulated
        bool accumulationFilled = true;
        foreach (bool available in InputBallSlotAvailable)
        {
            if (available)
            {
                accumulationFilled = false;
                break;
            }
        }

        if (accumulationFilled)
        {
            PlayPayoutSound();

            // pay out rewards
            if (OutputBalls != null)
            {
                foreach (BallVariant variant in OutputBalls)
                {
                    GlobalEvents.Instance.NotifyBallAwarded(variant);
                }
            }

            // reset input ball slots
            for (int i = 0; i < InputBallSlotAvailable.Count; i++)
            {
                InputBallSlotAvailable[i] = true;
            }

            if (IsCentralPocket)
            {
                GlobalEvents.Instance.NotifyCentralPocketPaidOut();
            }
            else
            {
                if (IsOpen)
                {
                    CloseArms();
                }
                else
                {
                    OpenArms(ArmOpenDuration);
                }
            }
        }
        else
        {
            PlayAcceptSound(filledBefore);
        }
    }
}
