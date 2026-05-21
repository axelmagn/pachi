using Godot;
using System;

public partial class CountdownLabel : Label
{
    public override void _Ready()
    {
        Game.Instance.PhaseChanged += OnGamePhaseChanged;
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;

        int seconds = Game.Instance.GetCountdownSecondsLeft();
        if (seconds >= 60)
        {
            int minutes = seconds / 60;
            seconds %= 60;

            Text = $"{minutes:D2}:{seconds:D2}";
        }
        else
        {
            Text = $"{seconds:D2}";

        }
    }

    public void OnGamePhaseChanged(Game.GamePhase newPhase)
    {
        switch (newPhase)
        {
            case Game.GamePhase.PrePlay:
            case Game.GamePhase.Play:
            case Game.GamePhase.PostPlay:
                Show();
                break;
            default:
                Hide();
                break;
        }
    }
}
