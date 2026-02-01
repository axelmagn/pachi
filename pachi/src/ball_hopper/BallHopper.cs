using Godot;
using System;
using System.Diagnostics;

namespace Pachi
{
    public partial class BallHopper : Node2D
    {

        [Export]
        public int NumInitialBalls { get; set; } = 10;

        [Export]
        public PackedScene InitialBallScn { get; set; } = null;

        [Export]
        public BallSpawner BallSpawner { get; set; } = null;

        public override void _Ready()
        {
            Debug.Assert(InitialBallScn != null);
            Debug.Assert(BallSpawner != null);
            for (int i = 0; i < NumInitialBalls; i++)
            {
                Ball ball = InitialBallScn.Instantiate<Ball>();
                Debug.Assert(ball != null);
                BallSpawner.QueueSpawn(ball);
            }
        }
    }
}
