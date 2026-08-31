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
            if (_backgroundColor == value) return;
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
            if (_pinBaseColor == value) return;
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
            if (_pinTexture == value) return;
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
            if (Mathf.IsEqualApprox(_pinTextureScale, value)) return;
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
            if (_pinTextureOffset == value) return;
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
            if (_flashColor == value) return;
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
            if (_inputIndicatorBackgroundColor == value) return;
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
            if (_outputIndicatorBackgroundColor == value) return;
            _outputIndicatorBackgroundColor = value;
            EmitChanged();
        }
    }

    public Color IndicatorBackgroundColor
    {
        get => _inputIndicatorBackgroundColor;
        set
        {
            if (_inputIndicatorBackgroundColor == value) return;
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
            if (_indicatorBorderColor == value) return;
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
            if (_armColor == value) return;
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
            if (_armTexture == value) return;
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
            if (Mathf.IsEqualApprox(_armTextureScale, value)) return;
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
            if (_armTextureOffset == value) return;
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
            if (_pocketTexture == value) return;
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
            if (Mathf.IsEqualApprox(_pocketTextureScale, value)) return;
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
            if (_pocketTextureOffset == value) return;
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
            if (_cardBackgroundColor == value) return;
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
            if (_cardBorderColor == value) return;
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
            if (_cardIndicatorBackgroundColor == value) return;
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
            if (Mathf.IsEqualApprox(_yakumonoScale, value)) return;
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
            if (_faceTextures == value) return;
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
            if (_jackpotFaceTexture == value) return;
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
            if (_yakumonoBaseColor == value) return;
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
            if (_frameTexture == value) return;
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
            if (_foregroundTexture == value) return;
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
            if (_ballTier1Color == value) return;
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
            if (_ballTier2Color == value) return;
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
            if (_ballTier3Color == value) return;
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
            if (_ballTier4Color == value) return;
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
            if (_ballTier5Color == value) return;
            _ballTier5Color = value;
            EmitChanged();
        }
    }

    // -------------------------------------------------------------------------
    // Helper Loader
    // -------------------------------------------------------------------------
    private static VisualConfig? _cachedDefault;

    public static VisualConfig? LoadDefault()
    {
        if (_cachedDefault != null && GodotObject.IsInstanceValid(_cachedDefault))
        {
            return _cachedDefault;
        }

        if (ResourceLoader.Exists(DefaultPath))
        {
            _cachedDefault = ResourceLoader.Load<VisualConfig>(DefaultPath);
            return _cachedDefault;
        }
        return null;
    }
}
