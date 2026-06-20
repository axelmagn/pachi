using Godot;
using System;

public partial class GamePhaseLabel : Label
{
	public override void _Process(double delta)
	{
        Text = Game.Instance.Phase.ToString();
	}
}

