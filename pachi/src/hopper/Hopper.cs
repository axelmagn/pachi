using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Hopper : Node2D
{
    private readonly LinkedList<Ball> _containedBalls = new();

    public override void _Ready()
    {
        Debug.Assert(_containedBalls != null);

        // find all contained balls at start
        foreach (Node child in GetChildren())
        {
            if (child is Ball ball)
            {
                GD.Print("found hopper ball");
                _containedBalls.AddLast(ball);
            }
        }
        GD.Print("total hopper balls:", _containedBalls.Count());
    }

    public int BallCount()
    {
        return _containedBalls.Count();
    }

    public Ball PopFirstBall()
    {
        if (_containedBalls.Count() == 0)
        {
            return null;
        }
        Ball first = _containedBalls.First();
        _containedBalls.RemoveFirst();
        return first;
    }
}
