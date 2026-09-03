using Godot;
using Godot.Collections;

/// <summary>
/// Specialized central pocket featuring animated reactive facial states and jackpot payout behavior.
/// </summary>
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

    public override void _Ready()
    {
        base._Ready();

        // Keep character face layers visible above background board components.
        if (VisualLayersContainer != null)
        {
            VisualLayersContainer.ZIndex = VisualZIndex;
        }

        TransitionToFaceState(0);
    }

    /// <summary>
    /// Updates sprite texture and notifies listeners of character emotion state changes.
    /// </summary>
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

    /// <summary>
    /// Selects a distinct new random face expression upon ball impact.
    /// </summary>
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

        // Guarantee a noticeable expression change on every ball catch.
        int nextIndex = CurrentFaceIndex;
        while (nextIndex == CurrentFaceIndex)
        {
            nextIndex = GameConfig.Instance?.Rng != null
                ? GameConfig.Instance.Rng.Next(0, FaceTextures.Count)
                : (int)(GD.Randi() % (uint)FaceTextures.Count);
        }

        TransitionToFaceState(nextIndex);
    }

    /// <summary>
    /// Displays the celebratory jackpot texture and announces payout completion.
    /// </summary>
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
        // Animate expression reaction before triggering base pocket ball accumulation logic.
        TransitionToRandomFaceState();
        base.OnBallCatch(ball);
    }

    protected override void OnCentralPocketPaidOut()
    {
        base.OnCentralPocketPaidOut();
        TransitionToJackpotState();
    }
}
