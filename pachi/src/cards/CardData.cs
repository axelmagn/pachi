using Godot;
using Godot.Collections;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class CardData : Resource
{
    [Export]
    public string Title { get; set; } = "Upgrade Card";

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "Drag onto target to apply upgrade.";

    [Export]
    public Color CardColor { get; set; } = new Color(0.2f, 0.4f, 0.8f);

    [Export]
    public Array<BallVariant> BonusBalls { get; set; } = [];

    [Export]
    public BallVariant? TargetTier { get; set; }

    [Export]
    public BallVariant? ResultTier { get; set; }

    [Export]
    public Array<BallVariant> PackBalls { get; set; } = [];

    public virtual bool CanApply(Node target) => false;

    public virtual bool Apply(Node target) => false;

    public virtual void PopulateCardUI(Control container)
    {
        if (TargetTier != null && ResultTier != null)
        {
            RenderTierTransitionIndicator(container, TargetTier, ResultTier, BonusBalls);
        }
        else if (PackBalls != null && PackBalls.Count > 0)
        {
            RenderPackIndicator(container, PackBalls, 18.0f);
        }
        else if (TargetTier != null)
        {
            RenderSingleTierIndicator(container, TargetTier, BonusBalls);
        }
        else if (BonusBalls != null && BonusBalls.Count > 0)
        {
            RenderPackIndicator(container, BonusBalls, 18.0f);
        }
    }

    protected void AwardBonusBalls()
    {
        if (BonusBalls == null || BonusBalls.Count == 0) return;

        Debug.Assert(GlobalEvents.Instance != null, "GlobalEvents.Instance must not be null when awarding bonus balls");
        foreach (var variant in BonusBalls)
        {
            if (variant != null)
            {
                GlobalEvents.Instance.NotifyBallAwarded(variant);
            }
        }
    }

    protected static void RenderPackIndicator(Control container, Array<BallVariant>? balls, float centerY = 18.0f)
    {
        if (container == null || balls == null || balls.Count == 0) return;

        PocketBallsIndicator packInd = new PocketBallsIndicator();
        packInd.IsCardIndicator = true;
        packInd.Balls = balls;
        float width = Mathf.Clamp(balls.Count * 10.0f + 8.0f, 30.0f, 90.0f);
        packInd.Size = new Vector2(width, 14.0f);
        packInd.Position = new Vector2(53.5f, centerY);
        container.AddChild(packInd);
    }

    protected static void RenderTierTransitionIndicator(Control container, BallVariant? sourceTier, BallVariant? resultTier, Array<BallVariant>? bonusBalls)
    {
        if (container == null) return;

        bool hasBonus = bonusBalls != null && bonusBalls.Count > 0;
        float topY = hasBonus ? 9.0f : 18.0f;
        float arrowY = hasBonus ? 3.0f : 7.0f;

        if (sourceTier != null)
        {
            PocketBallsIndicator sourceInd = new PocketBallsIndicator();
            sourceInd.IsCardIndicator = true;
            sourceInd.Balls = [sourceTier];
            sourceInd.Size = new Vector2(20.0f, 14.0f);
            sourceInd.Position = new Vector2(25.0f, topY);
            container.AddChild(sourceInd);
        }

        Label arrowLabel = new Label();
        arrowLabel.Text = "->";
        arrowLabel.Position = new Vector2(46.0f, arrowY);
        arrowLabel.AddThemeFontSizeOverride("font_size", 10);
        arrowLabel.AddThemeColorOverride("font_color", Colors.White);
        container.AddChild(arrowLabel);

        if (resultTier != null)
        {
            PocketBallsIndicator resInd = new PocketBallsIndicator();
            resInd.IsCardIndicator = true;
            resInd.Balls = [resultTier];
            resInd.Size = new Vector2(20.0f, 14.0f);
            resInd.Position = new Vector2(82.0f, topY);
            container.AddChild(resInd);
        }

        if (hasBonus)
        {
            RenderPackIndicator(container, bonusBalls, 27.0f);
        }
    }

    protected static void RenderSingleTierIndicator(Control container, BallVariant? tier, Array<BallVariant>? bonusBalls)
    {
        if (container == null) return;

        bool hasBonus = bonusBalls != null && bonusBalls.Count > 0;
        float topY = hasBonus ? 9.0f : 18.0f;

        if (tier != null)
        {
            PocketBallsIndicator sourceInd = new PocketBallsIndicator();
            sourceInd.IsCardIndicator = true;
            sourceInd.Balls = [tier];
            sourceInd.Size = new Vector2(20.0f, 14.0f);
            sourceInd.Position = new Vector2(53.5f, topY);
            container.AddChild(sourceInd);
        }

        if (hasBonus)
        {
            RenderPackIndicator(container, bonusBalls, 27.0f);
        }
    }
}

[Tool]
[GlobalClass]
public partial class BallPackCardData : CardData
{
    public override bool CanApply(Node target)
    {
        return target is Hopper;
    }

    public override bool Apply(Node target)
    {
        if (target is not Hopper hopper) return false;

        hopper.AddQueuedBalls(PackBalls);
        AwardBonusBalls();
        return true;
    }

