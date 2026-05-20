using Godot;
using System.Diagnostics;

public partial class CardEffectControlReceiver : Control
{
    [Export]
    public Node EffectSubject { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Debug.Assert(EffectSubject is not null);

        CardManager cardManager = Game.Instance.CardManager;
        cardManager.CardDragStarted += OnDragStarted;
        cardManager.CardDragStopped += OnDragStopped;

        Hide();
    }

    public void OnDragStarted(Card card)
    {
        Debug.Assert(EffectSubject is not null);

        CardEffect cardEffect = card.CardEffect;

        if (cardEffect is HopperCardEffect && EffectSubject is Hopper)
        {
            Show();
        }
        // TODO: more effect types
    }

    public void OnDragStopped(Card card)
    {
        Debug.Assert(EffectSubject is not null);

        if (Visible) Hide();

        bool dragReceived = GetGlobalRect().HasPoint(GetGlobalMousePosition());
        if (dragReceived) card.CardEffect.ApplyCardEffect(EffectSubject);
    }
}
