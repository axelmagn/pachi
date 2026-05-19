using Godot;

public partial class Card : Control
{
    [Export]
    public CardEffect CardEffect { get; set; }

    [Export]
    public uint Price {get; set; } = 1;

    public override void _Ready()
    {
        GuiInput += OnGuiInput;
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event.IsActionPressed("mouse_select"))
        {
            Game.Instance.Events.EmitSignal(GameEvents.SignalName.CardClicked, this);
        }
    }
}
