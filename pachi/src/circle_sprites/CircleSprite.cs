using Godot;
using System;
using System.Numerics;

[Tool]
public partial class CircleSprite : Node2D
{
    [Export]
    public float radius = 10;
    [Export]
    public Color color = Colors.White;

    public override void _Draw()
    {
        DrawCircle(Godot.Vector2.Zero, radius, color);
    }
}
