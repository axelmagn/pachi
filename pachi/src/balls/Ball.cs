using Godot;
using System;
using System.Diagnostics;

public partial class Ball : RigidBody2D
{
    [Export]
    public AudioStreamPlayer2D BallPinBounceAudioPlayer { get; set; }
    [Export]
    public AudioStreamPlayer2D BallBallBounceAudioPlayer { get; set; }

    /// impact strength threshold at which bounce audio will play
    [Export]
    public float BounceAudioThreshold { get; set; } = 10.0f;

    [Export]
    public float MaxExpectedVelocity = 800.0f;

    private Vector2 _previousVelocity = Vector2.Zero;

    public override void _Ready()
    {
        Debug.Assert(BallPinBounceAudioPlayer != null);
        Debug.Assert(BallBallBounceAudioPlayer != null);

        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        _previousVelocity = LinearVelocity;
    }

    private void OnBodyEntered(Node body)
    {
        Debug.Assert(BallPinBounceAudioPlayer != null);

        float impactStrength = (_previousVelocity - LinearVelocity).Length();

        if (impactStrength < BounceAudioThreshold) return;

        // TODO: move group names to a constant
        if (body.IsInGroup("ball_material"))
        {
            PlayImpactAudio(BallBallBounceAudioPlayer, impactStrength);
        }
        else if (body.IsInGroup("pin_material"))
        {
            PlayImpactAudio(BallPinBounceAudioPlayer, impactStrength);
        }
    }

    private void PlayImpactAudio(AudioStreamPlayer2D audioPlayer, float impactStrength)
    {
        float normalizedImpact = Mathf.Clamp(impactStrength / MaxExpectedVelocity, 0.01f, 1.0f);
        audioPlayer.VolumeDb = Mathf.LinearToDb(normalizedImpact);

        float targetPitch = Mathf.Remap(normalizedImpact, 0.0f, 1.0f, 0.9f, 1.3f);
        audioPlayer.PitchScale = targetPitch + (float)GD.RandRange(-0.05, 0.05);

        audioPlayer.Play();
    }

}
