using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

// TODO: arm behavior

[Tool]
public partial class Pocket : Node2D
{
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

    [Export]
    public Hole CatchHole { get; set; }

    [Export]
    public Hole RejectHole { get; set; }

    [Export]
    public CharacterBody2D LeftArm { get; set; }

    [Export]
    public CharacterBody2D RightArm { get; set; }

    [Export]
    public CollisionShape2D LeftArmCollider;

    [Export]
    public CollisionShape2D RightArmCollider;

    [Export]
    public Array<BallVariant> InputBalls;

    public Array<bool> InputBallSlotAvailable;

    [Export]
    public Array<BallVariant> OutputBalls;

    [Export]
    public PocketBallsIndicator InputsIndicator;

    [Export]
    public PocketBallsIndicator OutputsIndicator;

    [Export]
    public AudioStreamPlayer2D AcceptAudioPlayer { get; set; }

    [Export]
    public AudioStreamPlayer2D RejectAudioPlayer { get; set; }

    [Export]
    public AudioStreamPlayer2D PayoutAudioPlayer { get; set; }

    [Export]
    public Array<AudioStream> AcceptAudioStreams { get; set; }

    [Export]
    public AudioStream RejectAudioStream { get; set; }

    [Export]
    public AudioStream PayoutAudioStream { get; set; }

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

    private Tween _activeArmTween = null;
    private double _openTimerRemaining = 0.0;
    private bool _hasArms = true;
    private float _armLength = 24;
    private float _armRadius = 2;


    public override void _Ready()
    {
        // for now, do nothing on editor ready
        if (Engine.IsEditorHint()) return;

        Debug.Assert(CatchHole != null);
        Debug.Assert(RejectHole != null);
        Debug.Assert(LeftArm != null);
        Debug.Assert(RightArm != null);
        Debug.Assert(InputsIndicator != null);
        Debug.Assert(OutputsIndicator != null);

        InputBalls = InputBalls == null ? [] : InputBalls.Duplicate();
        OutputBalls = OutputBalls == null ? [] : OutputBalls.Duplicate();

        var tiers = GameConfig.Instance.BallTiers;
        var random = GameConfig.Instance.Rng;
        if (RandomizeInputBalls && InputBalls != null && tiers != null)
        {
            for (int i = 0; i < InputBalls.Count; i++)
            {
                var idx = random.Next(0, tiers.Count);
                InputBalls[i] = tiers[idx];
            }
        }
        if (RandomizeOutputBalls && OutputBalls != null && tiers != null)
        {
            for (int i = 0; i < OutputBalls.Count; i++)
            {
                var idx = random.Next(0, tiers.Count);
                OutputBalls[i] = tiers[idx];
            }
        }

        CatchHole.BallOverlapped += OnBallCatch;
        InputsIndicator.Balls = InputBalls;
        OutputsIndicator.Balls = OutputBalls;

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

        AcceptAudioPlayer ??= GetNodeOrNull<AudioStreamPlayer2D>("AcceptAudioPlayer");
        RejectAudioPlayer ??= GetNodeOrNull<AudioStreamPlayer2D>("RejectAudioPlayer");
        PayoutAudioPlayer ??= GetNodeOrNull<AudioStreamPlayer2D>("PayoutAudioPlayer");

        Debug.Assert(GlobalEvents.Instance != null, "GlobalEvents.Instance must not be null");
        GlobalEvents.Instance.CentralPocketPaidOut += OnCentralPocketPaidOut;

        AddToGroup("pockets");
        Debug.Assert(CardDragController.Instance != null, "CardDragController.Instance must not be null");
        CardDragController.Instance.RegisterTarget(this, 40.0f);
    }

    public override void _ExitTree()
    {
        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.CentralPocketPaidOut -= OnCentralPocketPaidOut;
        }

        CardDragController.Instance?.UnregisterTarget(this);
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

    private void OnCentralPocketPaidOut()
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

    private void OnBallCatch(Ball ball)
    {
        Debug.Assert(GlobalEvents.Instance != null, "GlobalEvents.Instance must not be null");
        GlobalEvents.Instance.NotifyBallEnteredPocket(this, ball);

        int filledBefore = 0;
        foreach (bool available in InputBallSlotAvailable)
        {
            if (!available) filledBefore++;
        }

        // accumulate ball
        bool reject = true;
        Debug.Assert(InputBalls.Count == InputBallSlotAvailable.Count);
        ball.FadeOut(CatchHole.GlobalPosition);
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
                    Callable.From(() => { ball.FadeIn(RejectHole.GlobalPosition, true); }),
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
            foreach (BallVariant variant in OutputBalls)
            {
                GlobalEvents.Instance.NotifyBallAwarded(variant);
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
