using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class DealMeter : Node
{
    [Signal]
    public delegate void ProgressChangedEventHandler(float progress, float effectiveRateMultiplier);

    [Signal]
    public delegate void DealThresholdReachedEventHandler();

    [Export]
    public float BaselinePeriod { get; set; } = 20.0f;

    [Export]
    public float PocketBoostChunk { get; set; } = 0.10f;

    [Export]
    public float PocketSpeedMultiplier { get; set; } = 0.5f;

    [Export]
    public float YakumonoBoostChunk { get; set; } = 0.35f;

    [Export]
    public float YakumonoSpeedMultiplier { get; set; } = 2.0f;

    [Export]
    public float BoostDuration { get; set; } = 5.0f;

    [Export]
    public bool IsActive { get; set; } = true;

    public float Progress { get; private set; } = 0.0f;

    public float EffectiveRateMultiplier
    {
        get
        {
            float total = 1.0f;
            foreach (SpeedBoost boost in _activeBoosts)
            {
                total += boost.Multiplier;
            }
            return total;
        }
    }

    public float ActiveFillRate => (BaselinePeriod > 0.0f ? (1.0f / BaselinePeriod) : 0.0f) * EffectiveRateMultiplier;

    private readonly List<SpeedBoost> _activeBoosts = new();

    private sealed class SpeedBoost
    {
        public float Multiplier { get; }
        public float TimeRemaining { get; set; }

        public SpeedBoost(float multiplier, float duration)
        {
            Multiplier = multiplier;
            TimeRemaining = duration;
        }
    }

    public override void _Ready()
    {
        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.BallEnteredPocket += OnGlobalBallEnteredPocket;
            GlobalEvents.Instance.YakumonoPaidOut += OnGlobalYakumonoPaidOut;
        }
    }

    public override void _ExitTree()
    {
        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.BallEnteredPocket -= OnGlobalBallEnteredPocket;
            GlobalEvents.Instance.YakumonoPaidOut -= OnGlobalYakumonoPaidOut;
        }
    }

    public override void _Process(double delta)
    {
        if (!IsActive)
        {
            return;
        }

        Advance(delta);
    }

    public void Advance(double delta)
    {
        float dt = (float)delta;
        if (dt <= 0.0f)
        {
            return;
        }

        // Update active speed boost timers
        for (int i = _activeBoosts.Count - 1; i >= 0; i--)
        {
            _activeBoosts[i].TimeRemaining -= dt;
            if (_activeBoosts[i].TimeRemaining <= 0.0f)
            {
                _activeBoosts.RemoveAt(i);
            }
        }

        float fillDelta = ActiveFillRate * dt;
        AddProgress(fillDelta);
    }

    public void AddProgress(float amount)
    {
        if (amount <= 0.0f)
        {
            return;
        }

        Progress += amount;
        if (Progress >= 1.0f)
        {
            Progress = 0.0f;
            EmitSignal(SignalName.ProgressChanged, Progress, EffectiveRateMultiplier);
            EmitSignal(SignalName.DealThresholdReached);
        }
        else
        {
            EmitSignal(SignalName.ProgressChanged, Progress, EffectiveRateMultiplier);
        }
    }

    public void AddSpeedMultiplier(float multiplier, float duration)
    {
        if (multiplier <= 0.0f || duration <= 0.0f)
        {
            return;
        }

        _activeBoosts.Add(new SpeedBoost(multiplier, duration));
        EmitSignal(SignalName.ProgressChanged, Progress, EffectiveRateMultiplier);
    }

    public void AddPocketHit()
    {
        AddProgress(PocketBoostChunk);
        AddSpeedMultiplier(PocketSpeedMultiplier, BoostDuration);
    }

    public void AddYakumonoHit()
    {
        AddProgress(YakumonoBoostChunk);
        AddSpeedMultiplier(YakumonoSpeedMultiplier, BoostDuration);
    }

    public void ResetProgress()
    {
        Progress = 0.0f;
        _activeBoosts.Clear();
        EmitSignal(SignalName.ProgressChanged, Progress, EffectiveRateMultiplier);
    }

    private void OnGlobalBallEnteredPocket(Node pocket, Node ball)
    {
        if (IsActive)
        {
            AddPocketHit();
        }
    }

    private void OnGlobalYakumonoPaidOut(Node yakumono)
    {
        if (IsActive)
        {
            AddYakumonoHit();
        }
    }
}
