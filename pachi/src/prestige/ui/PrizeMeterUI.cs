using Godot;
using System;

[GlobalClass]
public partial class PrizeMeterUI : HBoxContainer
{
    [Signal]
    public delegate void ResetRequestedEventHandler();

    private Label? _titleLabel;
    private ProgressBar? _progressBar;
    private Label? _progressLabel;
    private Label? _tokenLabel;
    private Button? _resetButton;

    [Export]
    public Label? TitleLabel
    {
        get
        {
            if (_titleLabel == null) InitControls();
            return _titleLabel;
        }
        set => _titleLabel = value;
    }

    [Export]
    public ProgressBar? ProgressBar
    {
        get
        {
            if (_progressBar == null) InitControls();
            return _progressBar;
        }
        set => _progressBar = value;
    }

    [Export]
    public Label? ProgressLabel
    {
        get
        {
            if (_progressLabel == null) InitControls();
            return _progressLabel;
        }
        set => _progressLabel = value;
    }

    [Export]
    public Label? TokenLabel
    {
        get
        {
            if (_tokenLabel == null) InitControls();
            return _tokenLabel;
        }
        set => _tokenLabel = value;
    }

    [Export]
    public Button? ResetButton
    {
        get
        {
            if (_resetButton == null) InitControls();
            return _resetButton;
        }
        set => _resetButton = value;
    }

    private PrizeMeter? _meter;

    public override void _Ready()
    {
        InitControls();
    }

    public void InitControls()
    {
        if (_progressBar != null && _progressLabel != null && _tokenLabel != null && _resetButton != null)
        {
            return;
        }

        CustomMinimumSize = new Vector2(0, 32);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        Alignment = AlignmentMode.Center;
        AddThemeConstantOverride("separation", 12);

        // Clear existing children if re-initializing
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        _titleLabel = new Label
        {
            Name = "TitleLabel",
            Text = "PRIZE METER",
            VerticalAlignment = VerticalAlignment.Center
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 12);
        _titleLabel.Modulate = new Color(0.9f, 0.85f, 0.5f);
        AddChild(_titleLabel);

        // Progress bar with overlaid text
        var progressContainer = new PanelContainer
        {
            Name = "ProgressContainer",
            CustomMinimumSize = new Vector2(240, 20),
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        AddChild(progressContainer);

        _progressBar = new ProgressBar
        {
            Name = "ProgressBar",
            MinValue = 0,
            MaxValue = 100,
            Value = 0,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(240, 20),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        progressContainer.AddChild(_progressBar);

        _progressLabel = new Label
        {
            Name = "ProgressLabel",
            Text = "0 / 100 pts",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _progressLabel.AddThemeFontSizeOverride("font_size", 10);
        progressContainer.AddChild(_progressLabel);

        _tokenLabel = new Label
        {
            Name = "TokenLabel",
            Text = "Tokens: 0",
            VerticalAlignment = VerticalAlignment.Center
        };
        _tokenLabel.AddThemeFontSizeOverride("font_size", 12);
        _tokenLabel.Modulate = new Color(1.0f, 0.84f, 0.0f);
        AddChild(_tokenLabel);

        _resetButton = new Button
        {
            Name = "ResetButton",
            Text = "Prestige Reset",
            Disabled = true,
            CustomMinimumSize = new Vector2(110, 24),
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        _resetButton.AddThemeFontSizeOverride("font_size", 11);
        _resetButton.Pressed += OnResetButtonPressed;
        AddChild(_resetButton);
    }

    public void Bind(PrizeMeter meter)
    {
        InitControls();

        if (_meter != null)
        {
            _meter.ProgressChanged -= OnMeterProgressChanged;
            _meter.PrizeTokenAwarded -= OnPrizeTokenAwarded;
            _meter.MeterReset -= OnMeterReset;
        }

        _meter = meter;

        _meter.ProgressChanged += OnMeterProgressChanged;
        _meter.PrizeTokenAwarded += OnPrizeTokenAwarded;
        _meter.MeterReset += OnMeterReset;

        UpdateDisplay(_meter.CurrentProgress, _meter.CurrentTargetCapacity, _meter.ProgressPercent, _meter.TotalTokens, _meter.TokensEarnedInRun);
    }

    public override void _ExitTree()
    {
        if (_meter != null)
        {
            _meter.ProgressChanged -= OnMeterProgressChanged;
            _meter.PrizeTokenAwarded -= OnPrizeTokenAwarded;
            _meter.MeterReset -= OnMeterReset;
        }

        if (ResetButton != null)
        {
            ResetButton.Pressed -= OnResetButtonPressed;
        }
    }

    public void OnResetButtonPressed()
    {
        EmitSignal(SignalName.ResetRequested);
    }

    private void UpdateDisplay(float current, float target, float percent, int totalTokens, int tokensInRun)
    {
        if (ProgressBar != null)
        {
            ProgressBar.Value = percent * 100.0f;
        }

        if (ProgressLabel != null)
        {
            ProgressLabel.Text = $"{(int)current} / {(int)target} pts ({percent * 100.0f:F0}%)";
        }

        if (TokenLabel != null)
        {
            TokenLabel.Text = $"Tokens: {totalTokens}";
        }

        if (ResetButton != null)
        {
            bool canReset = _meter?.CanPrestigeReset ?? (totalTokens >= 1 || tokensInRun >= 1);
            ResetButton.Disabled = !canReset;
            if (canReset)
            {
                ResetButton.Modulate = new Color(1.0f, 1.0f, 1.0f);
            }
            else
            {
                ResetButton.Modulate = new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }

    private void OnMeterProgressChanged(float currentProgress, float targetCapacity, float percent)
    {
        int total = _meter?.TotalTokens ?? 0;
        int inRun = _meter?.TokensEarnedInRun ?? 0;
        UpdateDisplay(currentProgress, targetCapacity, percent, total, inRun);
    }

    private void OnPrizeTokenAwarded(int totalTokens, int tokensInRun)
    {
        if (_meter != null)
        {
            UpdateDisplay(_meter.CurrentProgress, _meter.CurrentTargetCapacity, _meter.ProgressPercent, totalTokens, tokensInRun);
        }
    }

    private void OnMeterReset()
    {
        if (_meter != null)
        {
            UpdateDisplay(_meter.CurrentProgress, _meter.CurrentTargetCapacity, _meter.ProgressPercent, _meter.TotalTokens, _meter.TokensEarnedInRun);
        }
    }
}
