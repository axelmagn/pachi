using Godot;
using System;

namespace Pachi
{
    public partial class Camera : Camera2D
    {
        [Export]
        float PanSpeed = 10.0f;


        public override void _Process(double delta)
        {
            float panVerticalInput = Input.GetAxis("camera_pan_up", "camera_pan_down");
            Vector2 pos = Position;
            pos.Y += (float)(delta * panVerticalInput * PanSpeed);
            Position = pos;
        }
    }

}