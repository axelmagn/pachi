using System;
using Godot;

/// <summary>
/// Debug-only entry point for scripts/capture.sh: seeds the RNGs, loads
/// --scene, and replays --input through <see cref="InputPlayback"/>.
/// </summary>
[GlobalClass]
public partial class CaptureRunner : SceneTree
{
    public override void _Initialize()
    {
        try
        {
            HarnessArgs.ApplySeed();

            string scene = HarnessArgs.Get("scene")
                ?? (string)ProjectSettings.GetSetting("application/run/main_scene");
            string? input = HarnessArgs.Get("input");
            if (input != null)
            {
                Root.AddChild(InputPlayback.FromFile(input));
            }

            Error err = ChangeSceneToFile(scene);
            if (err != Error.Ok)
            {
                throw new InvalidOperationException($"could not load scene {scene}: {err}");
            }
            GD.Print($"Capture: scene={scene} input={input ?? "(none)"}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Capture failed: {ex}");
            Quit(1);
        }
    }
}
