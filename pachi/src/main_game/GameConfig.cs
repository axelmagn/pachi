using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

public partial class GameConfig : Node
{
    public static GameConfig? Instance { get; private set; }

    [Export]
    public PackedScene? BallScene { get; set; }

    [Export]
    public Array<BallVariant>? BallTiers { get; set; }

    public Random Rng { get; set; } = new Random();

    public override void _EnterTree()
    {
        Debug.Assert(Instance == null);
        Instance = this;
    }

    public override void _Ready()
    {
        Debug.Assert(BallScene != null, "BallScene must be configured on GameConfig");
        Debug.Assert(BallTiers != null && BallTiers.Count > 0, "BallTiers must be configured on GameConfig");
    }
}
