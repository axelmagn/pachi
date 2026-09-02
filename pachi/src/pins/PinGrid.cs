using Godot;
using System;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class PinGrid : PinGenerator
{
    private int _rows = 5;
    private int _columns = 5;
    private float _spacingX = 16.0f; // Standard flow pathway
    private float _spacingY = 16.0f;
    private float _rowOffset = 0.5f;

    [Export]
    public int Rows
    {
        get => _rows;
        set
        {
            int clamped = Math.Max(1, value);
            if (_rows == clamped) return;
            _rows = clamped;
            Rebuild();
        }
    }

    [Export]
    public int Columns
    {
        get => _columns;
        set
        {
            int clamped = Math.Max(1, value);
            if (_columns == clamped) return;
            _columns = clamped;
            Rebuild();
        }
    }

    [Export]
    public float SpacingX
    {
        get => _spacingX;
        set
        {
            if (Mathf.IsEqualApprox(_spacingX, value)) return;
            _spacingX = value;
            Rebuild();
        }
    }

    [Export]
    public float SpacingY
    {
        get => _spacingY;
        set
        {
            if (Mathf.IsEqualApprox(_spacingY, value)) return;
            _spacingY = value;
            Rebuild();
        }
    }

    [Export]
    public float RowOffset
    {
        get => _rowOffset;
        set
        {
            if (Mathf.IsEqualApprox(_rowOffset, value)) return;
            _rowOffset = value;
            Rebuild();
        }
    }

    protected override void GeneratePins()
    {
        Debug.Assert(_columns > 0);

        for (int r = 0; r < _rows; r++)
        {
            int rowColumns = _columns - (r % 2);
            float currentOffset = _rowOffset * _spacingX * (r % 2);
            for (int c = 0; c < rowColumns; c++)
            {
                Vector2 position = new((c * _spacingX) + currentOffset, r * _spacingY);

                SpawnPin(position);
            }
        }
    }
}
