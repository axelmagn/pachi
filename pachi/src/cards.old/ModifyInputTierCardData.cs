using Godot;

[Tool]
[GlobalClass]
public partial class ModifyInputTierCardData : CardData
{
    public override bool CanApply(Node target) =>
        target is Pocket pocket && pocket.InputBalls != null && TargetTier != null && pocket.InputBalls.Contains(TargetTier);

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
