using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

public partial class PocketArmsControllerTests : TestClass
{
    public PocketArmsControllerTests(Node testScene) : base(testScene)
    {
    }

    [Test]
    public void ArmsController_InitializesInClosedState()
    {
        var controller = new PocketArmsController();
        try
        {
            controller.CurrentArmState.ShouldBe(PocketArmsController.ArmState.Closed);
            controller.IsOpen.ShouldBeFalse();
        }
        finally
        {
            controller.Free();
        }
    }

    [Test]
    public void OpenArmsAndCloseArms_TransitionStateCorrectly()
    {
        var controller = new PocketArmsController();
        var leftArm = new CharacterBody2D();
        var rightArm = new CharacterBody2D();
        using var config = new PocketConfig { ArmOpenRotation = 45.0f, ArmOpenDuration = 3.0f };

        try
        {
            controller.Config = config;
            controller.LeftArm = leftArm;
            controller.RightArm = rightArm;

            // When outside scene tree, transitions apply synchronously without relying on SceneTree tweens.
            controller.OpenArms();
            controller.IsOpen.ShouldBeTrue();
            controller.CurrentArmState.ShouldBe(PocketArmsController.ArmState.Open);
            leftArm.RotationDegrees.ShouldBe(-45.0f);
            rightArm.RotationDegrees.ShouldBe(45.0f);

            controller.CloseArms();
            controller.IsOpen.ShouldBeFalse();
            controller.CurrentArmState.ShouldBe(PocketArmsController.ArmState.Closed);
            leftArm.RotationDegrees.ShouldBe(0.0f);
            rightArm.RotationDegrees.ShouldBe(0.0f);
        }
        finally
        {
            leftArm.Free();
            rightArm.Free();
            controller.Free();
        }
    }

    [Test]
    public void ApplyArmVisibility_ConfiguresVisibilityAndProcessMode()
    {
        var controller = new PocketArmsController();
        var leftArm = new CharacterBody2D();
        var rightArm = new CharacterBody2D();
        using var config = new PocketConfig { HasArms = false };

        try
        {
            controller.Config = config;
            controller.LeftArm = leftArm;
            controller.RightArm = rightArm;

            controller.ApplyArmVisibility();

            leftArm.Visible.ShouldBeFalse();
            leftArm.ProcessMode.ShouldBe(Node.ProcessModeEnum.Disabled);
            rightArm.Visible.ShouldBeFalse();
            rightArm.ProcessMode.ShouldBe(Node.ProcessModeEnum.Disabled);
        }
        finally
        {
            leftArm.Free();
            rightArm.Free();
            controller.Free();
        }
    }
}
