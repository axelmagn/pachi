using Godot;

public partial class CardManager : Node
{
    [Export]
    public Card DraggedCardProxy { get; set; }

    public override void _Ready()
    {
        // if (DraggedCardProxy == null) GD.PushError("CardManager: DraggedCardProxy is not assigned!");

        Game.Instance.Events.CardClicked += OnCardClicked;
    }

    private void OnCardClicked(Card card)
    {
        GD.Print($"card click detected by manager: {card.Name}");
    }
}
