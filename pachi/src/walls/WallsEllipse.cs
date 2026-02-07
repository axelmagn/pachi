using Godot;
using Godot.Collections;
using System;

namespace Pachi
{
    [Tool]
    public partial class WallsEllipse : StaticBody2D
    {
        [Export]
        float Thickness
        {
            get;
            set
            {
                field = value;
                UpdateWallShapes();
            }
        } = 10;

        [Export]
        Vector2 Size
        {
            get;
            set
            {
                field = value;
                UpdateWallShapes();
            }
        } = new(400, 800);

        [Export]
        int NumSegments
        {
            get;
            set
            {
                field = value;
                UpdateWallShapes();
            }

        } = 64;

        Array<CollisionShape2D> WallSegments = new();

        public void UpdateWallShapes()
        {
            // ensure there are enough segments
        }
    }

}
