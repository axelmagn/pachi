using Godot;

[Tool]
public partial class DynamicBoundary : StaticBody2D
{
    private Vector2 _boundarySize = new Vector2(1080, 1080);

    [Export]
    public Vector2 BoundarySize
    {
        get => _boundarySize;
        set
        {
            _boundarySize = value;
            PlaceBoundaries();
        }
    }

    [Export]
    public CollisionShape2D TopBoundary { get; set; }

    [Export]
    public CollisionShape2D BottomBoundary { get; set; }

    [Export]
    public CollisionShape2D LeftBoundary { get; set; }

    [Export]
    public CollisionShape2D RightBoundary { get; set; }

    public override void _Ready()
    {
        PlaceBoundaries();
    }

    private void PlaceBoundaries()
    {
        if (_boundarySize.X <= 0 || _boundarySize.Y <= 0) return;

        if (TopBoundary != null)
        {
            TopBoundary.Position = new Vector2(0, -_boundarySize.Y / 2.0f);
        }

        if (BottomBoundary != null)
        {
            BottomBoundary.Position = new Vector2(0, _boundarySize.Y / 2.0f);
        }

        if (LeftBoundary != null)
        {
            LeftBoundary.Position = new Vector2(-_boundarySize.X / 2.0f, 0);
        }

        if (RightBoundary != null)
        {
            RightBoundary.Position = new Vector2(_boundarySize.X / 2.0f, 0);
        }
    }
}
