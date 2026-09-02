using Godot;

[Tool]
[GlobalClass]
public partial class RemoveOutputBallCardData : CardData
{
    [Export]
    public BallVariant? BallToRemove { get; set; }

    public override bool CanApply(Node target) =>
        target is Pocket pocket && pocket.OutputBalls != null && pocket.OutputBalls.Count > 1 && BallToRemove != null && pocket.OutputBalls.Contains(BallToRemove);

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
