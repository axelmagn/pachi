using System.Diagnostics;
using Godot;

public partial class CardManager : Node
{
    [Signal]
    public delegate void CardDragStartedEventHandler(Card card);

    [Signal]
    public delegate void CardDragStoppedEventHandler(Card card);

    [Signal]
    public delegate void CardDragSucceededEventHandler(Card card);

    [Signal]
    public delegate void CardDragCancelledEventHandler(Card card);

    [Export]
    public CanvasLayer CardDragLayer { get; set; }

    public bool DragActive { get; set; } = false;
    public Card DraggedCardProxy;
    public Card DraggedCardOriginal;

    // temporary callback variable when card drag drop is completed
    private bool _cardEffectApplied = false;

    public override void _Ready()
    {
        Debug.Assert(CardDragLayer != null, "CardDragLayer is not assigned!");
        Game.Instance.Events.CardClicked += OnCardClicked;
    }

    public override void _Input(InputEvent @event)
    {
        if (DragActive && @event is InputEventMouseMotion mouseMotionEvent)
        {
            DraggedCardProxy.GlobalPosition =
                mouseMotionEvent.Position - DraggedCardProxy.Size * 0.5f;
        }
        else if (DragActive && @event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.IsActionReleased("mouse_select"))
            {
                // TODO: handle dragged card release.  for now just hide it
                DraggedCardProxy.QueueFree();
                StopDraggingCard();
            }
        }
    }

    public void NotifyCardEffectApplied()
    {
        _cardEffectApplied = true;
    }

    private void OnCardClicked(Card card)
    {
        if (DragActive) return;
        StartDraggingCard(card);
    }

    private void StartDraggingCard(Card card)
    {
        Debug.Assert(!DragActive, "card drag is already active");
        DragActive = true;
        DraggedCardOriginal = card;
        DraggedCardProxy = (Card)card.Duplicate();
        CardDragLayer.AddChild(DraggedCardProxy);
        DraggedCardProxy.GlobalPosition =
            DraggedCardProxy.GetGlobalMousePosition() - DraggedCardProxy.Size * 0.5f;
        EmitSignalCardDragStarted(card);
    }

    private void StopDraggingCard()
    {
        _cardEffectApplied = false;
        EmitSignalCardDragStopped(DraggedCardOriginal);
        Debug.Assert(DragActive, "card drag is already active");
        if (_cardEffectApplied) {
            EmitSignalCardDragSucceeded(DraggedCardOriginal);
        } else {
            EmitSignalCardDragCancelled(DraggedCardOriginal);
        }
        DragActive = false;
        DraggedCardOriginal = null;
        DraggedCardProxy.QueueFree();
        DraggedCardProxy = null;
    }
}
