using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

/// <summary>
/// Checks that scenes wire their exported node references. Loading every scene
/// is covered by <see cref="AllScenesTests"/>.
/// </summary>
public partial class SceneExportTests : IntegrationTestClass
{
    public SceneExportTests(Node testScene) : base(testScene)
    {
    }

    [Test]
    public void BallScene_WiresSprite()
    {
        var ball = InstantiateAndTrack<Ball>("res://src/balls/ball.tscn");
        ball.Sprite.ShouldNotBeNull();
    }

    [Test]
    public void PocketScene_WiresComponents()
    {
        // Verifies that native packed scenes resolve child controller exports on instantiation.
        var pocket = InstantiateAndTrack<Pocket>("res://src/pockets/pocket.tscn");
        pocket.Config.ShouldNotBeNull();
        pocket.CatchHole.ShouldNotBeNull();
        pocket.RejectHole.ShouldNotBeNull();
        pocket.ArmsController.ShouldNotBeNull();
        pocket.AudioPlayer.ShouldNotBeNull();
        pocket.InputsIndicator.ShouldNotBeNull();
        pocket.OutputsIndicator.ShouldNotBeNull();
    }

    [Test]
    public void YakumonoScene_WiresComponents()
    {
        // Verifies that specialized pocket subclasses resolve audio and visual layers cleanly.
        var yakumono = InstantiateAndTrack<Yakumono>("res://src/yakumono/yakumono.tscn");
        yakumono.Config.ShouldNotBeNull();
        yakumono.CatchHole.ShouldNotBeNull();
        yakumono.RejectHole.ShouldNotBeNull();
        yakumono.AudioPlayer.ShouldNotBeNull();
        yakumono.InputsIndicator.ShouldNotBeNull();
        yakumono.OutputsIndicator.ShouldNotBeNull();
        yakumono.FaceSprite.ShouldNotBeNull();
    }
}
