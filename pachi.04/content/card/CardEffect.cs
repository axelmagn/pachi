using Godot;

public abstract partial class CardEffect : Resource
{
    public void ApplyCardEffect(Node subject)
    {
        ApplyCardEffectInner(subject);
        Game.Instance.CardManager.NotifyCardEffectApplied();
    }

    protected abstract void ApplyCardEffectInner(Node subject);
}
