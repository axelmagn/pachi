using Godot;

[Tool]
[GlobalClass]
public partial class LauncherModeIndicator : Node2D
{
    [Signal]
    public delegate void ModeChangedEventHandler(bool isAutoFiring, float interval);

    public struct ModePreset
    {
        public bool IsAutoFiring;
        public float Interval;

        public ModePreset(bool auto, float interval)
        {
            IsAutoFiring = auto;
            Interval = interval;
        }
    }

    public static readonly ModePreset[] Presets = new ModePreset[]
    {
        new ModePreset(false, 1.0f),
        new ModePreset(true, 1.0f),
        new ModePreset(true, 0.6f),
        new ModePreset(true, 0.4f),
        new ModePreset(true, 0.2f),
    };

    private int _currentPresetIndex = 0;
    private float _pulseTimer = 0.0f;

    [Export]
    public bool IsAutoFiring
    {
        get => Presets[_currentPresetIndex].IsAutoFiring;
        set
        {
            SyncPresetIndex(value, AutoFireInterval);
            QueueRedraw();
        }
    }

    [Export]
    public float AutoFireInterval
    {
        get => Presets[_currentPresetIndex].Interval;
        set
        {
            SyncPresetIndex(IsAutoFiring, value);
            QueueRedraw();
        }
    }

    public int CurrentPresetIndex => _currentPresetIndex;

    public override void _Process(double delta)
    {
        if (IsAutoFiring)
        {
            _pulseTimer += (float)delta * 5.0f;
            QueueRedraw();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Engine.IsEditorHint()) return;

        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            Vector2 localPos = ToLocal(GetGlobalMousePosition());
            Vector2 size = new Vector2(90, 22);
            Rect2 rect = new Rect2(-size / 2.0f, size);

            if (rect.HasPoint(localPos))
            {
                if (mouseButton.ButtonIndex == MouseButton.Left)
                {
                    CycleMode(forward: true);
                    GetViewport().SetInputAsHandled();
                }
                else if (mouseButton.ButtonIndex == MouseButton.Right)
                {
                    CycleMode(forward: false);
                    GetViewport().SetInputAsHandled();
                }
            }
        }
    }

    public void CycleMode(bool forward)
    {
        if (forward)
        {
            _currentPresetIndex = (_currentPresetIndex + 1) % Presets.Length;
        }
        else
        {
            _currentPresetIndex = (_currentPresetIndex - 1 + Presets.Length) % Presets.Length;
        }

        var preset = Presets[_currentPresetIndex];
        EmitSignal(SignalName.ModeChanged, preset.IsAutoFiring, preset.Interval);
        QueueRedraw();
    }

    private void SyncPresetIndex(bool autoFiring, float interval)
    {
        for (int i = 0; i < Presets.Length; i++)
        {
            if (Presets[i].IsAutoFiring == autoFiring && Mathf.IsEqualApprox(Presets[i].Interval, interval))
            {
                _currentPresetIndex = i;
                return;
            }
        }

        if (!autoFiring)
        {
            _currentPresetIndex = 0;
        }
    }

    public override void _Draw()
    {
        Vector2 size = new Vector2(90, 22);
        Rect2 rect = new Rect2(-size / 2.0f, size);

        Color bg = IsAutoFiring ? new Color(0.08f, 0.22f, 0.12f, 0.9f) : new Color(0.12f, 0.12f, 0.15f, 0.75f);
        Color border = IsAutoFiring ? new Color(0.2f, 0.85f, 0.45f, 0.95f) : new Color(0.4f, 0.4f, 0.5f, 0.5f);

        DrawRect(rect, bg, filled: true);
        DrawRect(rect, border, filled: false, width: 1.5f);

        // Draw status LED indicator dot
        Vector2 dotPos = new Vector2(-size.X / 2.0f + 10.0f, 0);
        Color dotColor = IsAutoFiring
            ? new Color(0.3f, 1.0f, 0.5f, 0.5f + 0.5f * Mathf.Sin(_pulseTimer))
            : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        DrawCircle(dotPos, 3.5f, dotColor);

        // Text
        string text = IsAutoFiring ? $"auto ({AutoFireInterval:F1}s)" : "manual";
        Font font = ThemeDB.FallbackFont;
        Color textColor = IsAutoFiring ? new Color(0.85f, 1.0f, 0.9f) : new Color(0.7f, 0.7f, 0.75f);
        DrawString(font, new Vector2(-size.X / 2.0f + 17.0f, 4.0f), text, HorizontalAlignment.Left, -1, 10, textColor);
    }
}
