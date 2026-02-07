using Godot;
using System;

namespace Pachi
{
[Tool]
public partial class Wall : StaticBody2D
{
    [Export]
    public Vector2 Size
    {
    get;
        set {
            field = value;
            TryUpdateWallShape();
        }
    } = new Vector2(100, 10);

    [Export]
    public CollisionShape2D Collider;

    public override void _Ready()
    {
        TryUpdateWallShape();
    }

    public override void _Draw()
    {
        if (Collider == null)
            return;
        if (!(Collider.Shape is RectangleShape2D rectShape))
            return;

        Rect2 rect = new Rect2(-rectShape.Size / 2, rectShape.Size);
        DrawRect(rect, Colors.White, filled: true);
    }

    private void TryUpdateWallShape()
    {
        if (Collider == null)
            return;

        RectangleShape2D wallShape = new();
        wallShape.SetSize(Size);
        Collider.Shape = wallShape;
    }
}
}
