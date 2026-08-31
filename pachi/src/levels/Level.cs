using Godot;
using System;
using System.Diagnostics;

public partial class Level : Node2D
{

    [Export]
    public Node2D? BallsRoot { get; set; }

    // TODO: create launch point class
    [Export]
    public Node2D? BallLaunchPoint { get; set; }

    public override void _Ready()
    {
        Debug.Assert(BallsRoot != null);
        Debug.Assert(BallLaunchPoint != null);
    }

    public void ClearActiveBalls()
    {
        if (BallsRoot != null)
        {
            foreach (Node child in BallsRoot.GetChildren())
            {
                if (child is Ball ball)
                {
                    ball.QueueFree();
                }
            }
        }
    }

    public void ResetAllSockets(Hopper? hopper = null)
    {
        if (IsInsideTree() && GetTree() != null)
        {
            foreach (Node node in GetTree().GetNodesInGroup(Socket2D.GroupSockets))
            {
                if (node is Socket2D socket)
                {
                    socket.ResetToStarter(hopper);
                }
            }
        }
        else
        {
            ResetSocketsRecursive(this, hopper);
        }
    }

    private static void ResetSocketsRecursive(Node parent, Hopper? hopper)
    {
        foreach (Node child in parent.GetChildren())
        {
            if (child is Socket2D socket)
            {
                socket.ResetToStarter(hopper);
            }
            else
            {
                ResetSocketsRecursive(child, hopper);
            }
        }
    }
}
