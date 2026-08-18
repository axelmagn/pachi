using Godot;
using System;
using System.Diagnostics;

[GlobalClass]
public partial class Pin : StaticBody2D
{
    [Export]
    public Node2D Sprite { get; set; }

    [Export]
    public CpuParticles2D SparkParticles { get; set; }

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
    /// <summary>
    /// Minimum impact strength required to trigger spark particles. Hits below this threshold omit particles entirely.
    /// </summary>
    [Export]
    public float MinParticleImpactThreshold { get; set; } = 50.0f;

    /// <summary>
    /// Impact strength at which particle emission reaches maximum intensity.
    /// </summary>
    [Export]
    public float MaxParticleImpactThreshold { get; set; } = 500.0f;

    /// <summary>
    /// Particle count emitted for hits at the minimum impact threshold.
    /// </summary>
    [Export]
    public int MinParticleAmount { get; set; } = 1;

    /// <summary>
    /// Particle count emitted for hits at or above maximum impact threshold.
    /// </summary>
    [Export]
    public int MaxParticleAmount { get; set; } = 4;

    /// <summary>
    /// Speed multiplier for particles emitted at minimum impact threshold.
    /// </summary>
    [Export(PropertyHint.Range, "0.1,1.0")]
    public float MinParticleSpeedScale { get; set; } = 0.4f;

    /// <summary>
    /// Size multiplier for particles emitted at minimum impact threshold.
    /// </summary>
    [Export(PropertyHint.Range, "0.1,1.0")]
    public float MinParticleSizeScale { get; set; } = 0.5f;

    private Tween _pulseTween;
    private Vector2 _baseScale = Vector2.One;
    private Color _baseModulate = Colors.White;

    private float _baseInitialVelocityMin = 50.0f;
    private float _baseInitialVelocityMax = 100.0f;
    private float _baseScaleAmountMin = 1.5f;
    private float _baseScaleAmountMax = 2.5f;

    public override void _Ready()
    {
        Debug.Assert(Sprite != null, "Pin requires a Sprite reference.");
        _baseScale = Sprite.Scale;
        _baseModulate = Sprite.Modulate;

        if (SparkParticles != null)
        {
            _baseInitialVelocityMin = SparkParticles.InitialVelocityMin;
            _baseInitialVelocityMax = SparkParticles.InitialVelocityMax;
            _baseScaleAmountMin = SparkParticles.ScaleAmountMin;
            _baseScaleAmountMax = SparkParticles.ScaleAmountMax;
        }
    }

    public void NotifyHit(Vector2 impactPosition, Vector2 impactNormal, float impactStrength)
    {
        if (EnableVisualPulse && Sprite != null)
        {
            if (_pulseTween != null && _pulseTween.IsValid())
            {
                _pulseTween.Kill();
            }

            Sprite.Position = impactNormal * RecoilDistance;
            Sprite.Scale = _baseScale * PulseScale;
            Sprite.Modulate = FlashColor;

            _pulseTween = GetTree().CreateTween().SetParallel(true);

            _pulseTween.TweenProperty(Sprite, (NodePath)Node2D.PropertyName.Scale.ToString(), _baseScale, PulseDuration)
                .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

            _pulseTween.TweenProperty(Sprite, (NodePath)Node2D.PropertyName.Position.ToString(), Vector2.Zero, PulseDuration)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

            _pulseTween.TweenProperty(Sprite, (NodePath)CanvasItem.PropertyName.Modulate.ToString(), _baseModulate, PulseDuration)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }

        if (EnableSparkParticles && SparkParticles != null)
        {
            // GD.Print("Pin Impact Strength: ", impactStrength);
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
        if (Sprite is CircleSprite circleSprite)
        {
            return circleSprite.Radius * Scale.X;
        }
        return 2.0f;
    }
}
