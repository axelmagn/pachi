using Godot;

[Tool]
[GlobalClass]
public partial class AddInputBallCardData : CardData
{
    [Export]
    public BallVariant? BallToAdd { get; set; }

    public override bool CanApply(Node target) =>
        target is Pocket pocket && (pocket.InputBalls == null || pocket.InputBalls.Count < Pocket.MaxInputCapacity);

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
