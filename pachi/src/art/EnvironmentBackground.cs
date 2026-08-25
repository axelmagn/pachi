using Godot;
using System;

[GlobalClass]
[Tool]
public partial class EnvironmentBackground : ColorRect
{
    private readonly VisualConfigBinding _binding;
    private VisualConfig _configOverride;

    public EnvironmentBackground()
    {
        _binding = new VisualConfigBinding(ApplyVisualConfig);
    }

    [Export]
    public VisualConfig ConfigOverride
    {
        get => _configOverride;
        set
        {
            _configOverride = value;
            if (IsInsideTree())
            {
                _binding.Bind(_configOverride);
            }
        }
    }

    public override void _EnterTree()
    {
        _binding.Bind(_configOverride);
    }

    public override void _ExitTree()
    {
        _binding.Unbind();
    }

    public override void _Ready()
    {
        if (_binding.ActiveConfig != null)
        {
            ApplyVisualConfig(_binding.ActiveConfig);
        }
    }

    public void ApplyVisualConfig(VisualConfig config)
    {
        if (config == null) return;
        Color = config.BackgroundColor;
    }
}
