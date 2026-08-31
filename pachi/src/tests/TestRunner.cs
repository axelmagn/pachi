using Godot;
using System;

[GlobalClass]
public partial class TestRunner : SceneTree
{
    public override void _Initialize()
    {
        GD.Print("Running tests...");
        try
        {
            VisualConfigTests.RunAllTests();
            BallStuckTests.RunAllTests();
            PocketIndicatorTests.RunAllTests();
            SocketLifecycleTests.RunAllTests();
            PackageDealCardTests.RunAllTests();
            DealMeterTests.RunAllTests();
            HopperCostDeductionTests.RunAllTests();
            CardShopTests.RunAllTests();
            CardShopUITests.RunAllTests();
            CardShopIntegrationTests.RunAllTests();
            GD.Print("All tests passed successfully!");
            Quit(0);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Test failed: {ex}");
            Quit(1);
        }
    }
}
