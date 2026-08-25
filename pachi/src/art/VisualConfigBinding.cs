using Godot;
using System;

public class VisualConfigBinding : IDisposable
{
    private VisualConfig _activeConfig;
    private readonly Action<VisualConfig> _onConfigApplied;

    public VisualConfigBinding(Action<VisualConfig> onConfigApplied)
    {
        _onConfigApplied = onConfigApplied ?? throw new ArgumentNullException(nameof(onConfigApplied));
    }

    public VisualConfig ActiveConfig => _activeConfig;

    public void Bind(VisualConfig configOverride)
    {
        Unbind();
        _activeConfig = configOverride ?? VisualConfig.LoadDefault();
        if (_activeConfig != null)
        {
            _activeConfig.Changed += OnConfigChanged;
            _onConfigApplied(_activeConfig);
        }
    }

    public void Unbind()
    {
        if (_activeConfig != null)
        {
            _activeConfig.Changed -= OnConfigChanged;
            _activeConfig = null;
        }
    }

    private void OnConfigChanged()
    {
        if (_activeConfig != null)
        {
            _onConfigApplied(_activeConfig);
        }
    }

    public void Dispose()
    {
        Unbind();
    }
}
