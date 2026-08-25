using Godot;
using System;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class Yakumono : Pocket
{
    public const int JackpotFaceIndex = -1;

    [Export]
    public Sprite2D FrameSprite { get; set; }

    [Export]
    public Sprite2D FaceSprite { get; set; }

    [Export]
    public Sprite2D ForegroundSprite { get; set; }

    [Export]
    public Node2D FrameProcedural { get; set; }

    [Export]
    public Node2D FaceProcedural { get; set; }

    [Export]
    public Node2D ForegroundProcedural { get; set; }

    [Export]
    public Node2D VisualLayersContainer { get; set; }

    [Export]
    public int VisualZIndex { get; set; } = 2;

    public int CurrentFaceIndex { get; private set; } = 0;

    public bool IsJackpotState => CurrentFaceIndex == JackpotFaceIndex;

    private readonly Random _random = new();

    public override void _Ready()
    {
        base._Ready();
    }

    public override void ApplyVisualConfig(VisualConfig config)
    {
        base.ApplyVisualConfig(config);
        if (config == null) return;

        if (VisualLayersContainer != null)
        {
            VisualLayersContainer.ZIndex = VisualZIndex;
        }

        Vector2 scaleVector = Vector2.One * config.YakumonoScale;
        if (FrameSprite != null) FrameSprite.Scale = scaleVector;
        if (FaceSprite != null) FaceSprite.Scale = scaleVector;
        if (ForegroundSprite != null) ForegroundSprite.Scale = scaleVector;

        // Frame Layer
        if (config.FrameTexture != null)
        {
            if (FrameSprite != null)
            {
                FrameSprite.Texture = config.FrameTexture;
                FrameSprite.Visible = true;
            }
            if (FrameProcedural != null)
            {
                FrameProcedural.Visible = false;
            }
        }
        else
        {
            if (FrameSprite != null)
            {
                FrameSprite.Visible = false;
            }
            if (FrameProcedural != null)
            {
                FrameProcedural.Visible = true;
                FrameProcedural.Modulate = config.YakumonoBaseColor;
            }
        }

        // Face Layer
        Texture2D activeFaceTexture = null;
        if (CurrentFaceIndex == JackpotFaceIndex)
        {
            activeFaceTexture = config.JackpotFaceTexture;
        }
        else if (config.FaceTextures != null && config.FaceTextures.Count > 0)
        {
            int idx = Mathf.Clamp(CurrentFaceIndex, 0, config.FaceTextures.Count - 1);
            activeFaceTexture = config.FaceTextures[idx];
        }

        if (activeFaceTexture != null)
        {
            if (FaceSprite != null)
            {
                FaceSprite.Texture = activeFaceTexture;
                FaceSprite.Visible = true;
            }
            if (FaceProcedural != null)
            {
                FaceProcedural.Visible = false;
            }
        }
        else
        {
            if (FaceSprite != null)
            {
                FaceSprite.Visible = false;
            }
            if (FaceProcedural != null)
            {
                FaceProcedural.Visible = true;
                FaceProcedural.Modulate = config.YakumonoBaseColor;
            }
        }

        // Foreground Layer
        if (config.ForegroundTexture != null)
        {
            if (ForegroundSprite != null)
            {
                ForegroundSprite.Texture = config.ForegroundTexture;
                ForegroundSprite.Visible = true;
            }
            if (ForegroundProcedural != null)
            {
                ForegroundProcedural.Visible = false;
            }
        }
        else
        {
            if (ForegroundSprite != null)
            {
                ForegroundSprite.Visible = false;
            }
            if (ForegroundProcedural != null)
            {
                ForegroundProcedural.Visible = true;
                ForegroundProcedural.Modulate = config.YakumonoBaseColor;
            }
        }
    }

    public void TransitionToFaceState(int faceIndex)
    {
        CurrentFaceIndex = faceIndex;
        var activeConfig = ConfigOverride ?? VisualConfig.LoadDefault();
        if (activeConfig != null)
        {
            ApplyVisualConfig(activeConfig);
        }
        GlobalEvents.Instance?.NotifyYakumonoStateChanged(this, CurrentFaceIndex);
    }

    public void TransitionToRandomFaceState()
    {
        var activeConfig = ConfigOverride ?? VisualConfig.LoadDefault();
        var faceTextures = activeConfig?.FaceTextures;
        if (faceTextures == null || faceTextures.Count == 0)
        {
            TransitionToFaceState(0);
            return;
        }

        if (faceTextures.Count == 1)
        {
            TransitionToFaceState(0);
            return;
        }

        int nextIndex = CurrentFaceIndex;
        while (nextIndex == CurrentFaceIndex)
        {
            nextIndex = _random.Next(0, faceTextures.Count);
        }

        TransitionToFaceState(nextIndex);
    }

    public void TransitionToJackpotState()
    {
        CurrentFaceIndex = JackpotFaceIndex;
        var activeConfig = ConfigOverride ?? VisualConfig.LoadDefault();
        if (activeConfig != null)
        {
            ApplyVisualConfig(activeConfig);
        }
        GlobalEvents.Instance?.NotifyYakumonoPaidOut(this);
    }

    protected override void OnBallCatch(Ball ball)
    {
        TransitionToRandomFaceState();
        base.OnBallCatch(ball);
    }

    protected override void OnCentralPocketPaidOut()
    {
        base.OnCentralPocketPaidOut();
        TransitionToJackpotState();
    }
}
