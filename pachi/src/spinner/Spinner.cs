using Godot;
using System;

[Tool]
[GlobalClass]
public partial class Spinner : Node2D, ISocketComponent
{
    public virtual SocketCategory Category => SocketCategory.Spinner;
    public virtual Vector2 ComponentBounds => new Vector2(100, 160);

    public virtual void OnMounted(Socket2D parentSocket)
    {
    }

    public virtual void OnUnmounting(Socket2D parentSocket)
    {
    }

    public virtual void FlushActiveBalls(Action<BallVariant> refundCallback)
    {
    }
}
