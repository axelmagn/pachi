using Godot;

public partial class MainScene : Node
{
    [Export]
    public Hopper Hopper { get; set; }

    [Export]
    public BallSource BallSource { get; set; }

    public override void _EnterTree()
    {
        Game.Instance.RegisterMainScene(this);
    }

    public override void _ExitTree()
    {
        Game.Instance.UnregisterMainScene();
    }

    public Hopper ExpectHopper()
    {
        if (Hopper == null) throw new System.Exception("Hopper is null");
        return Hopper;
    }
}
