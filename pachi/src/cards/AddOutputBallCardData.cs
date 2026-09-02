using Godot;

[Tool]
[GlobalClass]
public partial class AddOutputBallCardData : CardData
{
    [Export]
    public BallVariant? BallToAdd { get; set; }

    public override bool CanApply(Node target) =>
        target is Pocket pocket && (pocket.OutputBalls == null || pocket.OutputBalls.Count < Pocket.MaxOutputCapacity);

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
