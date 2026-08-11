using Godot;
using System;
using System.Diagnostics;

/// Event bus for global events
public partial class GlobalEvents : Node
{
    public static GlobalEvents Instance { get; private set; }

    [Signal]
    public delegate void BallAwardedEventHandler(BallVariant ballVariant);

    [Signal]
    public delegate void CentralPocketPaidOutEventHandler();

    public override void _EnterTree()
    {
        Debug.Assert(Instance == null);
        Instance = this;
    }

    public void NotifyBallAwarded(BallVariant ballVariant)
    {
        EmitSignal(SignalName.BallAwarded, ballVariant);
    }

    public void NotifyCentralPocketPaidOut()
    {
        EmitSignal(SignalName.CentralPocketPaidOut);
    }
}
