using Godot;
using System;
using System.Diagnostics;

public partial class CardShopItem : PanelContainer
{
    public enum VisualState { Enabled, Disabled, Empty };

    [Export]
    public Card Card { get; set; }

    [Export]
    public Label PriceLabel { get; set; }

    [Export]
    public Color EnabledModulate { get; set; } = Color.Color8(255, 255, 255, 255);

    [Export]
    public Color DisabledModulate { get; set; } = Color.Color8(64, 64, 64, 255);

    [Export]
    public Color EmptyModulate { get; set; } = Color.Color8(255, 255, 255, 0);

    private MouseBehaviorRecursiveEnum _originalMouseBehavior;
    private VisualState _visualState;

    public override void _Ready()
    {
        Debug.Assert(Card is not null);
        Debug.Assert(PriceLabel is not null);
        UpdateVisuals();


        Game.Instance.CashChanged += OnCashChanged;
        Game.Instance.CardManager.CardDragStarted += OnDragStarted;
        Game.Instance.CardManager.CardDragSucceeded += OnDragSucceeded;
        Game.Instance.CardManager.CardDragCancelled += OnDragCancelled;

        UpdateVisualStateNonEmpty();
    }

    private void UpdateVisuals()
    {
        PriceLabel.Text = $"${Card.Price}";
    }

    public void SetVisualState(VisualState visualState)
    {
        _visualState = visualState;

        switch (visualState)
        {
            case VisualState.Enabled:
                Modulate = EnabledModulate;
                MouseBehaviorRecursive = _originalMouseBehavior;
                break;
            case VisualState.Disabled:
                Modulate = DisabledModulate;
                _originalMouseBehavior = MouseBehaviorRecursive;
                MouseBehaviorRecursive = MouseBehaviorRecursiveEnum.Disabled;
                break;
            case VisualState.Empty:
                Modulate = EmptyModulate;
                _originalMouseBehavior = MouseBehaviorRecursive;
                MouseBehaviorRecursive = MouseBehaviorRecursiveEnum.Disabled;
                break;
        }

    }

    private void UpdateVisualStateNonEmpty()
    {
        VisualState visualState = Card.CanAfford() ? VisualState.Enabled : VisualState.Disabled;
        SetVisualState(visualState);
    }

    private void OnDragStarted(Card card)
    {
        if (card == Card)
        {
            SetVisualState(VisualState.Empty);
        }
    }

    private void OnDragSucceeded(Card card)
    {
        if (card == Card)
        {
            Game.Instance.Cash -= Card.Price;
        }
    }

    private void OnDragCancelled(Card card)
    {
        if (card == Card)
        {
            UpdateVisualStateNonEmpty();
        }
    }

    private void OnCashChanged(uint newCash)
    {
        if (_visualState != VisualState.Empty)
        {
            UpdateVisualStateNonEmpty();
        }
    }

}
