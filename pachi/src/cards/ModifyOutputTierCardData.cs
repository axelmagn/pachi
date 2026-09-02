using Godot;

[Tool]
[GlobalClass]
public partial class ModifyOutputTierCardData : CardData
{
    public override bool CanApply(Node target) =>
        target is Pocket pocket && pocket.OutputBalls != null && TargetTier != null && pocket.OutputBalls.Contains(TargetTier);

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
