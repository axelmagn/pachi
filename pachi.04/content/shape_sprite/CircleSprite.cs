using Godot;

[Tool]
public partial class CircleSprite : Node2D
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
            QueueRedraw();
        }
    }

    [Export]
    public Vector2 Offset
    {
        get => _offset;
        set
        {
            _offset = value;
            QueueRedraw();
        }
    }

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
        DrawCircle(_offset, _radius, _color);
    }
}
