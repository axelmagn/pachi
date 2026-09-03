using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Core gameplay pocket responsible for ball acceptance, capacity clamping,
/// slot state tracking, and payout reward dispatching.
/// </summary>
[Tool]
[GlobalClass]
public partial class Pocket : Node2D
{
    public static readonly StringName GroupPockets = new("pockets");
    public const int MaxInputCapacity = 4;
    public const int MaxOutputCapacity = 8;

    [Export]
    public PocketConfig? Config { get; set; }

    [Export]
    public Hole? CatchHole { get; set; }

    [Export]
    public Hole? RejectHole { get; set; }

    [Export]
    public Sprite2D? PocketSprite { get; set; }

    [Export]
    public Array<BallVariant>? InputBalls { get; set; }

    [Export]
    public Array<BallVariant>? OutputBalls { get; set; }

    [Export]
    public PocketBallsIndicator? InputsIndicator { get; set; }

    [Export]
    public PocketBallsIndicator? OutputsIndicator { get; set; }

    [Export]
    public PocketArmsController? ArmsController { get; set; }

    [Export]
    public PocketAudioPlayer? AudioPlayer { get; set; }

    [Export]
    public bool IsCentralPocket { get; set; } = false;

    [Export]
    public bool RandomizeInputBalls { get; set; } = false;

    [Export]
    public bool RandomizeOutputBalls { get; set; } = false;

    private readonly List<bool> _inputBallSlotAvailable = [];

    public IReadOnlyList<bool> InputBallSlotAvailable => _inputBallSlotAvailable;

