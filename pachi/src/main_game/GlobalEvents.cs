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
    public delegate void BallEnteredPocketEventHandler(Node pocket, Node ball);

    [Signal]
    public delegate void YakumonoStateChangedEventHandler(Node yakumono, int faceIndex);

    [Signal]
    public delegate void YakumonoPaidOutEventHandler(Node yakumono);

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

    public void NotifyBallEnteredPocket(Node pocket, Node ball) =>
        EmitSignal(SignalName.BallEnteredPocket, pocket, ball);

    public void NotifyYakumonoStateChanged(Node yakumono, int faceIndex) =>
        EmitSignal(SignalName.YakumonoStateChanged, yakumono, faceIndex);

    public void NotifyYakumonoPaidOut(Node yakumono) =>
        EmitSignal(SignalName.YakumonoPaidOut, yakumono);
}
