using Godot;

public partial class SellBallsButton : Button
{
    [Export]
    public uint Price { get; set; }

    [Export]
    public uint Balls { get; set; }

    [Export]
    public string TextFmt { get; set; } = "Sell {balls} balls\n(${price})";

    public override void _Ready()
    {
        Pressed += OnPressed;

        string text = TextFmt.Replace("{price}", Price.ToString()).Replace("{balls}", Balls.ToString());
        Text = text;
    }

    public override void _Process(double delta)
    {
        Hopper hopper = Game.Instance.GetSceneHopper();
        if (hopper != null)
        {
            Disabled = Balls > hopper.GetBallCount();
        }
        else
        {
            Disabled = true;
        }
    }

    private void OnPressed()
    {
        Hopper hopper = Game.Instance.GetSceneHopper();
        if (hopper == null || hopper.GetBallCount() < Balls) return;

        Game.Instance.Cash += Price;
        hopper.DestroyBalls((int)Balls);
    }
}
