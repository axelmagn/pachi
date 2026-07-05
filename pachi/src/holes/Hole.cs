using Godot;
using System;
using System.Diagnostics;

public partial class Hole : Area2D
{
    [Export]
    public CollisionShape2D Collider { get; set; }

    public override void _Ready()
    {
        Debug.Assert(Collider != null);
    }

    public float GetRadius()
    {
        Debug.Assert(Collider.Shape is CircleShape2D);
        CircleShape2D circle = (CircleShape2D)Collider.Shape;
        Debug.Assert(Mathf.IsEqualApprox(Scale.X, Scale.Y));
        return circle.Radius * Scale.X;
    }
}
