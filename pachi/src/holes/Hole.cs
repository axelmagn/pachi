using Godot;
using System;
using System.Diagnostics;

public partial class Hole : Area2D
{
    [Export]
    public Node2D BallRoot { get; set; }

    [Export]
    public float AnimateDuration { get; set; } = 0.5f;

    [Export]
    public float AnimateMinBallScale { get; set; } = 0.1f;

    [Export]
    public float AnimateMinBallAlpha { get; set; } = 0.1f;


    public Ball HeldBall { get; set; }


    public override void _Ready()
    {
        Debug.Assert(BallRoot != null);
    }

    private void AnimateIncomingBall()
    {
        Debug.Assert(BallRoot != null);

        // set up ball root in initial state
        BallRoot.Modulate = new(1.0f, 1.0f, 1.0f, AnimateMinBallAlpha);
        BallRoot.Scale = Vector2.One * AnimateMinBallScale;
        BallRoot.Show();

        // tween in to full size and alpha
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(BallRoot, "modulate", Colors.White, AnimateDuration);
        tween.TweenProperty(BallRoot, "scale", Vector2.One, AnimateDuration);
    }

    private void AnimateOutgoingBall()
    {
        Debug.Assert(BallRoot != null);

        // set up ball root in initial state
        BallRoot.Modulate = Colors.White;
        BallRoot.Scale = Vector2.One;
        BallRoot.Show();

        // tween out to min size and alpha
        Tween tween = GetTree().CreateTween();
        Color outColor = new(1.0f, 1.0f, 1.0f, AnimateMinBallAlpha);
        tween.TweenProperty(BallRoot, "modulate", outColor, AnimateDuration);
        Vector2 outScale = Vector2.One * AnimateMinBallScale;
        tween.TweenProperty(BallRoot, "scale", outScale, AnimateDuration);
    }
}
