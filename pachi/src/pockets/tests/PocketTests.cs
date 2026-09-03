using Chickensoft.GoDotTest;
using Godot;
using Godot.Collections;
using Shouldly;

public partial class PocketTests : TestClass
{
    public PocketTests(Node testScene) : base(testScene)
    {
    }

    [Test]
    public void ClampBallCapacities_RestrictsInputsAndOutputsToMaximums()
    {
        // Unparented test nodes must be explicitly freed to prevent headless ObjectDB leaks on exit.
        var pocket = new Pocket();
        using var v1 = new BallVariant { BasePrice = 1 };
        using var v2 = new BallVariant { BasePrice = 2 };

        try
        {
            // Overflow both lists past board balance capacity limits.
            pocket.InputBalls = [v1, v1, v1, v1, v1, v1];
            pocket.OutputBalls = [v2, v2, v2, v2, v2, v2, v2, v2, v2, v2];

            pocket.ClampBallCapacities();

            pocket.InputBalls.Count.ShouldBe(Pocket.MaxInputCapacity);
            pocket.OutputBalls.Count.ShouldBe(Pocket.MaxOutputCapacity);
        }
        finally
        {
            pocket.Free();
        }
    }

    [Test]
    public void RefreshIndicatorAndSlots_InitializesAllSlotsAsAvailable()
    {
        var pocket = new Pocket();
        using var v1 = new BallVariant { BasePrice = 1 };
        using var v2 = new BallVariant { BasePrice = 2 };

        try
        {
            pocket.InputBalls = [v1, v2];
            pocket.RefreshIndicatorAndSlots();

            pocket.InputBallSlotAvailable.ShouldNotBeNull();
            pocket.InputBallSlotAvailable.Count.ShouldBe(2);
            pocket.InputBallSlotAvailable[0].ShouldBeTrue();
            pocket.InputBallSlotAvailable[1].ShouldBeTrue();
        }
        finally
        {
            pocket.Free();
        }
    }

    [Test]
    public void ArmsDelegation_PassesThroughToArmsController()
    {
        var pocket = new Pocket();
        var armsController = new PocketArmsController();
        var leftArm = new CharacterBody2D();
        var rightArm = new CharacterBody2D();

        try
        {
            armsController.LeftArm = leftArm;
            armsController.RightArm = rightArm;
            pocket.ArmsController = armsController;

            // Pockets delegate arm queries to preserve backward compatibility for existing callers.
            pocket.IsOpen.ShouldBeFalse();

            pocket.OpenArms();
            pocket.IsOpen.ShouldBeTrue();

            pocket.CloseArms();
            pocket.IsOpen.ShouldBeFalse();

            pocket.ToggleArms();
            pocket.IsOpen.ShouldBeTrue();
        }
        finally
        {
            leftArm.Free();
            rightArm.Free();
            armsController.Free();
            pocket.Free();
        }
    }
}
