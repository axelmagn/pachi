using Godot;
using Godot.Collections;
using System;

[Tool]
[GlobalClass]
public partial class PocketBallsIndicator : Node2D
{
    private bool _isInputIndicator = true;
    private Color _backgroundColor = new(0.14f, 0.14f, 0.14f);
    private bool _showQuestionMark = false;
    private Color _questionMarkColor = Colors.White;
    private Vector2 _size = new(34, 10);
    private float _dotRadius = 1.5f;
    private Color _borderColor = Colors.Black;
    private float _borderThickness = 1.0f;
    private Array<BallVariant>? _balls;
    private readonly StyleBoxFlat _pipStyleBox = new()
    {
        BorderWidthLeft = 1,
        BorderWidthTop = 1,
        BorderWidthRight = 1,
        BorderWidthBottom = 1,
        CornerRadiusTopLeft = 2,
        CornerRadiusTopRight = 2,
        CornerRadiusBottomRight = 2,
        CornerRadiusBottomLeft = 2,
    };

    [Export]
    public bool IsInputIndicator
    {
        get => _isInputIndicator;
        set
        {
            if (_isInputIndicator == value) return;
            _isInputIndicator = value;
            QueueRedraw();
        }
    }

    [Export]
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor == value) return;
            _backgroundColor = value;
            QueueRedraw();
        }
    }

    [Export]
    public bool ShowQuestionMark
    {
        get => _showQuestionMark;
        set
        {
            if (_showQuestionMark == value) return;
            _showQuestionMark = value;
            QueueRedraw();
        }
    }

    [Export]
    public Color QuestionMarkColor
    {
        get => _questionMarkColor;
        set
        {
            if (_questionMarkColor == value) return;
            _questionMarkColor = value;
            QueueRedraw();
        }
    }

    [Export]
    public Vector2 Size
    {
        get => _size;
        set
        {
            if (_size == value) return;
            _size = value;
            QueueRedraw();
        }
    }

    [Export]
    public float PipSize { get; set; } = 8.0f;

    [Export]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor == value) return;
            _borderColor = value;
            QueueRedraw();
        }
    }

    [Export]
    public float BorderThickness
    {
        get => _borderThickness;
        set
        {
            if (Mathf.IsEqualApprox(_borderThickness, value)) return;
            _borderThickness = value;
            QueueRedraw();
        }
    }

    [Export]
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

    private void UpdateIndicatorSize()
    {
        _size = (_balls == null || _balls.Count <= 4) ? new Vector2(34, 10) : new Vector2(34, 18);
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
            DrawString(font, new Vector2(-3, 4), "?", HorizontalAlignment.Center, -1, 11, QuestionMarkColor);
            return;
        }

        // draw squircle pips
        if (Balls == null) return;
        int numDots = Math.Min(Balls.Count, 8);
        if (numDots <= 0) return;

        float pipSize = PipSize;
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

                _pipStyleBox.BgColor = pipColor;
                _pipStyleBox.BorderColor = strokeColor;
                DrawStyleBox(_pipStyleBox, pipRect);
            }
        }
    }
}
