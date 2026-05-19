using Godot;

public abstract partial class HopperCardEffect : CardEffect
{
    protected override void ApplyCardEffectInner(Node subject)
    {
        if (subject is Hopper hopper)
        {
            ApplyHopperCardEffect(hopper);
        }
        else
        {
            GD.PushError("Cannot apply hopper card effect to non-hopper node");
        }
    }

    protected abstract void ApplyHopperCardEffect(Hopper hopper);
}
