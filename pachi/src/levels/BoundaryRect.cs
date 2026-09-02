using Godot;
using System;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class BoundaryRect : StaticBody2D
{
    private float _width = 800;
    private float _height = 400;

    private CollisionShape2D? _topBoundary;
    private CollisionShape2D? _bottomBoundary;
    private CollisionShape2D? _leftBoundary;
    private CollisionShape2D? _rightBoundary;

    [Export]
    public float Width
    {
        get => _width;
        set
        {
            if (Mathf.IsEqualApprox(_width, value)) return;
            _width = value;
            Rebuild();
        }
    }

    [Export]
    public float Height
    {
        get => _height;
        set
        {
            if (Mathf.IsEqualApprox(_height, value)) return;
            _height = value;
            Rebuild();
        }
    }

    [Export]
    public CollisionShape2D? TopBoundary
    {
        get => _topBoundary;
        set
        {
            _topBoundary = value;
            Rebuild();
        }
    }

    [Export]
    public CollisionShape2D? BottomBoundary
    {
        get => _bottomBoundary;
        set
        {
            _bottomBoundary = value;
            Rebuild();
        }
    }

    [Export]
    public CollisionShape2D? LeftBoundary
    {
        get => _leftBoundary;
        set
        {
            _leftBoundary = value;
            Rebuild();
        }
    }

    [Export]
    public CollisionShape2D? RightBoundary
    {
        get => _rightBoundary;
        set
        {
            _rightBoundary = value;
            Rebuild();
        }
    }

    public override void _Ready()
    {
        Rebuild();

        if (!Engine.IsEditorHint())
        {
            Debug.Assert(TopBoundary != null, "BoundaryRect requires TopBoundary CollisionShape2D.");
            Debug.Assert(BottomBoundary != null, "BoundaryRect requires BottomBoundary CollisionShape2D.");
            Debug.Assert(LeftBoundary != null, "BoundaryRect requires LeftBoundary CollisionShape2D.");
            Debug.Assert(RightBoundary != null, "BoundaryRect requires RightBoundary CollisionShape2D.");
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
