using Godot;

public partial class BuyBallsButton : Button
{
    [Export]
    public uint Price { get; set; }

    [Export]
    public uint Balls { get; set; }

    [Export]
    public string TextFmt { get; set; } = "Buy {balls} balls\n(${price})";

    public override void _Ready()
    {
        Pressed += OnPressed;

        string text = TextFmt.Replace("{price}", Price.ToString()).Replace("{balls}", Balls.ToString());
        Text = text;
    }

    public override void _Process(double delta)
    {
        Disabled = Price > Game.Instance.Cash;
    }

    private void OnPressed()
    {
        if (Game.Instance.Cash < Price) return;

        Game.Instance.Cash -= Price;
        Game.Instance.Events.EmitSignal(GameEvents.SignalName.AddDefaultBalls, (int)Balls);
    }
}
