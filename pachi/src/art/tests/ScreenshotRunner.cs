using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class ScreenshotRunner : SceneTree
{
    public override async void _Initialize()
    {
        GD.Print("Starting ScreenshotRunner...");
        try
        {
            string artifactDir = "/home/axel/.gemini/antigravity-cli/brain/8a074248-9eb8-47d7-a104-1df2dfcfddd5";

            // 1. Capture Main Game
            await CaptureScene("res://src/main_game/main_game.tscn", $"{artifactDir}/main_game.png");

            // 2. Capture Visual Showcase
            await CaptureScene("res://src/art/visual_showcase.tscn", $"{artifactDir}/visual_showcase.png");

            GD.Print("Screenshots captured successfully!");
            Quit(0);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"ScreenshotRunner error: {ex}");
            Quit(1);
        }
    }

    private async Task CaptureScene(string scenePath, string outputPath)
    {
        GD.Print($"Loading scene: {scenePath}");
        var packed = GD.Load<PackedScene>(scenePath);
        if (packed == null)
        {
            GD.PrintErr($"Could not load packed scene {scenePath}");
            return;
        }

        var instance = packed.Instantiate();
        Root.AddChild(instance);

        // Wait a few frames for layout, ready, shaders, and rendering
        for (int i = 0; i < 10; i++)
        {
            await ToSignal(this, SceneTree.SignalName.ProcessFrame);
        }

        var img = Root.GetTexture().GetImage();
        if (img != null)
        {
            var err = img.SavePng(outputPath);
            GD.Print($"Saved screenshot to {outputPath} (status: {err})");
        }
        else
        {
            GD.PrintErr($"Root viewport image was null for {scenePath}");
        }

        instance.QueueFree();
        await ToSignal(this, SceneTree.SignalName.ProcessFrame);
    }
}
