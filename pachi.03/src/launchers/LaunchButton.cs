using Godot;
using System.Diagnostics;
using System.Collections.Generic;

namespace Pachi
{
    public partial class LaunchButton : Button
    {
        [Export]
        public Area2D InputArea { get; set; } = null;

        [Export]
        public BallSpawner Spawner { get; set; } = null;

        private Queue<Ball> LaunchQueue { get; set; } = new();

        public override void _Ready()
        {
            Debug.Assert(InputArea != null);
            Debug.Assert(Spawner != null);

            Pressed += Launch;
            InputArea.BodyEntered += OnBodyEnteredInputArea;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("launch_ball") && !@event.IsEcho())
            {
                Launch();
            }
        }

        public void Launch()
        {
            Debug.Assert(InputArea != null);
            Debug.Assert(Spawner != null);

            while (LaunchQueue.Count > 0)
            {
                Ball ball = LaunchQueue.Dequeue();
                if (InputArea.OverlapsBody(ball))
                {
                    Spawner.QueueSpawn(ball);
                    return;
                }
            }
        }

        private void OnBodyEnteredInputArea(Node2D node)
        {
            if (node is Ball)
            {
                LaunchQueue.Enqueue(node as Ball);
            }
        }
    }

}