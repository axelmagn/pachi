using Godot;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pachi
{
    public partial class BallSpawner : Area2D
    {
        [Signal]
        public delegate void BallSpawnedEventHandler(Ball ball);

        [Export]
        public Vector2 BaseVel { get; set; } = Vector2.Zero;
        [Export]
        public Vector2 RandomVel { get; set; } = Vector2.Zero;

        [Export]
        public Node2D BallParent { get; set; } = null;

        [Export]
        public Timer CooldownTimer { get; set; } = null;

        private readonly Queue<Ball> spawnQueue = new();

        private bool isCoolingDown = false;

        private RandomNumberGenerator rng = new();

        public override void _Ready()
        {
            BallParent ??= GetParent<Node2D>();
            Debug.Assert(BallParent != null);

            Debug.Assert(CooldownTimer != null);

            BodyExited += (body) => TrySpawn();
            CooldownTimer.Timeout += FinishCooldown;
        }

        public void SpawnImmediately(Ball ball)
        {
            ball.GetParent()?.RemoveChild(ball);
            ball.Position = Position;
            Vector2 vel = RandomVel;
            vel.X *= rng.RandfRange(-1, 1);
            vel.Y *= rng.RandfRange(-1, 1);
            vel += BaseVel;
            ball.LinearVelocity = vel;
            ball.ResetPhysicsInterpolation();
            Callable.From(() =>
            {
                BallParent.AddChild(ball);
                EmitSignalBallSpawned(ball);
            }).CallDeferred();
            StartCooldown();
        }

        public void QueueSpawn(Ball ball)
        {
            spawnQueue.Enqueue(ball);
            TrySpawn();
        }

        private void TrySpawn()
        {
            if (HasOverlappingBodies())
            {
                // GD.Print("\tOVERLAP");
                return;
            }

            if (spawnQueue.Count == 0)
            {
                // GD.Print("\tEMPTY");
                return;
            }

            if (isCoolingDown)
            {
                // GD.Print("\tCOOLDOWN");
                return;
            }

            // GD.Print("SPAWN");
            Ball ball = spawnQueue.Dequeue();
            SpawnImmediately(ball);
        }

        private void StartCooldown()
        {
            isCoolingDown = true;
            CooldownTimer.Start();
        }

        private void FinishCooldown()
        {
            isCoolingDown = false;
            TrySpawn();
        }
    }
}
