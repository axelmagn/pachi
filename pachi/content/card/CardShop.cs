using Godot;
using System;

public partial class CardShop : Control
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Game.Instance.PhaseChanged += OnGamePhaseChanged;

        UpdateVisuals(Game.Instance.Phase);
    }

    public void UpdateVisuals(Game.GamePhase gamePhase)
    {
        if (gamePhase == Game.GamePhase.Shop)
        {
            Show();
            GD.Print("SHOW SHOP");
        }
        else
        {
            Hide();
        }

        // TODO: animations
    }

    private void OnGamePhaseChanged(Game.GamePhase newPhase)
    {
        UpdateVisuals(newPhase);
    }


}
