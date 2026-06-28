using Godot;

/// Capsule sprite that matches the radius and height of its parent. Continuously updates in editor,
/// but must be updated manually during runtime.
[Tool]
[GlobalClass]
public partial class ColliderCapsuleSprite : CapsuleSprite
{
    public override void _Ready()
    {
        SyncCapsuleWithParent();

        // only run Process if we are in the editor - otherwise assume that collider shape will not
        // change.
        SetProcess(Engine.IsEditorHint());
    }

    public override void _Process(double delta)
    {
        SyncCapsuleWithParent();
    }

    private void SyncCapsuleWithParent()
    {
        Node parent = GetParent();

        if (parent is not CollisionShape2D { Shape: CapsuleShape2D capsuleShape })
            return;

        if (Radius == capsuleShape.Radius && Height == capsuleShape.Height)
            return;

        Radius = capsuleShape.Radius;
        Height = capsuleShape.Height;
    }
}
