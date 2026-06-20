using System.Diagnostics;
using Godot;

[GlobalClass]
public partial class AddBallsHopperCardEffect : HopperCardEffect
{
    [Export]
    PackedScene BallScene;

    [Export]
    int NumBalls;

    protected override void ApplyHopperCardEffect(Hopper hopper)
    {
        Debug.Assert(BallScene is not null, "BallScene is unassigned");
        for (int i = 0; i < NumBalls; i++)
        {
            Ball ball = BallScene.Instantiate<Ball>();
            hopper.AddBall(ball);
        }
    }
}
