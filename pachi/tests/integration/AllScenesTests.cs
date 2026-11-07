using System.Collections.Generic;
using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Godot;
using Shouldly;

/// <summary>
/// Loads every scene under res://src, adds it to the tree for one frame, then
/// frees it. Load failures fail the test; runtime errors surface through the
/// verify.sh log scan.
/// </summary>
public partial class AllScenesTests : IntegrationTestClass
{
    private const string SceneRoot = "res://src";

    /// Scenes that only work wired into a parent (exports set by the parent,
    /// GameConfig/GlobalEvents present). They are covered through main_game.tscn.
    private static readonly HashSet<string> ContextOnlyScenes =
    [
        "res://src/hopper/hopper.tscn",     // needs GlobalEvents.Instance
        "res://src/launcher/launcher.tscn", // Hopper export is set by main_game
    ];

    public AllScenesTests(Node testScene) : base(testScene)
    {
    }

    [Test]
    public async Task EveryScene_LoadsAndSurvivesOneFrame()
    {
        List<string> scenes = FindScenes(SceneRoot);
        scenes.ShouldNotBeEmpty();
        foreach (string path in scenes)
        {
            GD.Print($"AllScenesTests: {path}");
            var packed = ResourceLoader.Load<PackedScene>(path);
            packed.ShouldNotBeNull($"failed to load {path}");
            Node instance = packed.Instantiate();
            instance.ShouldNotBeNull($"failed to instantiate {path}");
            TrackNode(instance);
            TestScene.AddChild(instance);
            await TestScene.ToSignal(TestScene.GetTree(), SceneTree.SignalName.ProcessFrame);
            instance.QueueFree();
            await TestScene.ToSignal(TestScene.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    private static List<string> FindScenes(string dir)
    {
        var result = new List<string>();
        foreach (string sub in DirAccess.GetDirectoriesAt(dir))
        {
            result.AddRange(FindScenes($"{dir}/{sub}"));
        }
        foreach (string file in DirAccess.GetFilesAt(dir))
        {
            string path = $"{dir}/{file}";
            if (file.EndsWith(".tscn", System.StringComparison.Ordinal) && !ContextOnlyScenes.Contains(path))
            {
                result.Add(path);
            }
        }
        result.Sort();
        return result;
    }
}
