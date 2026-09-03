using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public partial class PocketAudioPlayerTests : TestClass
{
    public PocketAudioPlayerTests(Node testScene) : base(testScene)
    {
    }

    [Test]
    public void PlaybackMethods_ExecuteSafely_WhenPlayersAreNull()
    {
        var player = new PocketAudioPlayer();
        try
        {
            // Verify null safety when invoked before _Ready or in headless isolation.
            Should.NotThrow(() => player.PlayAccept(0));
            Should.NotThrow(() => player.PlayReject());
            Should.NotThrow(() => player.PlayPayout());
        }
        finally
        {
            player.Free();
        }
    }

    [Test]
    public void PlayAccept_UsesConfigPitchScaleAndStream()
    {
        var player = new PocketAudioPlayer();
        var streamPlayer = new AudioStreamPlayer2D();
        using var stream1 = new AudioStreamWav();
        using var stream2 = new AudioStreamWav();
        using var config = new PocketConfig
        {
            AcceptAudioStreams = [stream1, stream2],
            UsePitchScaleFallback = true,
            SemitonesPerStep = 2.0f,
        };

        try
        {
            player.Config = config;
            player.AcceptAudioPlayer = streamPlayer;

            player.PlayAccept(0);
            streamPlayer.Stream.ShouldBe(stream1);
            streamPlayer.PitchScale.ShouldBe(1.0f);

            player.PlayAccept(1);
            streamPlayer.Stream.ShouldBe(stream2);
            streamPlayer.PitchScale.ShouldBe(1.0f);

            // Slot 2 exceeds stream count (2) by 1 step -> pitch shift.
            player.PlayAccept(2);
            streamPlayer.Stream.ShouldBe(stream2);
            streamPlayer.PitchScale.ShouldBe(Mathf.Pow(2.0f, 2.0f / 12.0f), 0.001f);
        }
        finally
        {
            streamPlayer.Free();
            player.Free();
        }
    }
}
