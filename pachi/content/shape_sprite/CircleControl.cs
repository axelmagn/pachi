using Godot;
using System;

[Tool]
public partial class CircleControl : Control
{
    private float _radius = 32.0f;
    private Vector2 _offset = Vector2.Zero;
    private Color _color = Colors.White;

    [Export]
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            UpdateVisuals();
        }
    }

    [Export]
    public Vector2 Offset
    {
        get => _offset;
        set
        {
            _offset = value;
            UpdateVisuals();
        }
    }

    [Export]
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateVisuals();
        }
    }


    public override void _Draw()
    {
        Vector2 center = new(_radius, _radius);
        DrawCircle(center + _offset, _radius, _color);
    }

    private void UpdateVisuals()
    {
        CustomMinimumSize = new Vector2(_radius * 2, _radius * 2);
        QueueRedraw();
    }
}
