using System.Diagnostics;
using Godot;

public partial class Game : Node
{
    public static Game Instance { get; private set; }

    [Export]
    public LauncherSystem LauncherSystem { get; set; }

    [Export]
    public CardManager CardManager { get; set; }

    public GameEvents Events { get; private set; }

    private MainScene _mainScene;

    [Export]
    public uint Cash { get; set; } = 0;

    public override void _Ready()
    {
        Debug.Assert(LauncherSystem is not null);
        Debug.Assert(CardManager is not null);
    }

    public override void _EnterTree()
    {
        Instance = this;
        Events = new GameEvents();
    }

    public void RegisterMainScene(MainScene mainScene)
    {
        if (_mainScene != null) throw new System.Exception("MainScene already registered");
        _mainScene = mainScene;
    }

    public void UnregisterMainScene()
    {
        _mainScene = null;
    }

    public MainScene ExpectMainScene()
    {
        return _mainScene ?? throw new System.Exception("MainScene not registered");
    }

    public Hopper ExpectHopper()
    {
        return ExpectMainScene().ExpectHopper();
    }

    public Hopper GetSceneHopper()
    {
        return _mainScene?.Hopper;
    }

    public BallSource GetSceneBallSource()
    {
        return _mainScene?.BallSource;
    }
}
