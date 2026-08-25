using Godot;
using Godot.Collections;
using System;

[Tool]
[GlobalClass]
public partial class PocketBallsIndicator : Node2D
{
    private readonly VisualConfigBinding _binding;
    private VisualConfig _configOverride;

    public PocketBallsIndicator()
    {
        _binding = new VisualConfigBinding(ApplyVisualConfig);
    }

    [Export]
    public VisualConfig ConfigOverride
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
    private float _borderThickness = 2.0f;

    public Array<BallVariant> Balls
    {
        get => _balls;
        set { _balls = value; QueueRedraw(); }
    }
    private Array<BallVariant> _balls;

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

    public void ApplyVisualConfig(VisualConfig config)
    {
        if (config == null) return;
        BackgroundColor = IsCardIndicator ? config.CardIndicatorBackgroundColor : config.IndicatorBackgroundColor;
        BorderColor = config.IndicatorBorderColor;
        QueueRedraw();
    }

    /// <summary>
    /// Draw an array of circular dots with a rectangle background, centered on the node position.
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

        // draw dots
        if (Balls == null) return;
        int numDots = Balls.Count;
        if (numDots <= 0 || DotRadius <= 0) return;

        float spacing = DotRadius;
        float step = DotRadius * 3.0f; // dot diameter (2 * DotRadius) + spacing (DotRadius)

        int dotsPerRow = Math.Max(1, (int)Math.Floor((Size.X + spacing) / step));
        int numRows = (int)Math.Ceiling((float)numDots / dotsPerRow);

        int dotIndex = 0;
        for (int r = 0; r < numRows; r++)
        {
            int dotsInThisRow = Math.Min(dotsPerRow, numDots - dotIndex);
            float rowY = -0.5f * (numRows - 1) * step + r * step;

            for (int c = 0; c < dotsInThisRow; c++)
            {
                float dotX = -0.5f * (dotsInThisRow - 1) * step + c * step;
                Color dotColor = Balls[dotIndex].PlaceholderColor;
                DrawCircle(new Vector2((int)dotX, (int)rowY), DotRadius, dotColor);
                dotIndex++;
            }
        }
    }
}
