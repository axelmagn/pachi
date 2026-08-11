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

    private Tween _pulseTween;
    private Vector2 _baseScale = Vector2.One;
    private Color _baseModulate = Colors.White;

    public override void _Ready()
    {
        Debug.Assert(Sprite != null, "Pin requires a Sprite reference.");
        _baseScale = Sprite.Scale;
        _baseModulate = Sprite.Modulate;
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
            SparkParticles.GlobalPosition = impactPosition;
            SparkParticles.Rotation = impactNormal.Angle();
            SparkParticles.Restart();
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
