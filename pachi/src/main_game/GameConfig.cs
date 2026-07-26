using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

public partial class GameConfig : Node
{
    public static GameConfig Instance { get; private set; }

    [Export]
    public PackedScene BallScene { get; set; }

    [Export]
    public Array<BallVariant> BallTiers { get; set; }

    public Random Rng { get; set; } = new Random();

    public override void _EnterTree()
    {
        Debug.Assert(Instance == null);
        Instance = this;
    }
}
