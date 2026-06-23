using Godot;
using System;
using System.Diagnostics;

[Tool]
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
        set { _rows = Math.Max(1, value); Rebuild(); }
    }

    [Export]
    public int Columns
    {
        get => _columns;
        set { _columns = Math.Max(1, value); Rebuild(); }
    }

    [Export]
    public float SpacingX
    {
        get => _spacingX;
        set { _spacingX = value; Rebuild(); }
    }

    [Export]
    public float SpacingY
    {
        get => _spacingY;
        set { _spacingY = value; Rebuild(); }
    }

    [Export]
    public float RowOffset
    {
        get => _rowOffset;
        set { _rowOffset = value; Rebuild(); }
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
