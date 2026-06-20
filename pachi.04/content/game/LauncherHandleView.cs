using Godot;

public partial class LauncherHandleView : Node2D
{
    [Export]
    public float MinAngleDeg { get; set; } = 0.0f;

    [Export]
    public float MaxAngleDeg { get; set; } = 30.0f;

    public override void _Process(double delta)
    {
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (Game.Instance.LauncherSystem != null)
        {
            float progress = Game.Instance.LauncherSystem.GetProgress();
            float angle = MinAngleDeg + MaxAngleDeg * progress;
            RotationDegrees = angle;
        }
    }
}
