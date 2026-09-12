using Godot;

[Tool]
[GlobalClass]
public partial class BallPackCardData : CardData
{
    public override bool CanApply(Node target) => target is Hopper;

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
