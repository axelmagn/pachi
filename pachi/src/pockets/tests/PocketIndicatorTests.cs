using Godot;
using Godot.Collections;
using System;
using static TestAssert;

public static class PocketIndicatorTests
{
    public static void RunAllTests()
    {
        TestStaticPipGridDimensions();
        TestPocketIndicatorDynamicRepositioning();
        TestPocketAndYakumonoCapacityClamping();
    }

    public static void TestStaticPipGridDimensions()
    {
        var indicator = new PocketBallsIndicator();
        indicator.IsCardIndicator = false;

        // Default / empty
        indicator.Balls = null;
        Assert(indicator.Size == new Vector2(34, 10), $"Default Size should be (34, 10), got {indicator.Size}");

        var variant = new BallVariant { PlaceholderColor = Colors.Red };

        // 1 to 4 balls -> 1 row (34x10)
        indicator.Balls = new Array<BallVariant> { variant };
        Assert(indicator.Size == new Vector2(34, 10), $"1 ball Size should be (34, 10), got {indicator.Size}");

        indicator.Balls = new Array<BallVariant> { variant, variant, variant, variant };
        Assert(indicator.Size == new Vector2(34, 10), $"4 balls Size should be (34, 10), got {indicator.Size}");

        // 5 to 8 balls -> 2 rows (34x18)
        indicator.Balls = new Array<BallVariant> { variant, variant, variant, variant, variant };
        Assert(indicator.Size == new Vector2(34, 18), $"5 balls Size should be (34, 18), got {indicator.Size}");

        indicator.Balls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant, variant, variant };
        Assert(indicator.Size == new Vector2(34, 18), $"8 balls Size should be (34, 18), got {indicator.Size}");
    }

    public static void TestPocketIndicatorDynamicRepositioning()
    {
        var pocket = new Pocket();
        var inputs = new PocketBallsIndicator { Position = new Vector2(0, 30), Size = new Vector2(34, 10) };
        var outputs = new PocketBallsIndicator { Position = new Vector2(0, 42), Size = new Vector2(34, 10) };
        pocket.InputsIndicator = inputs;
        pocket.OutputsIndicator = outputs;

        var variant = new BallVariant { PlaceholderColor = Colors.Blue };

        // 1 row outputs (1-4 balls)
        pocket.OutputBalls = new Array<BallVariant> { variant, variant };
        pocket.RefreshIndicatorAndSlots();
        Assert(outputs.Size == new Vector2(34, 10), $"1-row outputs indicator Size should be (34, 10), got {outputs.Size}");
        Assert(Mathf.IsEqualApprox(outputs.Position.Y, 42.0f), $"1-row outputs indicator Position.Y should be 42, got {outputs.Position.Y}");

        // 2 row outputs (5-8 balls)
        pocket.OutputBalls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant };
        pocket.RefreshIndicatorAndSlots();
        Assert(outputs.Size == new Vector2(34, 18), $"2-row outputs indicator Size should be (34, 18), got {outputs.Size}");
        Assert(Mathf.IsEqualApprox(outputs.Position.Y, 46.0f), $"2-row outputs indicator Position.Y should be 46, got {outputs.Position.Y}");

        // Yakumono positioning
        var yakumono = new Yakumono();
        var yInputs = new PocketBallsIndicator { Position = new Vector2(0, 44), Size = new Vector2(34, 10) };
        var yOutputs = new PocketBallsIndicator { Position = new Vector2(0, 56), Size = new Vector2(34, 10) };
        yakumono.InputsIndicator = yInputs;
        yakumono.OutputsIndicator = yOutputs;

        yakumono.OutputBalls = new Array<BallVariant> { variant };
        yakumono.RefreshIndicatorAndSlots();
        Assert(Mathf.IsEqualApprox(yOutputs.Position.Y, 56.0f), $"Yakumono 1-row outputs Position.Y should be 56, got {yOutputs.Position.Y}");

        yakumono.OutputBalls = new Array<BallVariant> { variant, variant, variant, variant, variant };
        yakumono.RefreshIndicatorAndSlots();
        Assert(Mathf.IsEqualApprox(yOutputs.Position.Y, 60.0f), $"Yakumono 2-row outputs Position.Y should be 60, got {yOutputs.Position.Y}");
    }

    public static void TestPocketAndYakumonoCapacityClamping()
    {
        var variant = new BallVariant { PlaceholderColor = Colors.Green };
        var pocket = new Pocket
        {
            InputsIndicator = new PocketBallsIndicator(),
            OutputsIndicator = new PocketBallsIndicator()
        };

        pocket.InputBalls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant };
        pocket.OutputBalls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant, variant, variant, variant, variant };
        pocket.RefreshIndicatorAndSlots();

        Assert(pocket.InputBalls.Count == 4, $"Pocket input balls should be clamped to 4, got {pocket.InputBalls.Count}");
        Assert(pocket.OutputBalls.Count == 8, $"Pocket output balls should be clamped to 8, got {pocket.OutputBalls.Count}");

        var yakumono = new Yakumono
        {
            InputsIndicator = new PocketBallsIndicator(),
            OutputsIndicator = new PocketBallsIndicator()
        };

        yakumono.InputBalls = new Array<BallVariant> { variant, variant, variant, variant, variant };
        yakumono.OutputBalls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant, variant, variant, variant };
        yakumono.RefreshIndicatorAndSlots();

        Assert(yakumono.InputBalls.Count == 4, $"Yakumono input balls should be clamped to 4, got {yakumono.InputBalls.Count}");
        Assert(yakumono.OutputBalls.Count == 8, $"Yakumono output balls should be clamped to 8, got {yakumono.OutputBalls.Count}");
    }
}
