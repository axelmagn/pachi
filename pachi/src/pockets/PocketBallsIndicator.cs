using Godot;
using System;

[Tool]
[GlobalClass]
public partial class PocketBallsIndicator : Node2D
{
    [Export]
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            QueueRedraw();
        }
    }
    private Color _backgroundColor = Colors.White;

    [Export]
    public Vector2 Size
    {
        get => _size;
        set
        {
            _size = value;
            QueueRedraw();
        }
    }
    private Vector2 _size = new(32, 16);

    [Export]
    public int NumDots
    {
        get => _numDots;
        set
        {
            _numDots = value;
            QueueRedraw();
        }
    }
    private int _numDots = 3;

    [Export]
    public float DotRadius
    {
        get => _dotRadius;
        set
        {
            _dotRadius = value;
            QueueRedraw();
        }
    }
    private float _dotRadius = 1.5f;

    [Export]
    public Color DotColor
    {
        get => _dotColor;
        set
        {
            _dotColor = value;
            QueueRedraw();
        }
    }
    private Color _dotColor = Colors.Gray;

    [Export]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            QueueRedraw();
        }
    }
    private Color _borderColor = Colors.Black;

    [Export]
    public float BorderThickness
    {
        get => _borderThickness;
        set
        {
            _borderThickness = value;
            QueueRedraw();
        }
    }
    private float _borderThickness = 2.0f;


    /// Draw an array of circular dots with a rectangle background, centered on the node position.
    public override void _Draw()
    {
        // draw background
        Rect2 rect = new Rect2(-Size / 2.0f, Size);
        DrawRect(rect, BackgroundColor);

        // draw border
        if (BorderThickness > 0.0f)
        {
            DrawRect(rect, BorderColor, filled: false, width: BorderThickness);
        }

        // draw dots
        if (NumDots <= 0 || DotRadius <= 0) return;

        float spacing = DotRadius;
        float step = DotRadius * 3.0f; // dot diameter (2 * DotRadius) + spacing (DotRadius)

        int dotsPerRow = Math.Max(1, (int)Math.Floor((Size.X + spacing) / step));
        int numRows = (int)Math.Ceiling((float)NumDots / dotsPerRow);

        int dotIndex = 0;
        for (int r = 0; r < numRows; r++)
        {
            int dotsInThisRow = Math.Min(dotsPerRow, NumDots - dotIndex);
            float rowY = -0.5f * (numRows - 1) * step + r * step;

            for (int c = 0; c < dotsInThisRow; c++)
            {
                float dotX = -0.5f * (dotsInThisRow - 1) * step + c * step;
                DrawCircle(new Vector2(dotX, rowY), DotRadius, DotColor);
                dotIndex++;
            }
        }
    }
}
