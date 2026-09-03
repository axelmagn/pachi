using Godot;
using Godot.Collections;

/// <summary>
/// Consolidated configuration resource for pocket arms animation, audio feedback, and timings.
/// </summary>
[Tool]
[GlobalClass]
public partial class PocketConfig : Resource
{
    // --- Arms Settings ---
    [Export]
    public bool HasArms { get; set; } = true;

    [Export]
    public float ArmOpenRotation { get; set; } = 60.0f;

    [Export]
    public float ArmRotationSpeed { get; set; } = Mathf.Pi;

    [Export]
    public float ArmOpenDuration { get; set; } = 5.0f;

    [Export]
    public float ArmTweenDuration { get; set; } = 0.3f;

    [Export]
    public Tween.TransitionType ArmTweenTransition { get; set; } = Tween.TransitionType.Cubic;

    [Export]
    public Tween.EaseType ArmTweenEase { get; set; } = Tween.EaseType.Out;

    // --- Audio Settings ---
    [Export]
    public Array<AudioStream>? AcceptAudioStreams { get; set; }

    [Export]
    public AudioStream? RejectAudioStream { get; set; }

    [Export]
    public AudioStream? PayoutAudioStream { get; set; }

    [Export]
    public bool UsePitchScaleFallback { get; set; } = true;

    [Export]
    public float SemitonesPerStep { get; set; } = 2.0f;

    /// <summary>
    /// Computes exponential pitch multiplier for progressive audio feedback as input requirements fill up.
    /// </summary>
    public float CalculatePitchScale(int filledSlotIndex, int streamCount)
    {
        if (streamCount > 0)
        {
            if (filledSlotIndex >= streamCount && UsePitchScaleFallback)
            {
                int extraSteps = filledSlotIndex - (streamCount - 1);
                return Mathf.Pow(2.0f, (extraSteps * SemitonesPerStep) / 12.0f);
            }
            return 1.0f;
        }

        return Mathf.Pow(2.0f, (filledSlotIndex * SemitonesPerStep) / 12.0f);
    }
}
