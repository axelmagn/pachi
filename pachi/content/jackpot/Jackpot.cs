using Godot;

public partial class Jackpot : Node2D
{
    [Export]
    public BallSink Sink { get; set; }

    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    [Export]
    public string OpenAnimation { get; set; }

    [Export]
    public string CloseAnimation { get; set; }

    [Export]
    public uint Payout { get; set; } = 15;

    private bool _gateOpen = false;

    [Export]
    public bool GateOpen
    {
        get => _gateOpen;
        set
        {
            _gateOpen = value;
            PlayGateAnimation();
        }
    }

    [Export]
    public Godot.Collections.Array<Jackpot> LinkedPayoutGates { get; set; } = new Godot.Collections.Array<Jackpot>();

    public override void _Ready()
    {
        if (Sink == null) GD.PushError("Jackpot: Sink is not assigned!");

        Sink?.Connect(BallSink.SignalName.BallSunk, Callable.From<Ball>(HandleBallSunk));

        // Initial animation state
        CallDeferred(MethodName.SetGateOpen, _gateOpen);
    }

    private void HandleBallSunk(Ball ball)
    {
        Game.Instance.Cash += Payout;

        foreach (Jackpot linkedGate in LinkedPayoutGates)
        {
            linkedGate.GateOpen = true;
        }

        ToggleGate();
    }

    public void ToggleGate()
    {
        GateOpen = !GateOpen;
    }

    public void SetGateOpen(bool gateOpen)
    {
        GateOpen = gateOpen;
    }

    private void PlayGateAnimation()
    {
        if (AnimationPlayer == null) return;

        string animName = _gateOpen ? OpenAnimation : CloseAnimation;
        if (!string.IsNullOrEmpty(animName))
        {
            AnimationPlayer.CurrentAnimation = animName;
            AnimationPlayer.Play();
        }
    }
}
