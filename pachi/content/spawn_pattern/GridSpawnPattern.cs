using Godot;
using System.Collections.Generic;

[Tool]
public partial class GridSpawnPattern : Node2D
{
    private Vector2 _spacing = new Vector2(64, 64);
    private Vector2I _cardinality = new Vector2I(8, 4);
    private PackedScene _spawnScn;
    private bool _startOddRow = false;

    [Export]
    public Vector2 Spacing
    {
        get => _spacing;
        set
        {
            _spacing = value;
            if (IsNodeReady()) UpdateChildren();
        }
    }

    [Export]
    public Vector2I Cardinality
    {
        get => _cardinality;
        set
        {
            _cardinality = value;
            if (IsNodeReady()) UpdateChildren();
        }
    }

    [Export]
    public PackedScene SpawnScn
    {
        get => _spawnScn;
        set
        {
            _spawnScn = value;
            if (IsNodeReady())
            {
                ClearChildren();
                UpdateChildren();
            }
        }
    }

    [Export]
    public bool StartOddRow
    {
        get => _startOddRow;
        set
        {
            _startOddRow = value;
            if (IsNodeReady()) UpdateChildren();
        }
    }

    public override void _Ready()
    {
        UpdateChildren();
    }

    private void UpdateChildren()
    {
        if (_spawnScn == null || _cardinality.X < 1 || _cardinality.Y < 1 || _spacing.X == 0 || _spacing.Y == 0)
        {
            ClearChildren();
            return;
        }

        int rowAdjust = _startOddRow ? 1 : 0;
        int childrenCount = GetChildCount();
        int requestedCount = _cardinality.X * _cardinality.Y - _cardinality.Y / 2 - rowAdjust * (_cardinality.Y % 2);

        if (childrenCount < requestedCount)
        {
            int createCount = requestedCount - childrenCount;
            for (int i = 0; i < createCount; i++)
            {
                Node child = _spawnScn.Instantiate();
                AddChild(child);
                if (Engine.IsEditorHint())
                {
                    child.Owner = GetTree().EditedSceneRoot;
                }
            }
        }
        else if (childrenCount > requestedCount)
        {
            int deleteCount = childrenCount - requestedCount;
            var children = GetChildren();
            for (int i = 0; i < deleteCount; i++)
            {
                Node child = children[i];
                RemoveChild(child);
                child.QueueFree();
            }
        }

        Vector2 offset = new Vector2(_spacing.X * (_cardinality.X - 1) / -2.0f, 0);
        Vector2 position = Vector2.Zero;
        int row = 0;
        int col = 0;
        int width = _cardinality.X - rowAdjust;

        position.X += rowAdjust * _spacing.X / 2.0f;

        foreach (Node child in GetChildren())
        {
            if (child is Node2D child2D)
            {
                child2D.Position = offset + position;
            }

            col++;
            position.X += _spacing.X;
            if (col >= width)
            {
                row++;
                col = 0;
                width = _cardinality.X - (row + rowAdjust) % 2;
                position.X = _spacing.X / 2.0f * ((row + rowAdjust) % 2);
                position.Y = row * _spacing.Y;
            }
        }
    }

    private void ClearChildren()
    {
        foreach (Node child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }
    }
}
