using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class Yakumono : Pocket
{
    public const int JackpotFaceIndex = -1;

    [Export]
    public Sprite2D? FrameSprite { get; set; }

    [Export]
    public Sprite2D? FaceSprite { get; set; }

    [Export]
    public Sprite2D? ForegroundSprite { get; set; }

    [Export]
    public Node2D? VisualLayersContainer { get; set; }

    [Export]
    public int VisualZIndex { get; set; } = 2;

    [Export]
    public Array<Texture2D>? FaceTextures { get; set; }

    [Export]
    public Texture2D? JackpotFaceTexture { get; set; }

    public int CurrentFaceIndex { get; private set; } = 0;

    public bool IsJackpotState => CurrentFaceIndex == JackpotFaceIndex;

    private readonly Random _random = new();

    public override void _Ready()
    {
        base._Ready();

        if (VisualLayersContainer != null)
        {
            VisualLayersContainer.ZIndex = VisualZIndex;
        }

        TransitionToFaceState(0);
    }

    public void TransitionToFaceState(int faceIndex)
    {
        CurrentFaceIndex = faceIndex;
        if (FaceTextures != null && FaceTextures.Count > 0)
        {
            int idx = Mathf.Clamp(CurrentFaceIndex, 0, FaceTextures.Count - 1);
            SetFaceTexture(FaceTextures[idx]);
        }
        GlobalEvents.Instance?.NotifyYakumonoStateChanged(this, CurrentFaceIndex);
    }

    public void TransitionToRandomFaceState()
    {
        if (FaceTextures == null || FaceTextures.Count == 0)
        {
            TransitionToFaceState(0);
            return;
        }

        if (FaceTextures.Count == 1)
        {
            TransitionToFaceState(0);
            return;
        }

        int nextIndex = CurrentFaceIndex;
        while (nextIndex == CurrentFaceIndex)
        {
            nextIndex = _random.Next(0, FaceTextures.Count);
        }

        TransitionToFaceState(nextIndex);
    }

    public void TransitionToJackpotState()
    {
        CurrentFaceIndex = JackpotFaceIndex;
        SetFaceTexture(JackpotFaceTexture);
        GlobalEvents.Instance?.NotifyYakumonoPaidOut(this);
    }

    private void SetFaceTexture(Texture2D? texture)
    {
        if (FaceSprite == null) return;
        FaceSprite.Texture = texture;
        FaceSprite.Visible = texture != null;
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

