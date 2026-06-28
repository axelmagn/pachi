using Godot;

/// Rectangle sprite that matches the size of its parent. Continuously updates size in editor,
/// but must be updated manually during runtime.
[Tool]
[GlobalClass]
public partial class ColliderRectSprite : RectSprite
{
    public override void _Ready()
    {
        SyncSizeWithParent();

        // only run Process if we are in the editor - otherwise assume that collider shape will not
        // change.
        SetProcess(Engine.IsEditorHint());
    }

    public override void _Process(double delta)
    {
        SyncSizeWithParent();
    }

    private void SyncSizeWithParent()
    {
        Node parent = GetParent();

        if (parent is not CollisionShape2D { Shape: RectangleShape2D rectShape })
            return;

        if (Size == rectShape.Size)
            return;

        Size = rectShape.Size;
    }
}
