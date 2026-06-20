using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class EllipseColliderSpawnPattern : Node2D
{
    private Vector2 _size = new Vector2(256, 128);
    private float _angleStartDeg = 0;
    private float _angleEndDeg = 360;
    private uint _segments = 32;
    private PackedScene _spawnScn;

    [Export]
    public Vector2 Size
    {
        get => _size;
        set
        {
            _size = value;
            if (IsNodeReady()) UpdateChildren();
        }
    }

    [Export]
    public float AngleStartDeg
    {
        get => _angleStartDeg;
        set
        {
            _angleStartDeg = value;
            if (IsNodeReady()) UpdateChildren();
        }
    }

    [Export]
    public float AngleEndDeg
    {
        get => _angleEndDeg;
        set
        {
            _angleEndDeg = value;
            if (IsNodeReady()) UpdateChildren();
        }
    }

    [Export]
    public uint Segments
    {
        get => _segments;
        set
        {
            _segments = value;
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

    public override void _Ready()
    {
        UpdateChildren();
    }

    private List<(Vector2 position, float rotation)> CalcPoints()
    {
        var outPoints = new List<(Vector2, float)>();

        if (Mathf.IsEqualApprox(_angleStartDeg, _angleEndDeg) || _segments == 0)
        {
            return outPoints;
        }

        float angleStart = Mathf.Min(_angleStartDeg, _angleEndDeg) * (float)Math.PI / 180.0f;
        float angleEnd = Mathf.Max(_angleStartDeg, _angleEndDeg) * (float)Math.PI / 180.0f;
        float step = (angleEnd - angleStart) / _segments;
        float t = angleStart;

        if (angleEnd - angleStart > 2.0f * (float)Math.PI)
        {
            angleEnd = angleStart + 2.0f * (float)Math.PI;
        }

        int nPoints = (int)_segments + 1;
        if (Mathf.IsEqualApprox(angleEnd - angleStart, 2.0f * (float)Math.PI))
        {
            nPoints--;
        }

        for (int i = 0; i < nPoints; i++)
        {
            float x = _size.X * 0.5f * Mathf.Cos(t);
            float y = _size.Y * 0.5f * Mathf.Sin(t);
            Vector2 point = new Vector2(x, y);

            float tNext = t + step;
            float xNext = _size.X * 0.5f * Mathf.Cos(tNext);
            float yNext = _size.Y * 0.5f * Mathf.Sin(tNext);
            Vector2 pointNext = new Vector2(xNext, yNext);

            float angle = point.AngleToPoint(pointNext) + (float)Math.PI * 0.5f;

            outPoints.Add((point, angle));
            t = tNext;
        }

        return outPoints;
    }

    private void UpdateChildren()
    {
        var points = CalcPoints();
        if (_spawnScn == null || points.Count < 2)
        {
            ClearChildren();
            return;
        }

        int childrenCount = GetChildCount();
        int requestedCount = points.Count;

        if (childrenCount < requestedCount)
        {
            int createCount = requestedCount - childrenCount;
            for (int i = 0; i < createCount; i++)
            {
                Node child = _spawnScn.Instantiate();
                AddChild(child);
                if (Engine.IsEditorHint())
                {
                    // child.Owner = GetTree().EditedSceneRoot;
                    child.Owner = this;
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

        var currentChildren = GetChildren();
        for (int i = 0; i < currentChildren.Count; i++)
        {
            if (currentChildren[i] is Node2D child2D)
            {
                child2D.Position = points[i].position;
                child2D.Rotation = points[i].rotation;
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
