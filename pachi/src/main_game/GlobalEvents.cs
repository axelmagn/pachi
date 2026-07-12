using Godot;
using System;
using System.Diagnostics;

/// Event bus for global events
public partial class GlobalEvents : Node
{

    public static GlobalEvents Instance { get; private set; }

    public override void _EnterTree()
    {
        Debug.Assert(Instance == null);
        Instance = this;
    }
}
