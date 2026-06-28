using Godot;
using System;

/// Simple capsule sprite
[Tool]
[GlobalClass]
public partial class CapsuleSprite : Node2D
{
    private float _radius = 20.0f;
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

    private float _height = 60.0f;
    [Export]
    public float Height
    {
        get => _height;
        set
        {
            _height = value;
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
        float clampedHeight = Mathf.Max(Height, 2.0f * Radius);
        float heightOffset = (clampedHeight / 2.0f) - Radius;

        // Draw the body (middle rectangle)
        if (heightOffset > 0f)
        {
            DrawRect(new Rect2(-Radius, -heightOffset, Radius * 2.0f, heightOffset * 2.0f), Color);
        }

        // Draw the two hemispherical caps
        DrawCircle(new Vector2(0f, -heightOffset), Radius, Color);
        DrawCircle(new Vector2(0f, heightOffset), Radius, Color);
    }
}
