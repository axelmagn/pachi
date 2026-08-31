using Godot;
using System;
using System.Diagnostics;

[Tool]
public abstract partial class PinGenerator : Node2D
{

    private PackedScene? _pinScene;

    [Export]
    public PackedScene? PinScene
    {
        get => _pinScene;
        set
        {
            if (_pinScene == value) return;
            _pinScene = value;
            Rebuild();
        }
    }

    public override void _Ready()
    {
        if (GetChildCount() == 0)
        {
            Rebuild();
        }
    }

    public void Rebuild()
    {
        ClearPins();
        if (PinScene == null) return;
        GeneratePins();
    }

    protected void ClearPins()
    {
        for (int i = GetChildCount() - 1; i >= 0; i--)
        {
            Node child = GetChild(i);
            RemoveChild(child);
            child.QueueFree();
        }
    }

    protected abstract void GeneratePins();

    protected void SpawnPin(Vector2 position, float rotation = 0.0f)
    {
        Debug.Assert(PinScene != null);
        Node2D pinInstance = PinScene.Instantiate<Node2D>();
        pinInstance.Position = position;
        pinInstance.Rotation = rotation;
        AddChild(pinInstance);

        // snap to pixel
        int x = (int)pinInstance.GlobalPosition.X;
        int y = (int)pinInstance.GlobalPosition.Y;
        pinInstance.GlobalPosition = new Vector2(x, y);

        // if (Engine.IsEditorHint())
        // {
        //     // pinInstance.Owner = GetTree().EditedSceneRoot;
        //     pinInstance.Owner = this;
        // }
    }
}
