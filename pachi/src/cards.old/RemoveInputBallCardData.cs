using Godot;

[Tool]
[GlobalClass]
public partial class RemoveInputBallCardData : CardData
{
    [Export]
    public BallVariant? BallToRemove { get; set; }

    public override bool CanApply(Node target) =>
        target is Pocket pocket && pocket.InputBalls != null && pocket.InputBalls.Count > 1 && BallToRemove != null && pocket.InputBalls.Contains(BallToRemove);

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
