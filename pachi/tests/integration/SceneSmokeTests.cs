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
}
