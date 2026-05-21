using System;
using System.Diagnostics;
using Godot;

public partial class Game : Node
{
    public enum GamePhase
    {
        None,
        /// waiting for input before play starts
        PrePlay,
        /// play has started.  timer is counting down
        Play,
        /// play has finished.  do scoring animations
        PostPlay,
        /// waiting for input before play starts
        Shop,
    };

    public static Game Instance { get; private set; }

    [Signal]
    public delegate void CashChangedEventHandler(uint newCash);

    [Signal]
    public delegate void PhaseChangedEventHandler(GamePhase newPhase);

    [Export]
    public LauncherSystem LauncherSystem { get; set; }

    [Export]
    public CardManager CardManager { get; set; }

    [Export]
    public Timer CountdownTimer { get; set; }

    [Export]
    public GamePhase Phase
    {
        get => _phase;
        set
        {
            _phase = value;
            // TODO: emit signal
        }
    }
    private GamePhase _phase = GamePhase.None;


    [Export]
    public uint Cash
    {
        get => _cash;
        set
        {
            _cash = value;
            EmitSignalCashChanged(_cash);
        }
    }
    private uint _cash = 0;

    public GameEvents Events { get; private set; }

    private MainScene _mainScene;

    public override void _Ready()
    {
        Debug.Assert(LauncherSystem is not null);
        Debug.Assert(CardManager is not null);
        Debug.Assert(CountdownTimer is not null);

        CountdownTimer.Timeout += OnCountdownTimeout;
        LauncherSystem.BallLaunched += OnBallLaunched;
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
        Debug.Assert(_mainScene != null);
        return _mainScene.Hopper;
    }

    public BallSource GetSceneBallSource()
    {
        Debug.Assert(_mainScene != null);
        return _mainScene.BallSource;
    }

    public void PhaseTransition(GamePhase newPhase)
    {
        // TODO: state transition handling

        // update countdown timer
        switch (newPhase)
        {
            case GamePhase.Play:
                CountdownTimer.Start();
                break;
            default:
                CountdownTimer.Stop();
                break;
        }
        Phase = newPhase;
    }

    public int GetCountdownSecondsLeft()
    {
        Debug.Assert(CountdownTimer != null);
        double remaining = Phase switch {
            GamePhase.Play => CountdownTimer.TimeLeft,
            GamePhase.PostPlay => CountdownTimer.TimeLeft,
            _ => CountdownTimer.WaitTime,
        };
        return (int)Math.Ceiling(remaining);
    }

    private void OnCountdownTimeout()
    {
        PhaseTransition(GamePhase.PostPlay);
    }

    private void OnBallLaunched(Ball ball)
    {
        if (Phase == GamePhase.PrePlay) {
            PhaseTransition(GamePhase.Play);
        }
    }
}
