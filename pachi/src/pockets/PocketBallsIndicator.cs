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
    private Vector2 _size = new(32, 16);

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
        set { _balls = value; QueueRedraw(); }
    }
    private Array<BallVariant>? _balls;

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
        int numDots = Balls.Count;
        if (numDots <= 0) return;

        float borderInset = BorderThickness > 0.0f ? BorderThickness : 1.0f;
        float availableHeight = Math.Max(2.0f, Size.Y - 2.0f * borderInset);
        float availableWidth = Math.Max(2.0f, Size.X - 2.0f * borderInset);
        float spacing = 2.0f;

        float pipSize = availableHeight;
        if (numDots * pipSize + (numDots - 1) * spacing > availableWidth)
        {
            pipSize = Math.Max(2.0f, (availableWidth - (numDots - 1) * spacing) / numDots);
        }

        float totalWidth = numDots * pipSize + (numDots - 1) * spacing;
        float startX = -totalWidth / 2.0f;
        float startY = -pipSize / 2.0f;
        int cornerRadius = Math.Max(1, (int)MathF.Round(pipSize * 0.25f));

        for (int i = 0; i < numDots; i++)
        {
            float pipX = startX + i * (pipSize + spacing);
            Rect2 pipRect = new Rect2(pipX, startY, pipSize, pipSize);
            Color pipColor = Balls[i].PlaceholderColor;
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
