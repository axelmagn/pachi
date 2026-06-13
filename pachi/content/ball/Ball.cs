using System.Diagnostics;
using Godot;

public partial class Ball : RigidBody2D
{
    private BallTier _tier;

    [Export]
    public BallTier Tier
    {
        get => _tier;
        set
        {
            _tier = value;
            UpdateVisuals();
        }
    }

    public float BasePrice => _tier?.BasePrice ?? 1.0f;

    public override void _Ready()
    {
        Debug.Assert(_tier != null);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        Modulate = _tier.Color;
    }
}
