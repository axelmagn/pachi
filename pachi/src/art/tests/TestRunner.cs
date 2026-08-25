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
