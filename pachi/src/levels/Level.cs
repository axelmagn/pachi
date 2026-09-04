using Godot;
using System.Diagnostics;

[GlobalClass]
public partial class Level : Node2D
{
	[Export]
	public Node2D? BallsRoot { get; set; }

	// TODO: create launch point class
	[Export]
	public Node2D? BallLaunchPoint { get; set; }

	public override void _Ready()
	{
		Debug.Assert(BallsRoot != null);
		Debug.Assert(BallLaunchPoint != null);
	}
}
