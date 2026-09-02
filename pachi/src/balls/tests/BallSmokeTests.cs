using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public partial class BallSmokeTests : TestClass
{
    public BallSmokeTests(Node testScene) : base(testScene)
    {
    }

    [Test]
    public void Ball_InitializesWithDefaultProperties()
    {
        var ball = new Ball();
        ball.DetectStuck.ShouldBeTrue();
        ball.IsInPlay.ShouldBeFalse();
        ball.CurrentTransitionState.ShouldBe(Ball.TransitionState.None);
    }

    [Test]
    public void BallVariant_SetsPlaceholderColorCorrectly()
    {
        var variant = new BallVariant
        {
            BasePrice = 5,
            PlaceholderColor = Colors.Green
        };

        variant.BasePrice.ShouldBe(5);
        variant.PlaceholderColor.ShouldBe(Colors.Green);
    }
}
