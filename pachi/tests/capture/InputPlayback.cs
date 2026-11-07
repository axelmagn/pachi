using System.Collections.Generic;
using Godot;

/// <summary>
/// Debug-only: replays a tests/input/*.json script by injecting input events
/// on fixed physics ticks (counted from when this node enters the tree).
/// </summary>
/// <remarks>
/// Script format: a JSON array of entries, each with a "frame" plus either
/// an action — <c>{ "frame": 30, "action": "launcher_charge", "pressed": true }</c> —
/// or a mouse button — <c>{ "frame": 30, "mouse": { "x": 480, "y": 270, "button": 1, "pressed": true } }</c>.
/// </remarks>
public partial class InputPlayback : Node
{
    private readonly SortedDictionary<long, List<InputEvent>> _events = new();
    private long _frame;

    public static InputPlayback FromFile(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            throw new System.IO.FileNotFoundException($"input script not found: {path}");
        }
        Variant parsed = Json.ParseString(file.GetAsText());
        if (parsed.VariantType != Variant.Type.Array)
        {
            throw new System.FormatException($"input script must be a JSON array: {path}");
        }

        var playback = new InputPlayback { Name = nameof(InputPlayback) };
        foreach (Variant entryVariant in parsed.AsGodotArray())
        {
            var entry = entryVariant.AsGodotDictionary();
            long frame = entry["frame"].AsInt64();
            playback.Add(frame, ToEvent(entry, path));
        }
        return playback;
    }

    public override void _Ready()
    {
        Input.UseAccumulatedInput = false;
        // Run before gameplay nodes so they see the input on the same tick.
        ProcessPhysicsPriority = int.MinValue;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_events.Remove(_frame, out List<InputEvent>? due))
        {
            foreach (InputEvent inputEvent in due)
            {
                Input.ParseInputEvent(inputEvent);
            }
        }
        _frame++;
    }

    private void Add(long frame, InputEvent inputEvent)
    {
        if (!_events.TryGetValue(frame, out List<InputEvent>? list))
        {
            list = [];
            _events[frame] = list;
        }
        list.Add(inputEvent);
    }

    private static InputEvent ToEvent(Godot.Collections.Dictionary entry, string path)
    {
        if (entry.TryGetValue("action", out Variant action))
        {
            return new InputEventAction
            {
                Action = action.AsStringName(),
                Pressed = entry["pressed"].AsBool(),
                Strength = 1.0f,
            };
        }
        if (entry.TryGetValue("mouse", out Variant mouseVariant))
        {
            var mouse = mouseVariant.AsGodotDictionary();
            var position = new Vector2(mouse["x"].AsSingle(), mouse["y"].AsSingle());
            return new InputEventMouseButton
            {
                ButtonIndex = (MouseButton)mouse["button"].AsInt32(),
                Pressed = mouse["pressed"].AsBool(),
                Position = position,
                GlobalPosition = position,
            };
        }
        throw new System.FormatException($"input entry needs 'action' or 'mouse': {path}");
    }
}
