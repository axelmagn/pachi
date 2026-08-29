using Godot;
using System;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class Pin : StaticBody2D
{
    private readonly VisualConfigBinding _binding;
    private VisualConfig? _configOverride;

    public Pin()
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
    public Node2D? ProceduralSprite { get; set; }

    [Export]
    public Sprite2D? TextureSprite { get; set; }

    /// <summary>
    /// Legacy alias for ProceduralSprite for backward compatibility.
    /// </summary>
    [Export]
    public Node2D? Sprite
    {
        get => ProceduralSprite;
        set => ProceduralSprite = value;
    }

    [Export]
    public CpuParticles2D? SparkParticles { get; set; }

    [Export]
    public bool EnableSparkParticles { get; set; } = true;

    [Export]
    public bool EnableVisualPulse { get; set; } = true;

    [Export]
    public float PulseScale { get; set; } = 1.75f;

    [Export]
    public float PulseDuration { get; set; } = 0.12f;

    [Export]
    public float RecoilDistance { get; set; } = 2.0f;

    [Export]
    public Color FlashColor { get; set; } = new Color(1.0f, 0.85f, 0.2f, 1.0f);

    [ExportGroup("Particle Modulation")]
    [Export]
    public float MinParticleImpactThreshold { get; set; } = 50.0f;

    [Export]
    public float MaxParticleImpactThreshold { get; set; } = 500.0f;

    [Export]
    public int MinParticleAmount { get; set; } = 1;

    [Export]
    public int MaxParticleAmount { get; set; } = 4;

    [Export(PropertyHint.Range, "0.1,1.0")]
    public float MinParticleSpeedScale { get; set; } = 0.4f;

    [Export(PropertyHint.Range, "0.1,1.0")]
    public float MinParticleSizeScale { get; set; } = 0.5f;

    public Node2D? ActiveVisual => (TextureSprite != null && TextureSprite.Visible)
        ? TextureSprite
        : ProceduralSprite;

    private Tween? _pulseTween;
    private Vector2 _baseScale = Vector2.One;
    private Vector2 _basePosition = Vector2.Zero;
    private Color _baseModulate = Colors.White;

    private float _baseInitialVelocityMin = 50.0f;
    private float _baseInitialVelocityMax = 100.0f;
    private float _baseScaleAmountMin = 1.5f;
    private float _baseScaleAmountMax = 2.5f;

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

        if (Engine.IsEditorHint()) return;

        Debug.Assert(ProceduralSprite != null || TextureSprite != null, "Pin requires either a ProceduralSprite or TextureSprite reference.");

        var targetSprite = ActiveVisual;
        if (targetSprite != null)
        {
            _baseScale = targetSprite.Scale;
            _basePosition = targetSprite.Position;
            _baseModulate = targetSprite.Modulate;
        }

        if (SparkParticles != null)
        {
            _baseInitialVelocityMin = SparkParticles.InitialVelocityMin;
            _baseInitialVelocityMax = SparkParticles.InitialVelocityMax;
            _baseScaleAmountMin = SparkParticles.ScaleAmountMin;
            _baseScaleAmountMax = SparkParticles.ScaleAmountMax;
        }
    }

    public void ApplyVisualConfig(VisualConfig? config)
    {
        if (config == null) return;

        FlashColor = config.FlashColor;

        if (config.PinTexture != null)
        {
            if (TextureSprite != null)
            {
                TextureSprite.Texture = config.PinTexture;
                TextureSprite.Scale = Vector2.One * config.PinTextureScale;
                TextureSprite.Position = config.PinTextureOffset;
                TextureSprite.Visible = true;
                TextureSprite.Modulate = Colors.White;
            }
            if (ProceduralSprite != null)
            {
                ProceduralSprite.Visible = false;
            }
        }
        else
        {
            if (TextureSprite != null)
            {
                TextureSprite.Visible = false;
            }

            if (ProceduralSprite != null)
            {
                ProceduralSprite.Visible = true;
                if (ProceduralSprite is CircleSprite circleSprite)
                {
                    circleSprite.Color = Colors.White;
                }
                ProceduralSprite.Modulate = config.PinBaseColor;
            }
        }

        var active = ActiveVisual;
        if (active != null)
        {
            _baseScale = active.Scale;
            _basePosition = active.Position;
            _baseModulate = active.Modulate;
        }
    }

    public void NotifyHit(Vector2 impactPosition, Vector2 impactNormal, float impactStrength)
    {
        if (Engine.IsEditorHint()) return;

        var targetSprite = ActiveVisual;
        if (EnableVisualPulse && targetSprite != null)
        {
            if (_pulseTween != null && _pulseTween.IsValid())
            {
                _pulseTween.Kill();
            }

            targetSprite.Position = _basePosition + impactNormal * RecoilDistance;
            targetSprite.Scale = _baseScale * PulseScale;
            targetSprite.Modulate = FlashColor;

            _pulseTween = GetTree().CreateTween().SetParallel(true);

            _pulseTween.TweenProperty(targetSprite, Node2D.PropertyName.Scale.ToString(), _baseScale, PulseDuration)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

            _pulseTween.TweenProperty(targetSprite, Node2D.PropertyName.Position.ToString(), _basePosition, PulseDuration)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

            _pulseTween.TweenProperty(targetSprite, CanvasItem.PropertyName.Modulate.ToString(), _baseModulate, PulseDuration)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }

        if (EnableSparkParticles && SparkParticles != null)
        {
            if (impactStrength >= MinParticleImpactThreshold)
            {
                float range = Math.Max(1.0f, MaxParticleImpactThreshold - MinParticleImpactThreshold);
                float t = Mathf.Clamp((impactStrength - MinParticleImpactThreshold) / range, 0.0f, 1.0f);

                int particleAmount = Mathf.RoundToInt(Mathf.Lerp(MinParticleAmount, MaxParticleAmount, t));
                float speedScale = Mathf.Lerp(MinParticleSpeedScale, 1.0f, t);
                float sizeScale = Mathf.Lerp(MinParticleSizeScale, 1.0f, t);

                SparkParticles.GlobalPosition = impactPosition;
                SparkParticles.Rotation = impactNormal.Angle();
                SparkParticles.Amount = Math.Max(1, particleAmount);
                SparkParticles.InitialVelocityMin = _baseInitialVelocityMin * speedScale;
                SparkParticles.InitialVelocityMax = _baseInitialVelocityMax * speedScale;
                SparkParticles.ScaleAmountMin = _baseScaleAmountMin * sizeScale;
                SparkParticles.ScaleAmountMax = _baseScaleAmountMax * sizeScale;
                SparkParticles.Restart();
            }
        }
    }

    public float GetRadius()
    {
        var visual = ActiveVisual;
        if (visual is CircleSprite circleSprite)
        {
            return circleSprite.Radius * Scale.X;
        }
        if (visual is Sprite2D sprite2D && sprite2D.Texture != null)
        {
            return (sprite2D.Texture.GetSize().X / 2.0f) * sprite2D.Scale.X * Scale.X;
        }
        return 2.0f;
    }
}
