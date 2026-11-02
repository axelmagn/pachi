using Godot;
using System.Diagnostics;

/// Event bus for global events
[GlobalClass]
public partial class GlobalEvents : Node
{
    public static GlobalEvents? Instance { get; private set; }

    [Signal]
    public delegate void BallAwardedEventHandler(BallVariant ballVariant);

    [Signal]
    public delegate void CentralPocketPaidOutEventHandler();

    [Signal]
    public delegate void BallEnteredPocketEventHandler(Pocket pocket, Ball ball);

    [Signal]
    public delegate void YakumonoStateChangedEventHandler(Yakumono yakumono, int faceIndex);

    [Signal]
    public delegate void YakumonoPaidOutEventHandler(Yakumono yakumono);

    public override void _EnterTree()
    {
        Debug.Assert(Instance == null);
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void NotifyBallAwarded(BallVariant ballVariant) =>
        EmitSignal(SignalName.BallAwarded, ballVariant);

    public void NotifyCentralPocketPaidOut() =>
        EmitSignal(SignalName.CentralPocketPaidOut);

    public void NotifyBallEnteredPocket(Pocket pocket, Ball ball) =>
        EmitSignal(SignalName.BallEnteredPocket, pocket, ball);

    public void NotifyYakumonoStateChanged(Yakumono yakumono, int faceIndex) =>
        EmitSignal(SignalName.YakumonoStateChanged, yakumono, faceIndex);

    public void NotifyYakumonoPaidOut(Yakumono yakumono) =>
        EmitSignal(SignalName.YakumonoPaidOut, yakumono);
}
