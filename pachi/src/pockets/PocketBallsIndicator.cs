using Godot;
using Godot.Collections;
using System;

[Tool]
[GlobalClass]
public partial class PocketBallsIndicator : Node2D
{
    private readonly VisualConfigBinding _binding;
    private VisualConfig? _configOverride;

    public PocketBallsIndicator()
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
    public bool IsInputIndicator
    {
        get => _isInputIndicator;
        set
        {
            _isInputIndicator = value;
            if (_binding.ActiveConfig != null)
            {
                ApplyVisualConfig(_binding.ActiveConfig);
            }
            QueueRedraw();
        }
    }
    private bool _isInputIndicator = true;

    [Export]
    public bool IsCardIndicator
    {
        get => _isCardIndicator;
        set
        {
            _isCardIndicator = value;
            UpdateIndicatorSize();
            if (_binding.ActiveConfig != null)
            {
                ApplyVisualConfig(_binding.ActiveConfig);
            }
            QueueRedraw();
        }
    }
    private bool _isCardIndicator = false;

    [Export]
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set { _backgroundColor = value; QueueRedraw(); }
    }
    private Color _backgroundColor = new(0.14f, 0.14f, 0.14f);

    [Export]
    public bool ShowQuestionMark
    {
        get => _showQuestionMark;
        set { _showQuestionMark = value; QueueRedraw(); }
    }
    private bool _showQuestionMark = false;

    [Export]
    public Vector2 Size
    {
        get => _size;
        set { _size = value; QueueRedraw(); }
    }
    private Vector2 _size = new(34, 10);

    [Export]
    public float DotRadius
    {
        get => _dotRadius;
        set { _dotRadius = value; QueueRedraw(); }
    }
    private float _dotRadius = 1.5f;

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

    private void UpdateIndicatorSize()
    {
        if (IsCardIndicator)
        {
            int count = _balls != null ? Math.Clamp(_balls.Count, 0, 8) : 0;
            _size = new Vector2(Math.Max(10.0f, count * 8.0f + 2.0f), 10.0f);
        }
        else
        {
            _size = (_balls == null || _balls.Count <= 4) ? new Vector2(34, 10) : new Vector2(34, 18);
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

    public void ApplyVisualConfig(VisualConfig? config)
    {
        if (config == null) return;
        if (IsCardIndicator)
        {
            BackgroundColor = config.CardIndicatorBackgroundColor;
        }
        else if (IsInputIndicator)
        {
            BackgroundColor = config.InputIndicatorBackgroundColor;
        }
        else
        {
            BackgroundColor = config.OutputIndicatorBackgroundColor;
        }
        BorderColor = config.IndicatorBorderColor;
        QueueRedraw();
    }

    /// <summary>
    /// Draw an array of squircle pips with a rectangle background, centered on the node position.
    /// </summary>
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

        if (ShowQuestionMark)
        {
            Font font = ThemeDB.FallbackFont;
            DrawString(font, new Vector2(-3, 4), "?", HorizontalAlignment.Center, -1, 11, Colors.White);
            return;
        }

        // draw squircle pips
        if (Balls == null) return;
        int numDots = Math.Min(Balls.Count, 8);
        if (numDots <= 0) return;

        const float pipSize = 8.0f;
        const int cornerRadius = 2;
        int numRows = numDots > 4 ? 2 : 1;

        for (int r = 0; r < numRows; r++)
        {
            int countInRow = (r == 0) ? Math.Min(numDots, 4) : (numDots - 4);
            float totalWidth = countInRow * pipSize;
            float startX = -totalWidth / 2.0f;
            float startY = (numRows == 1) ? -pipSize / 2.0f : (r == 0 ? -pipSize : 0.0f);

            for (int c = 0; c < countInRow; c++)
            {
                int ballIndex = r * 4 + c;
                float pipX = startX + c * pipSize;
                Rect2 pipRect = new Rect2(pipX, startY, pipSize, pipSize);
                Color pipColor = Balls[ballIndex].PlaceholderColor;
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
