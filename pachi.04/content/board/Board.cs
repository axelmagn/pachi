using Godot;

public partial class Board : Node2D
{
    [Export]
    public BallSource LaunchSource { get; set; }

    public override void _Ready()
    {
        GD.Print("board ready");
    }
}
