using Godot;
using System;
using static TestAssert;

public static class PinToolPerformanceTests
{
    public static void RunAllTests()
    {
        TestPinEllipsePropertyChangeNoop();
        TestPinGridPropertyChangeNoop();
        TestPinFunnelRebuildEfficiency();
    }

    public static void TestPinEllipsePropertyChangeNoop()
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        Assert(tree != null, "SceneTree main loop should not be null.");

        var ellipseScene = ResourceLoader.Load<PackedScene>("res://src/pins/pin_ellipse.tscn");
        Assert(ellipseScene != null, "pin_ellipse.tscn should load.");

        var ellipse = ellipseScene!.Instantiate<PinEllipse>();
        tree!.Root.AddChild(ellipse);
        RenderingServer.ForceDraw();

        int initialChildren = ellipse.GetChildCount();
        Assert(initialChildren > 0, $"PinEllipse should have spawned pins (got {initialChildren}).");

        // Grab first child reference to detect if children were recreated
        Node firstChildBefore = ellipse.GetChild(0);

        // Setting identical values should not trigger rebuild
        ellipse.RadiusX = ellipse.RadiusX;
        ellipse.RadiusY = ellipse.RadiusY;
        ellipse.AverageSpacing = ellipse.AverageSpacing;
        ellipse.StartAngle = ellipse.StartAngle;
        ellipse.EndAngle = ellipse.EndAngle;
        ellipse.MirrorX = ellipse.MirrorX;
        ellipse.MirrorY = ellipse.MirrorY;

        Node firstChildAfter = ellipse.GetChild(0);
        Assert(firstChildBefore == firstChildAfter, "Setting PinEllipse properties to existing values should not recreate pins.");

        tree.Root.RemoveChild(ellipse);
        ellipse.QueueFree();
    }

    public static void TestPinGridPropertyChangeNoop()
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        Assert(tree != null, "SceneTree main loop should not be null.");

        var gridScene = ResourceLoader.Load<PackedScene>("res://src/pins/pin_grid.tscn");
        Assert(gridScene != null, "pin_grid.tscn should load.");

        var grid = gridScene!.Instantiate<PinGrid>();
        tree!.Root.AddChild(grid);
        RenderingServer.ForceDraw();

        int initialChildren = grid.GetChildCount();
        Assert(initialChildren > 0, $"PinGrid should have spawned pins (got {initialChildren}).");

        Node firstChildBefore = grid.GetChild(0);

        // Setting identical values should not trigger rebuild
        grid.Rows = grid.Rows;
        grid.Columns = grid.Columns;
        grid.SpacingX = grid.SpacingX;
        grid.SpacingY = grid.SpacingY;
        grid.RowOffset = grid.RowOffset;

        Node firstChildAfter = grid.GetChild(0);
        Assert(firstChildBefore == firstChildAfter, "Setting PinGrid properties to existing values should not recreate pins.");

        tree.Root.RemoveChild(grid);
        grid.QueueFree();
    }

    public static void TestPinFunnelRebuildEfficiency()
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        Assert(tree != null, "SceneTree main loop should not be null.");

        var funnelScene = ResourceLoader.Load<PackedScene>("res://src/pins/pin_funnel.tscn");
        Assert(funnelScene != null, "pin_funnel.tscn should load.");

        var funnel = funnelScene!.Instantiate<PinFunnel>();
        tree!.Root.AddChild(funnel);
        RenderingServer.ForceDraw();

        Assert(funnel.LeftEllipse != null, "PinFunnel LeftEllipse should be present.");
        Assert(funnel.RightEllipse != null, "PinFunnel RightEllipse should be present.");

        // Track how many pins are spawned / recreated when changing a single property on PinFunnel
        int leftRebuildCount = 0;
        int rightRebuildCount = 0;

        funnel.LeftEllipse!.ChildEnteredTree += (node) => leftRebuildCount++;
        funnel.RightEllipse!.ChildEnteredTree += (node) => rightRebuildCount++;

        // Change InnerWidth
        funnel.InnerWidth = 40.0f;

        int leftChildCount = funnel.LeftEllipse.GetChildCount();
        int rightChildCount = funnel.RightEllipse.GetChildCount();

        // If it rebuilt once per ellipse, leftRebuildCount == leftChildCount
        // If it did 7 cascading rebuilds, leftRebuildCount would be 7 * leftChildCount
        Assert(leftRebuildCount <= leftChildCount, $"LeftEllipse spawned {leftRebuildCount} pins (expected <= {leftChildCount} for 1 rebuild, got {leftRebuildCount / Math.Max(1, leftChildCount)} rebuilds).");
        Assert(rightRebuildCount <= rightChildCount, $"RightEllipse spawned {rightRebuildCount} pins (expected <= {rightChildCount} for 1 rebuild, got {rightRebuildCount / Math.Max(1, rightChildCount)} rebuilds).");

        tree.Root.RemoveChild(funnel);
        funnel.QueueFree();
    }
}
