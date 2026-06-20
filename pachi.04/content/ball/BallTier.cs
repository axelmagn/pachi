using Godot;

[GlobalClass]
public partial class BallTier : Resource
{
    [Export]
    public Color Color { get; set; } = Colors.White;

    [Export]
    public float BasePrice { get; set; } = 1.0f;
}
