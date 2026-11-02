using Godot;
using System.Diagnostics;

public static class ShapeUtil
{
    /// Radius of a circle collider, scaled by its (uniformly scaled) owner.
    public static float GetCircleRadius(CollisionShape2D? collider, Node2D owner)
    {
        Debug.Assert(collider?.Shape is CircleShape2D);
        Debug.Assert(Mathf.IsEqualApprox(owner.Scale.X, owner.Scale.Y));
        return ((CircleShape2D)collider!.Shape).Radius * owner.Scale.X;
    }
}