    public bool IsOpen => ArmsController?.IsOpen ?? false;

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint()) return;

        // Prevent dangling event references when scenes reload or reload in tests.
        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.CentralPocketPaidOut -= OnCentralPocketPaidOut;
        }

        CardDragController.Instance?.UnregisterTarget(this);
    }

    public override void _Ready()
    {
        RefreshIndicatorAndSlots();

        if (Engine.IsEditorHint()) return;

        Debug.Assert(CatchHole != null, "Pocket requires CatchHole reference.");
        Debug.Assert(RejectHole != null, "Pocket requires RejectHole reference.");
        Debug.Assert(InputsIndicator != null, "Pocket requires InputsIndicator reference.");
        Debug.Assert(OutputsIndicator != null, "Pocket requires OutputsIndicator reference.");

        // Duplicate exported arrays so modifications to one pocket instance do not mutate the shared PackedScene resource.
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

        RefreshIndicatorAndSlots();
        CatchHole!.BallOverlapped += OnBallCatch;

        // Auto-resolve child components when scenes instantiate without explicit node path exports.
        ArmsController ??= GetNodeOrNull<PocketArmsController>(nameof(ArmsController))
            ?? GetNodeOrNull<PocketArmsController>("PocketArmsController");
        AudioPlayer ??= GetNodeOrNull<PocketAudioPlayer>(nameof(AudioPlayer))
            ?? GetNodeOrNull<PocketAudioPlayer>("PocketAudioPlayer");

        // Propagate consolidated config to child controllers if not already assigned.
        if (ArmsController != null && ArmsController.Config == null && Config != null)
        {
            ArmsController.Config = Config;
            ArmsController.ApplyArmVisibility();
        }
        if (AudioPlayer != null && AudioPlayer.Config == null && Config != null)
        {
            AudioPlayer.Config = Config;
        }

        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.CentralPocketPaidOut += OnCentralPocketPaidOut;
        }

        AddToGroup(GroupPockets);
        CardDragController.Instance?.RegisterTarget(this, 40.0f);
    }

    /// <summary>
    /// Enforces board balance constraints by trimming excess ball requirements and rewards.
    /// </summary>
    public void ClampBallCapacities()
    {
        if (InputBalls != null)
        {
            while (InputBalls.Count > MaxInputCapacity)
            {
                InputBalls.RemoveAt(InputBalls.Count - 1);
            }
        }
        if (OutputBalls != null)
        {
            while (OutputBalls.Count > MaxOutputCapacity)
            {
                OutputBalls.RemoveAt(OutputBalls.Count - 1);
            }
        }
    }

    /// <summary>
    /// Positions the output ball indicator directly underneath the input indicator.
    /// </summary>
    public void UpdateIndicatorLayout()
    {
        if (InputsIndicator != null && OutputsIndicator != null)
        {
            float gap = 2.0f;
            OutputsIndicator.Position = new Vector2(
                OutputsIndicator.Position.X,
                InputsIndicator.Position.Y + (InputsIndicator.Size.Y / 2.0f) + gap + (OutputsIndicator.Size.Y / 2.0f)
            );
        }
    }

    /// <summary>
    /// Re-evaluates ball capacities, clears slot states, and queues indicator redraws after external card modifications.
    /// </summary>
    public void RefreshIndicatorAndSlots()
    {
        ClampBallCapacities();
        _inputBallSlotAvailable.Clear();
        if (InputBalls != null)
        {
            for (int i = 0; i < InputBalls.Count; i++)
            {
                _inputBallSlotAvailable.Add(true);
            }
        }

        if (InputsIndicator != null)
        {
            InputsIndicator.IsInputIndicator = true;
            InputsIndicator.Balls = InputBalls;
            InputsIndicator.QueueRedraw();
        }
        if (OutputsIndicator != null)
        {
            OutputsIndicator.IsInputIndicator = false;
            OutputsIndicator.Balls = OutputBalls;
            OutputsIndicator.QueueRedraw();
        }
        UpdateIndicatorLayout();
    }

    public void OpenArms(float? duration = null)
    {
        ArmsController?.OpenArms(duration);
    }

    public void CloseArms()
    {
        ArmsController?.CloseArms();
    }

    public void ToggleArms(float? duration = null)
    {
        ArmsController?.ToggleArms(duration);
    }

    /// <summary>
    /// Central pocket payouts open peripheral tulip arms across the board to create cascade scoring windows.
    /// </summary>
    protected virtual void OnCentralPocketPaidOut()
    {
        ArmsController?.OpenArms();
    }

    protected virtual void OnBallCatch(Ball ball)
    {
        Debug.Assert(GlobalEvents.Instance != null, "GlobalEvents.Instance must not be null");
        GlobalEvents.Instance.NotifyBallEnteredPocket(this, ball);

        int filledBefore = 0;
        foreach (bool available in _inputBallSlotAvailable)
        {
            if (!available) filledBefore++;
        }

        // Match ball against first available requirement slot of identical tier.
        bool reject = true;
        Debug.Assert(InputBalls != null && CatchHole != null && RejectHole != null);
        Debug.Assert(InputBalls!.Count == _inputBallSlotAvailable.Count);
        ball.FadeOut(CatchHole!.GlobalPosition);
        for (int i = 0; i < InputBalls.Count; i++)
        {
            if (InputBalls[i] == ball.Variant && _inputBallSlotAvailable[i])
            {
                _inputBallSlotAvailable[i] = false;
                reject = false;
                // Delay node disposal until the fade-out tween completes smoothly.
                ball.Connect(Ball.SignalName.FadeOutFinished, Callable.From(ball.QueueFree),
                        (uint)ConnectFlags.OneShot);
                break;
            }
        }

        // Non-matching balls spit back onto the board through the reject hole.
        if (reject)
        {
            AudioPlayer?.PlayReject();
            ball.Connect(Ball.SignalName.FadeOutFinished,
                    Callable.From(() => { ball.FadeIn(RejectHole!.GlobalPosition, true); }),
                    (uint)ConnectFlags.OneShot);
            return;
        }

        // Evaluate whether all input slots have been satisfied to trigger payout.
        bool accumulationFilled = true;
        foreach (bool available in _inputBallSlotAvailable)
        {
            if (available)
            {
                accumulationFilled = false;
                break;
            }
        }

        if (accumulationFilled)
        {
            AudioPlayer?.PlayPayout();

            if (OutputBalls != null)
            {
                foreach (BallVariant variant in OutputBalls)
                {
                    GlobalEvents.Instance.NotifyBallAwarded(variant);
                }
            }

            // Reset requirement slots so the pocket can accumulate rewards again.
            for (int i = 0; i < _inputBallSlotAvailable.Count; i++)
            {
                _inputBallSlotAvailable[i] = true;
            }

            if (IsCentralPocket)
            {
                GlobalEvents.Instance.NotifyCentralPocketPaidOut();
            }
            else
            {
                // Standard pockets toggle arms to reward successful completion with a wider catch funnel.
                if (IsOpen)
                {
                    CloseArms();
                }
                else
                {
                    OpenArms();
                }
            }
        }
        else
        {
            AudioPlayer?.PlayAccept(filledBefore);
        }
    }
}
