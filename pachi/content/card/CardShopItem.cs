using Godot;
using System;
using System.Diagnostics;

public partial class CardShopItem : PanelContainer
{
    [Export]
    public Card Card { get; set; }

    [Export]
    public Label PriceLabel { get; set; }

    [Export]
    public Color EnabledModulate { get; set; } = Color.Color8(255, 255, 255, 255);

    [Export]
    public Color DisabledModulate { get; set; } = Color.Color8(64, 64, 64, 255);

    private MouseBehaviorRecursiveEnum _originalMouseBehavior;

    public override void _Ready()
    {
        Debug.Assert(Card is not null);
        Debug.Assert(PriceLabel is not null);
        UpdateVisuals();

        _originalMouseBehavior = MouseBehaviorRecursive;

        Game.Instance.CardManager.CardDragStarted += OnDragStarted;
        Game.Instance.CardManager.CardDragCancelled += OnDragCancelled;
    }

    private void UpdateVisuals()
    {
        PriceLabel.Text = $"${Card.Price}";
    }

    public void Enable()
    {
        Modulate = EnabledModulate;
        MouseBehaviorRecursive = _originalMouseBehavior;
    }

    public void Disable()
    {
        Modulate = DisabledModulate;
        MouseBehaviorRecursive = MouseBehaviorRecursiveEnum.Disabled;
    }

    private void OnDragStarted(Card card)
    {
        if (card == Card)
        {
            Disable();

        }
    }

    private void OnDragSucceeded(Card card) {
        if (card == Card) {
            Game.Instance.Cash -= Card.Price;
        }

    }

    private void OnDragCancelled(Card card)
    {
        if (card == Card)
        {
            Enable();
        }
    }

}
