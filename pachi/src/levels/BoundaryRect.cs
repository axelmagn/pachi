using Godot;
using System;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class BoundaryRect : StaticBody2D
{
    private float _width = 800;
    private float _height = 400;

    private CollisionShape2D _topBoundary;
    private CollisionShape2D _bottomBoundary;
    private CollisionShape2D _leftBoundary;
    private CollisionShape2D _rightBoundary;

    private readonly VisualConfigBinding _binding;
    private VisualConfig _configOverride;

    private Color _backgroundColor = Colors.Transparent;

    public BoundaryRect()
    {
        _binding = new VisualConfigBinding(ApplyVisualConfig);
    }

    [Export]
    public VisualConfig ConfigOverride
    {
        get => _configOverride;
        set
        {
            _configOverride = value;
            if (IsInsideTree())
            {
                _binding.Bind(_configOverride);
            }
        }
    }

    [Export]
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            QueueRedraw();
        }
    }

    [Export]
    public float Width
    {
        get => _width;
        set { _width = value; Rebuild(); QueueRedraw(); }
    }

    [Export]
    public float Height
    {
        get => _height;
        set { _height = value; Rebuild(); QueueRedraw(); }
    }

    [Export]
    public CollisionShape2D TopBoundary
    {
        get => _topBoundary;
        set { _topBoundary = value; Rebuild(); }
    }

    [Export]
    public CollisionShape2D BottomBoundary
    {
        get => _bottomBoundary;
        set { _bottomBoundary = value; Rebuild(); }
    }

    [Export]
    public CollisionShape2D LeftBoundary
    {
        get => _leftBoundary;
        set { _leftBoundary = value; Rebuild(); }
    }

    [Export]
    public CollisionShape2D RightBoundary
    {
        get => _rightBoundary;
        set { _rightBoundary = value; Rebuild(); }
    }

    public override void _EnterTree()
    {
        _binding.Bind(_configOverride);
    }

    public override void _ExitTree()
    {
        _binding.Unbind();
    }

    public override void _Ready()
    {
        Rebuild();
        if (_binding.ActiveConfig != null)
        {
            ApplyVisualConfig(_binding.ActiveConfig);
        }

        if (!Engine.IsEditorHint())
        {
            Debug.Assert(TopBoundary != null, "BoundaryRect requires TopBoundary CollisionShape2D.");
            Debug.Assert(BottomBoundary != null, "BoundaryRect requires BottomBoundary CollisionShape2D.");
            Debug.Assert(LeftBoundary != null, "BoundaryRect requires LeftBoundary CollisionShape2D.");
            Debug.Assert(RightBoundary != null, "BoundaryRect requires RightBoundary CollisionShape2D.");
        }
    }

    public void ApplyVisualConfig(VisualConfig config)
    {
        if (config == null) return;
        BackgroundColor = config.BackgroundColor;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (BackgroundColor.A > 0.0f)
        {
            Rect2 rect = new Rect2(-_width / 2.0f, -_height / 2.0f, _width, _height);
            DrawRect(rect, BackgroundColor, filled: true);
        }
    }

    private void Rebuild()
    {
        if (_topBoundary != null) _topBoundary.Position = new(0, -_height / 2);
        if (_bottomBoundary != null) _bottomBoundary.Position = new(0, _height / 2);
        if (_leftBoundary != null) _leftBoundary.Position = new(-_width / 2, 0);
        if (_rightBoundary != null) _rightBoundary.Position = new(_width / 2, 0);
    }
}
