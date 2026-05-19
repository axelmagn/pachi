using Godot;

public partial class Card : Control
{
    [Export]
    public Label Title { get; set; }

    [Export]
    public RichTextLabel Description { get; set; }

    public override void _Ready()
    {
        GuiInput += OnGuiInput;
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event.IsActionPressed("mouse_select"))
        {
            GD.Print("card clicked!");
            Game.Instance.Events.EmitSignal(GameEvents.SignalName.CardClicked, this);
        }
    }
}
