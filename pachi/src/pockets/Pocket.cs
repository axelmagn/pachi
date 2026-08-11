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
    public Array<AudioStream> AcceptAudioStreams { get; set; }

    [Export]
    public AudioStream RejectAudioStream { get; set; }

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

    private Tween _activeArmTween = null;
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

    private void OnBallCatch(Ball ball)
    {
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
                PlayAcceptSound(filledBefore);
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
        }
    }
}
