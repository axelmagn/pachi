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
        // Unparented Godot nodes allocated in unit tests must be explicitly freed via Free(),
        // otherwise their underlying C++ peers in ObjectDB leak on headless engine exit.
        var ball = new Ball();
        try
        {
            ball.DetectStuck.ShouldBeTrue();
            ball.IsInPlay.ShouldBeFalse();
            ball.CurrentTransitionState.ShouldBe(Ball.TransitionState.None);
        }
        finally
        {
            ball.Free();
        }
    }

    [Test]
    public void BallVariant_SetsPlaceholderColorCorrectly()
    {
        // Explicitly dispose custom Resource/RefCounted instances to prevent lingering ObjectDB references.
        using var variant = new BallVariant
        {
            BasePrice = 5,
            PlaceholderColor = Colors.Green
        };

        variant.BasePrice.ShouldBe(5);
        variant.PlaceholderColor.ShouldBe(Colors.Green);
    }
}
