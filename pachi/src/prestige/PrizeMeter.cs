using Godot;
using System;

[GlobalClass]
public partial class PrizeMeter : Node
{
    [Signal]
    public delegate void ProgressChangedEventHandler(float currentProgress, float targetCapacity, float percent);

    [Signal]
    public delegate void PrizeTokenAwardedEventHandler(int totalTokens, int tokensInRun);

    [Signal]
    public delegate void MeterResetEventHandler();

    [Export]
    public float BaseTarget { get; set; } = 100.0f;

    [Export]
    public float ScalingMultiplier { get; set; } = 1.50f;

    [Export]
    public float YakumonoBonusScore { get; set; } = 25.0f;

    [Export]
    public bool IsActive { get; set; } = true;

    public float CurrentProgress { get; private set; } = 0.0f;

    public int TokensEarnedInRun { get; private set; } = 0;

    public int TotalTokens { get; private set; } = 0;

    public float CurrentTargetCapacity => BaseTarget * Mathf.Pow(ScalingMultiplier, TokensEarnedInRun);

    public float ProgressPercent => CurrentTargetCapacity > 0.0f
        ? Mathf.Clamp(CurrentProgress / CurrentTargetCapacity, 0.0f, 1.0f)
        : 0.0f;

    public bool CanPrestigeReset => TotalTokens >= 1 || TokensEarnedInRun >= 1;

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

    public void AddScore(float points)
    {
        if (points <= 0.0f)
        {
            return;
        }

        CurrentProgress += points;

        while (CurrentProgress >= CurrentTargetCapacity && CurrentTargetCapacity > 0.0f)
        {
            CurrentProgress -= CurrentTargetCapacity;
            TokensEarnedInRun++;
            TotalTokens++;
            EmitSignal(SignalName.PrizeTokenAwarded, TotalTokens, TokensEarnedInRun);
        }

        EmitSignal(SignalName.ProgressChanged, CurrentProgress, CurrentTargetCapacity, ProgressPercent);
    }

    public float GetTierScoreValue(BallVariant? variant)
    {
        if (variant == null)
        {
            return 1.0f;
        }

        return variant.Tier switch
        {
            1 => 1.0f,
            2 => 3.0f,
            3 => 10.0f,
            4 => 50.0f,
            5 => 100.0f,
            6 => 250.0f,
            _ => variant.Tier > 0 ? (float)variant.Tier : 1.0f
        };
    }

    public void ResetRunState()
    {
        CurrentProgress = 0.0f;
        TokensEarnedInRun = 0;
        EmitSignal(SignalName.ProgressChanged, CurrentProgress, CurrentTargetCapacity, ProgressPercent);
        EmitSignal(SignalName.MeterReset);
    }

    public void ResetAll()
    {
        TotalTokens = 0;
        ResetRunState();
    }

    public void SpendTokens(int amount)
    {
        if (amount <= 0 || amount > TotalTokens)
        {
            return;
        }

        TotalTokens -= amount;
        EmitSignal(SignalName.ProgressChanged, CurrentProgress, CurrentTargetCapacity, ProgressPercent);
    }

    private void OnGlobalBallEnteredPocket(Node pocket, Node ball)
    {
        if (!IsActive)
        {
            return;
        }

        BallVariant? variant = null;
        if (ball is Ball ballNode)
        {
            variant = ballNode.Variant;
        }

        float score = GetTierScoreValue(variant);
        AddScore(score);
    }

    private void OnGlobalYakumonoPaidOut(Node yakumono)
    {
        if (!IsActive)
        {
            return;
        }

        AddScore(YakumonoBonusScore);
    }
}
