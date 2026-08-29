using Godot;
using Godot.Collections;
using System;
using static TestAssert;

public static class CardGuardrailTests
{
    public static void RunAllTests()
    {
        TestCardCapacityGuardrails();
        TestBallAwardIndicatorMultiRowPresentation();
    }

    public static void TestBallAwardIndicatorMultiRowPresentation()
    {
        var indicator = new BallAwardIndicator();
        indicator.MaxColumns = 6;

        var variant = new BallVariant { PlaceholderColor = Colors.Cyan };

        // 1 ball: 1 row of 1 col -> 10x10 px
        indicator.Balls = new Array<BallVariant> { variant };
        Assert(indicator.Size == new Vector2(10, 10), $"1 ball should be (10, 10), got {indicator.Size}");

        // 3 balls: 1 row of 3 cols -> 26x10 px (3*8 + 2)
        indicator.Balls = new Array<BallVariant> { variant, variant, variant };
        Assert(indicator.Size == new Vector2(26, 10), $"3 balls should be (26, 10), got {indicator.Size}");

        // 6 balls: 1 row of 6 cols -> 50x10 px (6*8 + 2)
        indicator.Balls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant };
        Assert(indicator.Size == new Vector2(50, 10), $"6 balls should be (50, 10), got {indicator.Size}");

        // 10 balls: 2 rows of up to 6 cols -> 50x18 px (2*8 + 2)
        indicator.Balls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant, variant, variant, variant, variant };
        Assert(indicator.Size == new Vector2(50, 18), $"10 balls should be (50, 18), got {indicator.Size}");

        // 15 balls: 3 rows of up to 6 cols -> 50x26 px (3*8 + 2)
        var fifteenBalls = new Array<BallVariant>();
        for (int i = 0; i < 15; i++) fifteenBalls.Add(variant);
        indicator.Balls = fifteenBalls;
        Assert(indicator.Size == new Vector2(50, 26), $"15 balls across 3 rows should be (50, 26), got {indicator.Size}");
    }

    public static void TestCardCapacityGuardrails()
    {
        var variant = new BallVariant { PlaceholderColor = Colors.White };
        var pocket = new Pocket
        {
            InputsIndicator = new PocketBallsIndicator(),
            OutputsIndicator = new PocketBallsIndicator(),
            InputBalls = new Array<BallVariant> { variant, variant, variant },
            OutputBalls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant, variant }
        };

        var addInputCard = new AddInputBallCardData { BallToAdd = variant };
        var addOutputCard = new AddOutputBallCardData { BallToAdd = variant };

        // Input count = 3 (< 4) -> can apply
        Assert(addInputCard.CanApply(pocket), "AddInputBallCardData should apply when pocket has 3 inputs");

        // Input count = 4 -> cannot apply
        pocket.InputBalls.Add(variant);
        Assert(!addInputCard.CanApply(pocket), "AddInputBallCardData should not apply when pocket has 4 inputs");

        // Output count = 7 (< 8) -> can apply
        Assert(addOutputCard.CanApply(pocket), "AddOutputBallCardData should apply when pocket has 7 outputs");

        // Output count = 8 -> cannot apply
        pocket.OutputBalls.Add(variant);
        Assert(!addOutputCard.CanApply(pocket), "AddOutputBallCardData should not apply when pocket has 8 outputs");
    }
}
