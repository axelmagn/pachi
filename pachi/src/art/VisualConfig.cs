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
    private Color _backgroundColor = new Color("#1C261D");

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
    private Color _pinBaseColor = new Color("#B9CBD9");
    private Texture2D? _pinTexture;
    private float _pinTextureScale = 1.0f;
    private Vector2 _pinTextureOffset = Vector2.Zero;
    private Color _flashColor = new Color("#F6E8A9");

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
    private Color _inputIndicatorBackgroundColor = new Color("#1A2433");
    private Color _outputIndicatorBackgroundColor = new Color("#33221A");
    private Color _indicatorBorderColor = new Color("#304A31");
    private Color _armColor = new Color("#7B924E");
    private Texture2D? _armTexture;
    private float _armTextureScale = 1.0f;
    private Vector2 _armTextureOffset = Vector2.Zero;

    [ExportGroup("Pockets")]
    [Export]
    public Color InputIndicatorBackgroundColor
    {
        get => _inputIndicatorBackgroundColor;
        set
        {
            _inputIndicatorBackgroundColor = value;
            EmitChanged();
        }
    }

    [Export]
    public Color OutputIndicatorBackgroundColor
    {
        get => _outputIndicatorBackgroundColor;
        set
        {
            _outputIndicatorBackgroundColor = value;
            EmitChanged();
        }
    }

    public Color IndicatorBackgroundColor
    {
        get => _inputIndicatorBackgroundColor;
        set
        {
            _inputIndicatorBackgroundColor = value;
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

    private Texture2D? _pocketTexture;
    private float _pocketTextureScale = 1.0f;
    private Vector2 _pocketTextureOffset = Vector2.Zero;

    [Export]
    public Texture2D? PocketTexture
    {
        get => _pocketTexture;
        set
        {
            _pocketTexture = value;
            EmitChanged();
        }
    }

    [Export]
    public float PocketTextureScale
    {
        get => _pocketTextureScale;
        set
        {
            _pocketTextureScale = value;
            EmitChanged();
        }
    }

    [Export]
    public Vector2 PocketTextureOffset
    {
        get => _pocketTextureOffset;
        set
        {
            _pocketTextureOffset = value;
            EmitChanged();
        }
    }

    // -------------------------------------------------------------------------
    // Cards & UI
    // -------------------------------------------------------------------------
    private Color _cardBackgroundColor = new Color("#452A21");
    private Color _cardBorderColor = new Color("#D2814A");
    private Color _cardIndicatorBackgroundColor = new Color("#1C261D");

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
    private Texture2D? _jackpotFaceTexture;
    private Color _yakumonoBaseColor = new Color("#CC6542");
    private Texture2D? _frameTexture;
    private Texture2D? _foregroundTexture;
    private float _yakumonoScale = 1.0f;

    [ExportGroup("Yakumono")]
    [Export]
    public float YakumonoScale
    {
        get => _yakumonoScale;
        set
        {
            _yakumonoScale = value;
            EmitChanged();
        }
    }
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
    public Texture2D? JackpotFaceTexture
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
    public Texture2D? FrameTexture
    {
        get => _frameTexture;
        set
        {
            _frameTexture = value;
            EmitChanged();
        }
    }

    [Export]
    public Texture2D? ForegroundTexture
    {
        get => _foregroundTexture;
        set
        {
            _foregroundTexture = value;
            EmitChanged();
        }
    }

    // -------------------------------------------------------------------------
    // Ball Tiers
    // -------------------------------------------------------------------------
    private Color _ballTier1Color = new Color("#F3E8AA");
    private Color _ballTier2Color = new Color("#EAB879");
    private Color _ballTier3Color = new Color("#D1814C");
    private Color _ballTier4Color = new Color("#CA6642");
    private Color _ballTier5Color = new Color("#C04D38");

    [ExportGroup("Ball Tiers")]
    [Export]
    public Color BallTier1Color
    {
        get => _ballTier1Color;
        set
        {
            _ballTier1Color = value;
            EmitChanged();
        }
    }

    [Export]
    public Color BallTier2Color
    {
        get => _ballTier2Color;
        set
        {
            _ballTier2Color = value;
            EmitChanged();
        }
    }

    [Export]
    public Color BallTier3Color
    {
        get => _ballTier3Color;
        set
        {
            _ballTier3Color = value;
            EmitChanged();
        }
    }

    [Export]
    public Color BallTier4Color
    {
        get => _ballTier4Color;
        set
        {
            _ballTier4Color = value;
            EmitChanged();
        }
    }

    [Export]
    public Color BallTier5Color
    {
        get => _ballTier5Color;
        set
        {
            _ballTier5Color = value;
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
