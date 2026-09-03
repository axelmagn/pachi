using Godot;

/// <summary>
/// Handles audio playback for pocket interactions, delegating sound resources and pitch calculations to PocketConfig.
/// </summary>
[Tool]
[GlobalClass]
public partial class PocketAudioPlayer : Node2D
{
    [Export]
    public PocketConfig? Config { get; set; }

    [Export]
    public AudioStreamPlayer2D? AcceptAudioPlayer { get; set; }

    [Export]
    public AudioStreamPlayer2D? RejectAudioPlayer { get; set; }

    [Export]
    public AudioStreamPlayer2D? PayoutAudioPlayer { get; set; }

    public override void _Ready()
    {
        // Resolve nested audio players if scenes instantiate without explicit node references.
        AcceptAudioPlayer ??= GetNodeOrNull<AudioStreamPlayer2D>(nameof(AcceptAudioPlayer));
        RejectAudioPlayer ??= GetNodeOrNull<AudioStreamPlayer2D>(nameof(RejectAudioPlayer));
        PayoutAudioPlayer ??= GetNodeOrNull<AudioStreamPlayer2D>(nameof(PayoutAudioPlayer));
    }

    /// <summary>
    /// Plays progressive chime feedback corresponding to the filled slot index.
    /// </summary>
    public void PlayAccept(int filledSlotIndex)
    {
        if (AcceptAudioPlayer == null) return;

        var streams = Config?.AcceptAudioStreams;
        if (streams != null && streams.Count > 0)
        {
            int streamIndex = Mathf.Clamp(filledSlotIndex, 0, streams.Count - 1);
            AcceptAudioPlayer.Stream = streams[streamIndex];
            AcceptAudioPlayer.PitchScale = Config?.CalculatePitchScale(filledSlotIndex, streams.Count) ?? 1.0f;
        }
        else if (AcceptAudioPlayer.Stream != null)
        {
            AcceptAudioPlayer.PitchScale = Config?.CalculatePitchScale(filledSlotIndex, 0) ?? 1.0f;
        }

        if (AcceptAudioPlayer.IsInsideTree())
        {
            AcceptAudioPlayer.Play();
        }
    }

    /// <summary>
    /// Plays negative feedback sound when a ball is rejected.
    /// </summary>
    public void PlayReject()
    {
        if (RejectAudioPlayer == null) return;

        if (Config?.RejectAudioStream != null)
        {
            RejectAudioPlayer.Stream = Config.RejectAudioStream;
        }
        RejectAudioPlayer.PitchScale = 1.0f;

        if (RejectAudioPlayer.IsInsideTree())
        {
            RejectAudioPlayer.Play();
        }
    }

    /// <summary>
    /// Plays celebratory sound when all input requirements are satisfied and payout is awarded.
    /// </summary>
    public void PlayPayout()
    {
        // Fall back to AcceptAudioPlayer if a dedicated payout player is not provisioned.
        var player = PayoutAudioPlayer ?? AcceptAudioPlayer;
        if (player == null) return;

        if (Config?.PayoutAudioStream != null)
        {
            player.Stream = Config.PayoutAudioStream;
        }
        player.PitchScale = 1.0f;

        if (player.IsInsideTree())
        {
            player.Play();
        }
    }
}
