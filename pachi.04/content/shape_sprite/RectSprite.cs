using Godot;

[Tool]
public partial class RectSprite : Node2D
{
    private Vector2 _size = new Vector2(64, 32);
    private Vector2 _offset = Vector2.Zero;
    private Color _color = Colors.White;

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
        Rect2 rect = new Rect2(_size / -2.0f + _offset, _size);
        DrawRect(rect, _color);
    }
}
