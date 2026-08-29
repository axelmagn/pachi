using Godot;
using Godot.Collections;
using System;

[Tool]
[GlobalClass]
public partial class BallAwardIndicator : Node2D
{
    private readonly VisualConfigBinding _binding;
    private VisualConfig? _configOverride;

    public BallAwardIndicator()
    {
        _binding = new VisualConfigBinding(ApplyVisualConfig);
    }

    [Export]
    public VisualConfig? ConfigOverride
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

    [Export]
    public int MaxColumns
    {
        get => _maxColumns;
        set
        {
            _maxColumns = Math.Max(1, value);
            UpdateIndicatorSize();
            QueueRedraw();
        }
    }
    private int _maxColumns = 6;

    [Export]
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set { _backgroundColor = value; QueueRedraw(); }
    }
    private Color _backgroundColor = new(0.14f, 0.14f, 0.14f);

    [Export]
    public Vector2 Size
    {
        get => _size;
        set { _size = value; QueueRedraw(); }
    }
    private Vector2 _size = new(10, 10);

    [Export]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; QueueRedraw(); }
    }
    private Color _borderColor = Colors.Black;

    [Export]
    public float BorderThickness
    {
        get => _borderThickness;
        set { _borderThickness = value; QueueRedraw(); }
    }
    private float _borderThickness = 1.0f;

    public Array<BallVariant>? Balls
    {
        get => _balls;
        set
        {
            _balls = value;
            UpdateIndicatorSize();
            QueueRedraw();
        }
    }
    private Array<BallVariant>? _balls;

    public void UpdateIndicatorSize()
    {
        int totalBalls = _balls != null ? _balls.Count : 0;
        int cols = Math.Clamp(totalBalls, 1, MaxColumns);
        int rows = totalBalls > 0 ? (int)Math.Ceiling((float)totalBalls / MaxColumns) : 1;

        float width = cols * 8.0f + 2.0f;
        float height = rows * 8.0f + 2.0f;
        _size = new Vector2(width, height);
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

    public void ApplyVisualConfig(VisualConfig? config)
    {
        if (config == null) return;
        BackgroundColor = config.CardIndicatorBackgroundColor;
        BorderColor = config.IndicatorBorderColor;
        QueueRedraw();
    }

    public override void _Draw()
    {
        Rect2 rect = new Rect2(-Size / 2.0f, Size);
        DrawRect(rect, BackgroundColor);

        if (BorderThickness > 0.0f)
        {
            DrawRect(rect, BorderColor, filled: false, width: BorderThickness);
        }

        if (Balls == null || Balls.Count == 0) return;

        const float pipSize = 8.0f;
        const int cornerRadius = 2;

        int totalBalls = Balls.Count;
        int numRows = (int)Math.Ceiling((float)totalBalls / MaxColumns);
        float totalGridHeight = numRows * pipSize;
        float startGridY = -totalGridHeight / 2.0f;

        for (int r = 0; r < numRows; r++)
        {
            int startIdx = r * MaxColumns;
            int countInRow = Math.Min(MaxColumns, totalBalls - startIdx);
            float rowWidth = countInRow * pipSize;
            float startX = -rowWidth / 2.0f;
            float startY = startGridY + r * pipSize;

            for (int c = 0; c < countInRow; c++)
            {
                int ballIdx = startIdx + c;
                float pipX = startX + c * pipSize;
                Rect2 pipRect = new Rect2(pipX, startY, pipSize, pipSize);
                Color pipColor = Balls[ballIdx].PlaceholderColor;
                float lum = 0.299f * pipColor.R + 0.587f * pipColor.G + 0.114f * pipColor.B;
                Color strokeColor = lum < 0.35f
                    ? pipColor.Lightened(0.25f)
                    : pipColor.Darkened(0.35f);

                var styleBox = new StyleBoxFlat
                {
                    BgColor = pipColor,
                    BorderColor = strokeColor,
                    BorderWidthLeft = 1,
                    BorderWidthTop = 1,
                    BorderWidthRight = 1,
                    BorderWidthBottom = 1,
                    CornerRadiusTopLeft = cornerRadius,
                    CornerRadiusTopRight = cornerRadius,
                    CornerRadiusBottomRight = cornerRadius,
                    CornerRadiusBottomLeft = cornerRadius,
                    AntiAliasing = false,
                };
                DrawStyleBox(styleBox, pipRect);
            }
        }
    }
}
