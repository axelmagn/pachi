using Godot;
using System;
using static TestAssert;

public static class BallVisualsTests
{
    public static void RunAllTests()
    {
        TestBallLoadsTierSpriteAndScalesProperly();
        TestBallFallbackToPlaceholderWhenNoSprite();
        TestBallDynamicVariantSwitchUpdatesVisuals();
        TestMotionTrailRendersUnderSprite();
    }


    public static void TestBallLoadsTierSpriteAndScalesProperly()
    {
        PackedScene ballScene = GD.Load<PackedScene>("res://src/balls/ball.tscn");
        Ball ball = ballScene.Instantiate<Ball>();
        ball.UpdateVisuals();

        Assert(ball.Sprite != null, "Ball Sprite2D node should not be null.");
        Assert(ball.PlaceholderSprite != null, "Ball PlaceholderSprite node should not be null.");
        Assert(ball.Variant != null, "Ball default variant should not be null.");
        Assert(ball.Variant!.Sprite != null, "Tier 1 variant should have a Sprite texture.");
        Assert(ball.Sprite!.Visible, "Ball Sprite2D should be visible when Variant has a sprite.");
        Assert(!ball.PlaceholderSprite!.Visible, "Ball PlaceholderSprite should be hidden when Variant has a sprite.");
        Assert(ball.Sprite.Texture == ball.Variant.Sprite, "Sprite2D texture should match Variant.Sprite.");

        // Check scaling: radius is 6.0, diameter 12.0
        float expectedDiameter = ball.GetRadius() * 2.0f;
        Vector2 texSize = ball.Sprite.Texture.GetSize();
        float expectedScale = expectedDiameter / Math.Max(texSize.X, texSize.Y);
        Assert(Mathf.IsEqualApprox(ball.Sprite.Scale.X, expectedScale), $"Sprite scale X ({ball.Sprite.Scale.X}) should match expected scale ({expectedScale}).");
        Assert(Mathf.IsEqualApprox(ball.Sprite.Scale.Y, expectedScale), $"Sprite scale Y ({ball.Sprite.Scale.Y}) should match expected scale ({expectedScale}).");

        ball.QueueFree();
    }

    public static void TestBallFallbackToPlaceholderWhenNoSprite()
    {
        PackedScene ballScene = GD.Load<PackedScene>("res://src/balls/ball.tscn");
        Ball ball = ballScene.Instantiate<Ball>();

        // Assign variant without sprite
        var fallbackVariant = new BallVariant
        {
            PlaceholderColor = Colors.Red,
            Sprite = null
        };
        ball.Variant = fallbackVariant;

        Assert(!ball.Sprite!.Visible, "Sprite2D should be hidden when Variant has no sprite.");
        Assert(ball.PlaceholderSprite!.Visible, "PlaceholderSprite should be visible when Variant has no sprite.");
        Assert(ball.PlaceholderSprite.Color == Colors.Red, "PlaceholderSprite color should match Variant.PlaceholderColor.");

        ball.QueueFree();
    }

    public static void TestBallDynamicVariantSwitchUpdatesVisuals()
    {
        PackedScene ballScene = GD.Load<PackedScene>("res://src/balls/ball.tscn");
        Ball ball = ballScene.Instantiate<Ball>();

        BallVariant tier5 = GD.Load<BallVariant>("res://src/balls/tiers/tier_5.tres");
        ball.Variant = tier5;

        Assert(ball.Sprite!.Visible, "Sprite2D should be visible for Tier 5.");
        Assert(!ball.PlaceholderSprite!.Visible, "PlaceholderSprite should be hidden for Tier 5.");
        Assert(ball.Sprite.Texture == tier5.Sprite, "Sprite texture should update to Tier 5 blue gem.");

        ball.QueueFree();
    }

    public static void TestMotionTrailRendersUnderSprite()

    {
        PackedScene ballScene = GD.Load<PackedScene>("res://src/balls/ball.tscn");
        Ball ball = ballScene.Instantiate<Ball>();

        Assert(ball.MotionTrail != null, "Ball MotionTrail2D node should not be null.");
        Assert(ball.MotionTrail!.ZIndex < 0, $"MotionTrail ZIndex ({ball.MotionTrail.ZIndex}) should be negative (render behind ball).");

        ball.QueueFree();
    }
}

