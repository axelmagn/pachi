using Godot;
using System;

public interface ISocketComponent
{
    SocketCategory Category { get; }
    Vector2 ComponentBounds { get; }
    void OnMounted(Socket2D parentSocket);
    void OnUnmounting(Socket2D parentSocket);
    void FlushActiveBalls(Action<BallVariant> refundCallback);
}
