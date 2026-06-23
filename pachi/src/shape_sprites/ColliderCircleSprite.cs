using Godot;

/// Circle sprite that matches the radius of its parent.  Continuously updates radius in editor,
/// but must be updated manually during runtime.
[Tool]
[GlobalClass]
public partial class ColliderCircleSprite : CircleSprite
{
    public override void _Ready()
    {
        SyncRadiusWithParent();

        // only run Process if we are in the editor - otherwise assume that collider shape will not
        // change.
        SetProcess(Engine.IsEditorHint());
    }

    public override void _Process(double delta)
    {
        SyncRadiusWithParent();
    }

    private void SyncRadiusWithParent()
    {
        Node parent = GetParent();

        if (parent is not CollisionShape2D { Shape: CircleShape2D circleShape })
            return;

        if (Radius == circleShape.Radius)
            return;

        Radius = circleShape.Radius;
    }
}
