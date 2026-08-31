using Godot;
using System;
using static TestAssert;

public static class PrizeMeterIntegrationTests
{
    public static void RunAllTests()
    {
        TestMainGameScenePrizeMeterIntegration();
    }

    public static void TestMainGameScenePrizeMeterIntegration()
    {
        var scene = ResourceLoader.Load<PackedScene>("res://src/main_game/main_game.tscn");
        Assert(scene != null, "main_game.tscn should load successfully.");

        var game = scene!.Instantiate<MainGameController>();
        Assert(game != null, "Game should instantiate as MainGameController.");

        var tree = Engine.GetMainLoop() as SceneTree;
        Assert(tree != null, "SceneTree must not be null.");
        tree!.Root.AddChild(game);
        game!._Ready();

        try
        {
            Assert(game!.PrizeMeter != null, "PrizeMeter node should be wired in MainGameController.");
            Assert(game.PrizeMeterUI != null, "PrizeMeterUI node should be wired in MainGameController.");
            Assert(game.Hopper != null, "Hopper node should be wired in MainGameController.");
            Assert(game.Level != null, "Level node should be wired in MainGameController.");

            // Verify initial state
            Assert(game.PrizeMeter!.CurrentProgress == 0.0f, "PrizeMeter progress starts at 0.");
            Assert(game.PrizeMeter.TotalTokens == 0, "PrizeMeter total tokens starts at 0.");
            Assert(game.PrizeMeterUI!.ResetButton != null, "ResetButton must not be null.");
            Assert(game.PrizeMeterUI.ResetButton!.Disabled, "Reset button should start disabled.");

            // Simulate scoring events
            game.PrizeMeter.AddScore(120.0f);

            Assert(game.PrizeMeter.TokensEarnedInRun == 1, "Should have earned 1 token in run.");
            Assert(game.PrizeMeter.TotalTokens == 1, "Total tokens should be 1.");
            Assert(!game.PrizeMeterUI.ResetButton.Disabled, "Reset button should now be enabled.");
            Assert(Mathf.IsEqualApprox(game.PrizeMeter.CurrentProgress, 20.0f, 0.001f), $"Expected 20.0 carryover, got {game.PrizeMeter.CurrentProgress}.");

            // Execute prestige reset
            bool resetOk = game.ExecutePrestigeReset();
            Assert(resetOk, "Prestige reset should execute successfully.");

            Assert(game.PrizeMeter.TokensEarnedInRun == 0, "Tokens earned in run should reset to 0.");
            Assert(game.PrizeMeter.TotalTokens == 1, "Total tokens should remain 1 after reset.");
            Assert(game.PrizeMeter.CurrentProgress == 0.0f, "Current progress should reset to 0.");
            Assert(Mathf.IsEqualApprox(game.PrizeMeter.CurrentTargetCapacity, 100.0f), "Target capacity should reset to 100.");
        }
        finally
        {
            tree!.Root.RemoveChild(game);
            game!.QueueFree();
        }
    }
}
