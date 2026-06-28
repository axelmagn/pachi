using Godot;
using System;

/// Simple rectangle sprite
[Tool]
[GlobalClass]
public partial class RectSprite : Node2D
{
    private Vector2 _size = new Vector2(100.0f, 100.0f);
    [Export]
    public Vector2 Size
    {
        get => _size;
        set
        {
            _size = value;
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
        // DrawRect is a built-in CanvasItem method.
        // We draw it centered at Vector2.Zero, matching how CollisionShape2D is centered.
        DrawRect(new Rect2(-Size / 2.0f, Size), Color);
    }
}
