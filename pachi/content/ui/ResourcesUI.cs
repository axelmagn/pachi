using Godot;

public partial class ResourcesUI : Control
{
    [Export]
    public Label CashLabel { get; set; }

    public override void _Process(double delta)
    {
        if (CashLabel != null)
        {
            CashLabel.Text = Game.Instance.Cash.ToString();
        }
    }
}
