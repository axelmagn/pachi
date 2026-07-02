using Godot;
using System;
using System.Diagnostics;

public partial class Hole : Area2D
{
    /// A clear occurs when either a transition animation finishes, is interrupted with a new ball,
    /// or ClearBall is manually called.  It is up to the owner to decide what behavior to
    /// implement when the ball is cleared.  The cleared ball will be orphaned, so by default it is
    /// freed.  This behavior may be overriden with the FreeHeldBallOnClear flag.
    [Signal]
    public delegate void HeldBallClearedEventHandler(Ball ball);

    [Export]
    public Node2D BallRoot { get; set; }

    [Export]
    public CollisionShape2D Collider { get; set; }

    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    [Export]
    public bool FreeHeldBallOnClear { get; set; } = true;

    public Ball HeldBall { get; set; }


    public override void _Ready()
    {
        Debug.Assert(BallRoot != null);
        Debug.Assert(Collider != null);
        Debug.Assert(AnimationPlayer != null);

        AnimationPlayer.AnimationFinished += OnAnimationFinished;
    }

    public float GetRadius()
    {
        Debug.Assert(Collider.Shape is CircleShape2D);
        CircleShape2D circle = (CircleShape2D)Collider.Shape;
        return circle.Radius;
    }

    public void AddIncomingBall(Ball ball) {
        ReplaceHeldBall(ball);
        AnimationPlayer.Play("incoming");
    }

    public void AddOutgoingBall(Ball ball) {
        ReplaceHeldBall(ball);
        AnimationPlayer.Play("outgoing");
    }



    /// clear HeldBall and handle cleanup
    public Ball ClearHeldBall() {
        Ball ball = HeldBall;
        HeldBall = null;

        if (ball != null) {
            BallRoot.RemoveChild(ball);
            if (FreeHeldBallOnClear) ball.QueueFree();
            EmitSignal(SignalName.HeldBallCleared, ball);
        }

        return ball;
    }

    public Ball ReplaceHeldBall(Ball ball) {
        if (ball == HeldBall) return null;
        Ball oldBall = ClearHeldBall();
        ball.SetPhysicsProcess(false);
        HeldBall = ball;
        ball.Reparent(BallRoot);
        ball.Position = Vector2.Zero;
        return oldBall;
    }

    private void OnAnimationFinished(StringName animName) {
        ClearHeldBall();
    }
}
