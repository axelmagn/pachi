using Godot;
using System;

[GlobalClass]
[Tool]
public partial class VisualConfig : Resource
{
    public const string DefaultPath = "res://src/art/visual_config.tres";

    // -------------------------------------------------------------------------
    // Environment
    // -------------------------------------------------------------------------
    private Color _backgroundColor = new Color(0.075f, 0.075f, 0.075f, 1.0f);

    [ExportGroup("Environment")]
    [Export]
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            _backgroundColor = value;
            EmitChanged();
        }
    }

    // -------------------------------------------------------------------------
    // Pins
    // -------------------------------------------------------------------------
    private Color _pinBaseColor = Colors.White;
    private Texture2D? _pinTexture;
    private float _pinTextureScale = 1.0f;
    private Vector2 _pinTextureOffset = Vector2.Zero;
    private Color _flashColor = new Color(1.0f, 0.85f, 0.2f, 1.0f);

    [ExportGroup("Pins")]
    [Export]
    public Color PinBaseColor
    {
        get => _pinBaseColor;
        set
        {
            _pinBaseColor = value;
            EmitChanged();
        }
    }

    [Export]
    public Texture2D? PinTexture
    {
        get => _pinTexture;
        set
        {
            _pinTexture = value;
            EmitChanged();
        }
    }

    [Export]
    public float PinTextureScale
    {
        get => _pinTextureScale;
        set
        {
            _pinTextureScale = value;
            EmitChanged();
        }
    }

    [Export]
    public Vector2 PinTextureOffset
    {
        get => _pinTextureOffset;
        set
        {
            _pinTextureOffset = value;
            EmitChanged();
        }
    }

    [Export]
    public Color FlashColor
    {
        get => _flashColor;
        set
        {
            _flashColor = value;
            EmitChanged();
        }
    }

    // -------------------------------------------------------------------------
    // Pockets
    // -------------------------------------------------------------------------
    private Color _indicatorBackgroundColor = new Color(0.087f, 0.087f, 0.087f, 1.0f);
    private Color _indicatorBorderColor = Colors.Black;
    private Color _armColor = Colors.White;
    private Texture2D? _armTexture;
    private float _armTextureScale = 1.0f;
    private Vector2 _armTextureOffset = Vector2.Zero;

    [ExportGroup("Pockets")]
    [Export]
    public Color IndicatorBackgroundColor
    {
        get => _indicatorBackgroundColor;
        set
        {
            _indicatorBackgroundColor = value;
            EmitChanged();
        }
    }

    [Export]
    public Color IndicatorBorderColor
    {
        get => _indicatorBorderColor;
        set
        {
            _indicatorBorderColor = value;
            EmitChanged();
        }
    }

    [Export]
    public Color ArmColor
    {
        get => _armColor;
        set
        {
            _armColor = value;
            EmitChanged();
        }
    }

    [Export]
    public Texture2D? ArmTexture
    {
        get => _armTexture;
        set
        {
            _armTexture = value;
            EmitChanged();
        }
    }

    [Export]
    public float ArmTextureScale
    {
        get => _armTextureScale;
        set
        {
            _armTextureScale = value;
            EmitChanged();
        }
    }

    [Export]
    public Vector2 ArmTextureOffset
    {
        get => _armTextureOffset;
        set
        {
            _armTextureOffset = value;
            EmitChanged();
        }
    }

    // -------------------------------------------------------------------------
    // Cards & UI
    // -------------------------------------------------------------------------
    private Color _cardBackgroundColor = new Color(0.2f, 0.4f, 0.8f, 1.0f);
    private Color _cardBorderColor = new Color(1.0f, 1.0f, 1.0f, 0.4f);
    private Color _cardIndicatorBackgroundColor = new Color(0.14f, 0.14f, 0.14f, 1.0f);

    [ExportGroup("Cards & UI")]
    [Export]
    public Color CardBackgroundColor
    {
        get => _cardBackgroundColor;
        set
        {
            _cardBackgroundColor = value;
            EmitChanged();
        }
    }

    [Export]
    public Color CardBorderColor
    {
        get => _cardBorderColor;
        set
        {
            _cardBorderColor = value;
            EmitChanged();
        }
    }

    [Export]
    public Color CardIndicatorBackgroundColor
    {
        get => _cardIndicatorBackgroundColor;
        set
        {
            _cardIndicatorBackgroundColor = value;
            EmitChanged();
        }
    }

    // -------------------------------------------------------------------------
    // Yakumono
    // -------------------------------------------------------------------------
    private Godot.Collections.Array<Texture2D> _faceTextures = new();
    private Texture2D _jackpotFaceTexture;
    private Color _yakumonoBaseColor = new Color(0.85f, 0.35f, 0.35f, 1.0f);
    private Texture2D _frameTexture;
    private Texture2D _foregroundTexture;

    [ExportGroup("Yakumono")]
    [Export]
    public Godot.Collections.Array<Texture2D> FaceTextures
    {
        get => _faceTextures;
        set
        {
            _faceTextures = value ?? new Godot.Collections.Array<Texture2D>();
            EmitChanged();
        }
    }

    [Export]
    public Texture2D JackpotFaceTexture
    {
        get => _jackpotFaceTexture;
        set
        {
            _jackpotFaceTexture = value;
            EmitChanged();
        }
    }

    [Export]
    public Color YakumonoBaseColor
    {
        get => _yakumonoBaseColor;
        set
        {
            _yakumonoBaseColor = value;
            EmitChanged();
        }
    }

    [Export]
    public Texture2D FrameTexture
    {
        get => _frameTexture;
        set
        {
            _frameTexture = value;
            EmitChanged();
        }
    }

    [Export]
    public Texture2D ForegroundTexture
    {
        get => _foregroundTexture;
        set
        {
            _foregroundTexture = value;
            EmitChanged();
        }
    }

    // -------------------------------------------------------------------------
    // Helper Loader
    // -------------------------------------------------------------------------
    public static VisualConfig? LoadDefault()
    {
        if (ResourceLoader.Exists(DefaultPath))
        {
            return ResourceLoader.Load<VisualConfig>(DefaultPath);
        }
        return null;
    }
}
