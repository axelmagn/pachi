using Godot;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace Pachi
{
[Tool]
public partial class WallsBox : StaticBody2D
{
    [Export]
    float Thickness
    {
    get;
        set {
            field = value;
            UpdateWallShapes();
        }
    } = 10;

    [Export] Vector2 Size
    {
    get;
        set {
            field = value;
            UpdateWallShapes();
        }
    } = new(400, 800);

    [Export] CollisionShape2D LeftWall;
    [Export]
    CollisionShape2D RightWall;
    [Export]
    CollisionShape2D TopWall;
    [Export]
    CollisionShape2D BottomWall;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            UpdateWallShapes();
        }
        else
        {
            Debug.Assert(LeftWall != null);
            Debug.Assert(RightWall != null);
            Debug.Assert(TopWall != null);
            Debug.Assert(BottomWall != null);
        }
    }

    public override void _Draw()
    {
        TryDrawWall(LeftWall);
        TryDrawWall(RightWall);
        TryDrawWall(TopWall);
        TryDrawWall(BottomWall);
    }

    void UpdateWallShapes()
    {
        Vector2 HalfSize = Size / 2;

        RectangleShape2D SideWallShape = new();
        SideWallShape.SetSize(new(Thickness, Size.Y + Thickness));
        if (LeftWall != null)
        {
            LeftWall.Shape = SideWallShape;
            LeftWall.Position = new(-HalfSize.X, 0);
        }
        if (RightWall != null)
        {
            RightWall.Shape = SideWallShape;
            RightWall.Position = new(HalfSize.X, 0);
        }

        RectangleShape2D BaseWallShape = new();
        BaseWallShape.SetSize(new(Size.X + Thickness, Thickness));
        if (TopWall != null)
        {
            TopWall.Shape = BaseWallShape;
            TopWall.Position = new(0, -HalfSize.Y);
        }
        if (BottomWall != null)
        {
            BottomWall.Shape = BaseWallShape;
            BottomWall.Position = new(0, HalfSize.Y);
        }
    }

    void TryDrawWall(CollisionShape2D wall)
    {
        if (wall == null)
            return;
        if (!(wall.Shape is RectangleShape2D rectShape))
            return;
        Rect2 rect = new(wall.Position - rectShape.Size / 2, rectShape.Size);
        DrawRect(rect, Colors.White, filled: true);
    }
}
}
