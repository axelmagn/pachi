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
    public delegate void PhaseExitedEventHandler(GamePhase oldPhase);

    [Signal]
    public delegate void PhaseEnteredEventHandler(GamePhase newPhase);

    [Export]
    public LauncherSystem LauncherSystem { get; set; }

    [Export]
    public CardManager CardManager { get; set; }

    [Export]
    public Timer CountdownTimer { get; set; }

    [Export]
    public Timer PostPlayTimer { get; set; }

    [Export]
    public Godot.Collections.Array<BallTier> BallTiers { get; set; }

    [Export]
    public GamePhase Phase
    {
        get => _phase;
        set
        {
            if (_phase != value)
            {
                GamePhase oldPhase = _phase;
                _phase = value;
                EmitSignalPhaseExited(oldPhase);
                EmitSignalPhaseEntered(_phase);
            }
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
        Debug.Assert(PostPlayTimer is not null);

        CountdownTimer.Timeout += OnCountdownTimeout;
        LauncherSystem.BallLaunched += OnBallLaunched;
        PostPlayTimer.Timeout += OnPostPlayTimeout;

        // We expect BallTiers to be empty or null when they have not been configured manually inside
        // the Game scene (game.tscn) via the inspector. In this case, we load default tiers from files.
        if (BallTiers == null || BallTiers.Count == 0)
        {
            BallTiers = [];
            for (int i = 1; i <= 16; i++)
            {
                var tier = GD.Load<BallTier>($"res://content/ball/tiers/tier_{i}.tres");
                // We expect tier to be null only if the specific tier file is missing or renamed in the project.
                if (tier != null)
                {
                    BallTiers.Add(tier);
                }
            }
        }
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
        // manage countdown timer
        if (newPhase == GamePhase.Play)
        {
            CountdownTimer.Start();
        }
        else
        {
            CountdownTimer.Stop();
        }

        // manage postplay timer
        if (newPhase == GamePhase.PostPlay)
        {
            PostPlayTimer.Start();
        }
        else
        {
            PostPlayTimer.Stop();
        }

        // TODO: transition animations
        Phase = newPhase;
    }

    public int GetCountdownSecondsLeft()
    {
        Debug.Assert(CountdownTimer != null);
        double remaining = Phase switch
        {
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
        switch (Phase)
        {
            case GamePhase.PrePlay:
            case GamePhase.Shop:
                PhaseTransition(GamePhase.Play);
                break;
            default:
                break;

        }
    }

    private void OnPostPlayTimeout()
    {
        // TODO: finish postplay when animations are done rather than timer
        PhaseTransition(GamePhase.Shop);
    }
}
