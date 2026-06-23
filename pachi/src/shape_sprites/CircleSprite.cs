using Godot;
using System;

/// Simple circle sprite
[Tool]
[GlobalClass]
public partial class CircleSprite : Node2D
{
    private float _radius = 50.0f;
    [Export]
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            QueueRedraw();
        }
    }

    private Color _color = Colors.White;
    [Export]
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        // DrawCircle is a built-in CanvasItem method
        DrawCircle(Vector2.Zero, Radius, Color);
    }
}
