using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public partial class SceneSmokeTests : IntegrationTestClass
{
    public SceneSmokeTests(Node testScene) : base(testScene)
    {
    }

    [Test]
    public void BallScene_InstantiatesAndFindsSpriteNode()
    {
        var ball = InstantiateAndTrack<Ball>("res://src/balls/ball.tscn");
        ball.ShouldNotBeNull();
        ball.Sprite.ShouldNotBeNull();
        ball.GetParent().ShouldNotBeNull();
    }

    [Test]
    public void PocketScene_InstantiatesAndFindsComponents()
    {
        // Verifies that native packed scenes resolve child controller exports on instantiation.
        var pocket = InstantiateAndTrack<Pocket>("res://src/pockets/pocket.tscn");
        pocket.ShouldNotBeNull();
        pocket.Config.ShouldNotBeNull();
        pocket.CatchHole.ShouldNotBeNull();
        pocket.RejectHole.ShouldNotBeNull();
        pocket.ArmsController.ShouldNotBeNull();
        pocket.AudioPlayer.ShouldNotBeNull();
        pocket.InputsIndicator.ShouldNotBeNull();
        pocket.OutputsIndicator.ShouldNotBeNull();
    }

    [Test]
    public void YakumonoScene_InstantiatesAndFindsComponents()
    {
        // Verifies that specialized pocket subclasses resolve audio and visual layers cleanly.
        var yakumono = InstantiateAndTrack<Yakumono>("res://src/yakumono/yakumono.tscn");
        yakumono.ShouldNotBeNull();
        yakumono.Config.ShouldNotBeNull();
        yakumono.CatchHole.ShouldNotBeNull();
        yakumono.RejectHole.ShouldNotBeNull();
        yakumono.AudioPlayer.ShouldNotBeNull();
        yakumono.InputsIndicator.ShouldNotBeNull();
        yakumono.OutputsIndicator.ShouldNotBeNull();
        yakumono.FaceSprite.ShouldNotBeNull();
    }
}
