using Godot;
using System;
using System.Diagnostics;

public partial class GameConfig : Node
{
    public static GameConfig Instance { get; private set; }

    [Export]
    public PackedScene BallScene { get; set; }

    public override void _EnterTree() {
        Debug.Assert(Instance == null);
        Instance = this;
    }
}