    public override void PopulateCardUI(Control container)
    {
        if (PackBalls != null && PackBalls.Count > 0)
        {
            RenderPackIndicator(container, PackBalls, 18.0f);
        }
    }
}

[Tool]
[GlobalClass]
public partial class ModifyInputTierCardData : CardData
{
    public override bool CanApply(Node target)
    {
        return target is Pocket pocket && pocket.InputBalls != null && TargetTier != null && pocket.InputBalls.Contains(TargetTier);
    }

    public override bool Apply(Node target)
    {
        if (target is not Pocket pocket || !CanApply(pocket) || TargetTier == null || ResultTier == null || pocket.InputBalls == null) return false;

        int idx = pocket.InputBalls.IndexOf(TargetTier);
        if (idx < 0) return false;

        pocket.InputBalls[idx] = ResultTier;
        pocket.RefreshIndicatorAndSlots();
        AwardBonusBalls();
        return true;
    }

    public override void PopulateCardUI(Control container)
    {
        RenderTierTransitionIndicator(container, TargetTier, ResultTier, BonusBalls);
    }
}

[Tool]
[GlobalClass]
public partial class ModifyOutputTierCardData : CardData
{
    public override bool CanApply(Node target)
    {
        return target is Pocket pocket && pocket.OutputBalls != null && TargetTier != null && pocket.OutputBalls.Contains(TargetTier);
    }

    public override bool Apply(Node target)
    {
        if (target is not Pocket pocket || !CanApply(pocket) || TargetTier == null || ResultTier == null || pocket.OutputBalls == null) return false;

        int idx = pocket.OutputBalls.IndexOf(TargetTier);
        if (idx < 0) return false;

        pocket.OutputBalls[idx] = ResultTier;
        pocket.RefreshIndicatorAndSlots();
        AwardBonusBalls();
        return true;
    }

    public override void PopulateCardUI(Control container)
    {
        RenderTierTransitionIndicator(container, TargetTier, ResultTier, BonusBalls);
    }
}

[Tool]
[GlobalClass]
public partial class AddInputBallCardData : CardData
{
    [Export]
    public BallVariant? BallToAdd { get; set; }

    public override bool CanApply(Node target)
    {
        return target is Pocket pocket && (pocket.InputBalls == null || pocket.InputBalls.Count < 6);
    }

    public override bool Apply(Node target)
    {
        if (target is not Pocket pocket || !CanApply(pocket) || BallToAdd == null) return false;

        pocket.InputBalls ??= [];
        pocket.InputBalls.Add(BallToAdd);
        pocket.RefreshIndicatorAndSlots();
        AwardBonusBalls();
        return true;
    }

    public override void PopulateCardUI(Control container)
    {
        RenderSingleTierIndicator(container, BallToAdd, BonusBalls);
    }
}

[Tool]
[GlobalClass]
public partial class RemoveInputBallCardData : CardData
{
    [Export]
    public BallVariant? BallToRemove { get; set; }

    public override bool CanApply(Node target)
    {
        return target is Pocket pocket && pocket.InputBalls != null && pocket.InputBalls.Count > 1 && BallToRemove != null && pocket.InputBalls.Contains(BallToRemove);
    }

    public override bool Apply(Node target)
    {
        if (target is not Pocket pocket || !CanApply(pocket) || BallToRemove == null || pocket.InputBalls == null) return false;

        pocket.InputBalls.Remove(BallToRemove);
        pocket.RefreshIndicatorAndSlots();
        AwardBonusBalls();
        return true;
    }

    public override void PopulateCardUI(Control container)
    {
        RenderSingleTierIndicator(container, BallToRemove, BonusBalls);
    }
}

[Tool]
[GlobalClass]
public partial class AddOutputBallCardData : CardData
{
    [Export]
    public BallVariant? BallToAdd { get; set; }

    public override bool CanApply(Node target)
    {
        return target is Pocket pocket && (pocket.OutputBalls == null || pocket.OutputBalls.Count < 6);
    }

    public override bool Apply(Node target)
    {
        if (target is not Pocket pocket || !CanApply(pocket) || BallToAdd == null) return false;

        pocket.OutputBalls ??= [];
        pocket.OutputBalls.Add(BallToAdd);
        pocket.RefreshIndicatorAndSlots();
        AwardBonusBalls();
        return true;
    }

    public override void PopulateCardUI(Control container)
    {
        RenderSingleTierIndicator(container, BallToAdd, BonusBalls);
    }
}

[Tool]
[GlobalClass]
public partial class RemoveOutputBallCardData : CardData
{
    [Export]
    public BallVariant? BallToRemove { get; set; }

    public override bool CanApply(Node target)
    {
        return target is Pocket pocket && pocket.OutputBalls != null && pocket.OutputBalls.Count > 1 && BallToRemove != null && pocket.OutputBalls.Contains(BallToRemove);
    }

    public override bool Apply(Node target)
    {
        if (target is not Pocket pocket || !CanApply(pocket) || BallToRemove == null || pocket.OutputBalls == null) return false;

        pocket.OutputBalls.Remove(BallToRemove);
        pocket.RefreshIndicatorAndSlots();
        AwardBonusBalls();
        return true;
    }

    public override void PopulateCardUI(Control container)
    {
        RenderSingleTierIndicator(container, BallToRemove, BonusBalls);
    }
}
