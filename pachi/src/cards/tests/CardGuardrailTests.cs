using Godot;
using Godot.Collections;
using System;
using static TestAssert;

public static class CardGuardrailTests
{
    public static void RunAllTests()
    {
        TestCardCapacityGuardrails();
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
