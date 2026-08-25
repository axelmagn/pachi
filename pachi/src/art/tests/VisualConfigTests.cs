using Godot;
using System;
using static TestAssert;

public static class VisualConfigTests
{

    public static void RunAllTests()
    {
        TestVisualConfigDefaults();
        TestVisualConfigPropertyChangesEmitChanged();
        TestBoundaryRectPropertyPropagation();
        TestPinDualRenderingFallback();
        TestPinTexturePriority();
        TestPocketDualRenderingFallback();
        TestPocketTexturePriority();
        TestPocketBallsIndicatorPropagation();
        TestCardUIStylingPropagation();
        TestYakumonoVisualConfigDefaults();
        TestYakumonoVisualConfigPropertyChangesEmitChanged();
        TestYakumonoDualRenderingFallback();
        TestYakumonoTexturePriority();
        TestYakumonoFaceStateTransitions();
        TestNullConfigGracefulHandling();
    }

    public static void TestVisualConfigDefaults()
    {
        var config = new VisualConfig();
        Assert(config.PinTextureScale == 1.0f, "PinTextureScale default should be 1.0f.");
        Assert(config.PinTextureOffset == Vector2.Zero, "PinTextureOffset default should be Vector2.Zero.");
        Assert(config.ArmTextureScale == 1.0f, "ArmTextureScale default should be 1.0f.");
        Assert(config.ArmTextureOffset == Vector2.Zero, "ArmTextureOffset default should be Vector2.Zero.");
    }

    public static void TestVisualConfigPropertyChangesEmitChanged()
    {
        var config = new VisualConfig();
        bool changedFired = false;
        config.Changed += () => { changedFired = true; };

        config.BackgroundColor = new Color(0.1f, 0.2f, 0.3f, 1.0f);
        Assert(changedFired, "Setting BackgroundColor should emit Changed.");

        changedFired = false;
        config.PinBaseColor = Colors.Red;
        Assert(changedFired, "Setting PinBaseColor should emit Changed.");

        changedFired = false;
        config.PinTextureScale = 2.0f;
        Assert(changedFired, "Setting PinTextureScale should emit Changed.");

        changedFired = false;
        config.PinTextureOffset = new Vector2(5.0f, 10.0f);
        Assert(changedFired, "Setting PinTextureOffset should emit Changed.");

        changedFired = false;
        config.ArmColor = Colors.Blue;
        Assert(changedFired, "Setting ArmColor should emit Changed.");

        changedFired = false;
        config.ArmTextureScale = 1.5f;
        Assert(changedFired, "Setting ArmTextureScale should emit Changed.");

        changedFired = false;
        config.ArmTextureOffset = new Vector2(-3.0f, 4.0f);
        Assert(changedFired, "Setting ArmTextureOffset should emit Changed.");

        changedFired = false;
        config.CardBackgroundColor = Colors.Green;
        Assert(changedFired, "Setting CardBackgroundColor should emit Changed.");
    }

    public static void TestBoundaryRectPropertyPropagation()
    {
        var boundary = new BoundaryRect();
        var config = new VisualConfig
        {
            BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f)
        };

        boundary.ApplyVisualConfig(config);

        Assert(boundary.BackgroundColor == config.BackgroundColor, "BackgroundColor should match VisualConfig.");
    }

    public static void TestPinDualRenderingFallback()
    {
        var pin = new Pin();
        var circleSprite = new CircleSprite();
        var sprite2D = new Sprite2D();
        pin.ProceduralSprite = circleSprite;
        pin.TextureSprite = sprite2D;

        var config = new VisualConfig
        {
            PinTexture = null,
            PinBaseColor = Colors.Coral,
            FlashColor = Colors.Yellow
        };

        pin.ApplyVisualConfig(config);

        Assert(circleSprite.Visible, "Procedural sprite should be visible when PinTexture is null.");
        Assert(!sprite2D.Visible, "Texture sprite should be hidden when PinTexture is null.");
        Assert(circleSprite.Modulate == Colors.Coral, "Procedural sprite modulate should match PinBaseColor.");
        Assert(pin.FlashColor == Colors.Yellow, "FlashColor should match VisualConfig.");
    }

    public static void TestPinTexturePriority()
    {
        var pin = new Pin();
        var circleSprite = new CircleSprite();
        var sprite2D = new Sprite2D();
        pin.ProceduralSprite = circleSprite;
        pin.TextureSprite = sprite2D;

        var texture = new ImageTexture();
        var config = new VisualConfig
        {
            PinTexture = texture,
            PinTextureScale = 0.5f,
            PinTextureOffset = new Vector2(3.0f, -4.0f)
        };

        pin.ApplyVisualConfig(config);

        Assert(!circleSprite.Visible, "Procedural sprite should be hidden when PinTexture is present.");
        Assert(sprite2D.Visible, "Texture sprite should be visible when PinTexture is present.");
        Assert(sprite2D.Texture == texture, "Texture sprite texture should match VisualConfig.PinTexture.");
        Assert(sprite2D.Scale == new Vector2(0.5f, 0.5f), "Texture sprite scale should match uniform PinTextureScale.");
        Assert(sprite2D.Position == new Vector2(3.0f, -4.0f), "Texture sprite position should match VisualConfig.PinTextureOffset.");
    }

    public static void TestPocketDualRenderingFallback()
    {
        var pocket = new Pocket();
        var leftProcedural = new CapsuleSprite();
        var rightProcedural = new CapsuleSprite();
        var leftSprite = new Sprite2D();
        var rightSprite = new Sprite2D();

        pocket.LeftArmProcedural = leftProcedural;
        pocket.RightArmProcedural = rightProcedural;
        pocket.LeftArmSprite = leftSprite;
        pocket.RightArmSprite = rightSprite;

        var config = new VisualConfig
        {
            ArmTexture = null,
            ArmColor = Colors.Lime
        };

        pocket.ApplyVisualConfig(config);

        Assert(leftProcedural.Visible && rightProcedural.Visible, "Procedural arms should be visible when ArmTexture is null.");
        Assert(!leftSprite.Visible && !rightSprite.Visible, "Arm sprites should be hidden when ArmTexture is null.");
        Assert(leftProcedural.Color == Colors.Lime && rightProcedural.Color == Colors.Lime, "Procedural arm color should match ArmColor.");
    }

    public static void TestPocketTexturePriority()
    {
        var pocket = new Pocket();
        var leftProcedural = new CapsuleSprite();
        var rightProcedural = new CapsuleSprite();
        var leftSprite = new Sprite2D();
        var rightSprite = new Sprite2D();

        pocket.LeftArmProcedural = leftProcedural;
        pocket.RightArmProcedural = rightProcedural;
        pocket.LeftArmSprite = leftSprite;
        pocket.RightArmSprite = rightSprite;

        var texture = new ImageTexture();
        var config = new VisualConfig
        {
            ArmTexture = texture,
            ArmTextureScale = 0.8f,
            ArmTextureOffset = new Vector2(4.0f, -6.0f)
        };

        pocket.ApplyVisualConfig(config);

        Assert(!leftProcedural.Visible && !rightProcedural.Visible, "Procedural arms should be hidden when ArmTexture is set.");
        Assert(leftSprite.Visible && rightSprite.Visible, "Arm sprites should be visible when ArmTexture is set.");
        Assert(leftSprite.Texture == texture && rightSprite.Texture == texture, "Arm textures should match VisualConfig.ArmTexture.");
        Assert(rightSprite.Scale == new Vector2(0.8f, 0.8f), "Right arm sprite scale should match uniform ArmTextureScale.");
        Assert(rightSprite.Position == new Vector2(4.0f, -6.0f), "Right arm sprite position should match ArmTextureOffset.");
        Assert(!rightSprite.FlipH, "Right arm sprite FlipH should be false.");
        Assert(leftSprite.Scale == new Vector2(0.8f, 0.8f), "Left arm sprite scale should match uniform ArmTextureScale.");
        Assert(leftSprite.Position == new Vector2(-4.0f, -6.0f), "Left arm sprite position should mirror ArmTextureOffset horizontally.");
        Assert(leftSprite.FlipH, "Left arm sprite FlipH should be true.");
    }

    public static void TestPocketBallsIndicatorPropagation()
    {
        var indicator = new PocketBallsIndicator();
        var config = new VisualConfig
        {
            IndicatorBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f),
            IndicatorBorderColor = Colors.White,
            CardIndicatorBackgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f)
        };

        indicator.IsCardIndicator = false;
        indicator.ApplyVisualConfig(config);
        Assert(indicator.BackgroundColor == config.IndicatorBackgroundColor, "Standard indicator should use IndicatorBackgroundColor.");
        Assert(indicator.BorderColor == config.IndicatorBorderColor, "Border color should match IndicatorBorderColor.");

        indicator.IsCardIndicator = true;
        indicator.ApplyVisualConfig(config);
        Assert(indicator.BackgroundColor == config.CardIndicatorBackgroundColor, "Card indicator should use CardIndicatorBackgroundColor.");
    }

    public static void TestCardUIStylingPropagation()
    {
        var cardUI = new CardUI();
        var config = new VisualConfig
        {
            CardBackgroundColor = new Color(0.7f, 0.1f, 0.2f, 1.0f),
            CardBorderColor = Colors.Gold
        };

        cardUI.ApplyVisualConfig(config);
        var style = cardUI.GetThemeStylebox(new StringName("panel")) as StyleBoxFlat;
        Assert(style != null, "CardUI should have a StyleBoxFlat panel override.");
        Assert(style!.BgColor == config.CardBackgroundColor, "Card panel BgColor should match CardBackgroundColor.");
        Assert(style!.BorderColor == config.CardBorderColor, "Card panel BorderColor should match CardBorderColor.");
    }

    public static void TestYakumonoVisualConfigDefaults()
    {
        var config = new VisualConfig();
        Assert(config.FaceTextures != null, "FaceTextures default should not be null.");
        Assert(config.JackpotFaceTexture == null, "JackpotFaceTexture default should be null.");
        Assert(config.FrameTexture == null, "FrameTexture default should be null.");
        Assert(config.ForegroundTexture == null, "ForegroundTexture default should be null.");
        Assert(config.YakumonoScale == 1.0f, "YakumonoScale default should be 1.0f.");
    }

    public static void TestYakumonoVisualConfigPropertyChangesEmitChanged()
    {
        var config = new VisualConfig();
        bool changedFired = false;
        config.Changed += () => { changedFired = true; };

        config.YakumonoBaseColor = Colors.Magenta;
        Assert(changedFired, "Setting YakumonoBaseColor should emit Changed.");

        changedFired = false;
        config.FaceTextures = new Godot.Collections.Array<Texture2D> { new ImageTexture() };
        Assert(changedFired, "Setting FaceTextures should emit Changed.");

        changedFired = false;
        config.JackpotFaceTexture = new ImageTexture();
        Assert(changedFired, "Setting JackpotFaceTexture should emit Changed.");

        changedFired = false;
        config.FrameTexture = new ImageTexture();
        Assert(changedFired, "Setting FrameTexture should emit Changed.");

        changedFired = false;
        config.ForegroundTexture = new ImageTexture();
        Assert(changedFired, "Setting ForegroundTexture should emit Changed.");

        changedFired = false;
        config.YakumonoScale = 1.5f;
        Assert(changedFired, "Setting YakumonoScale should emit Changed.");
    }

    private static (Yakumono yakumono, Node2D frameProcedural, Node2D faceProcedural, Node2D fgProcedural, Sprite2D frameSprite, Sprite2D faceSprite, Sprite2D fgSprite) CreateTestYakumonoWithNodes()
    {
        var yakumono = new Yakumono();
        var frameProcedural = new Node2D();
        var faceProcedural = new Node2D();
        var fgProcedural = new Node2D();
        var frameSprite = new Sprite2D();
        var faceSprite = new Sprite2D();
        var fgSprite = new Sprite2D();

        yakumono.FrameProcedural = frameProcedural;
        yakumono.FaceProcedural = faceProcedural;
        yakumono.ForegroundProcedural = fgProcedural;
        yakumono.FrameSprite = frameSprite;
        yakumono.FaceSprite = faceSprite;
        yakumono.ForegroundSprite = fgSprite;

        return (yakumono, frameProcedural, faceProcedural, fgProcedural, frameSprite, faceSprite, fgSprite);
    }

    public static void TestYakumonoDualRenderingFallback()
    {
        var (yakumono, frameProcedural, faceProcedural, fgProcedural, frameSprite, faceSprite, fgSprite) = CreateTestYakumonoWithNodes();

        var config = new VisualConfig
        {
            FrameTexture = null,
            JackpotFaceTexture = null,
            ForegroundTexture = null,
            YakumonoBaseColor = Colors.Purple
        };

        yakumono.ApplyVisualConfig(config);

        Assert(frameProcedural.Visible && faceProcedural.Visible && fgProcedural.Visible, "Procedural nodes should be visible when textures are null.");
        Assert(!frameSprite.Visible && !faceSprite.Visible && !fgSprite.Visible, "Sprite nodes should be hidden when textures are null.");
        Assert(frameProcedural.Modulate == Colors.Purple, "FrameProcedural modulate should match YakumonoBaseColor.");
        Assert(faceProcedural.Modulate == Colors.Purple, "FaceProcedural modulate should match YakumonoBaseColor.");
        Assert(fgProcedural.Modulate == Colors.Purple, "ForegroundProcedural modulate should match YakumonoBaseColor.");
    }

    public static void TestYakumonoTexturePriority()
    {
        var (yakumono, frameProcedural, faceProcedural, fgProcedural, frameSprite, faceSprite, fgSprite) = CreateTestYakumonoWithNodes();

        var frameTex = new ImageTexture();
        var faceTex1 = new ImageTexture();
        var faceTex2 = new ImageTexture();
        var fgTex = new ImageTexture();

        var config = new VisualConfig
        {
            FrameTexture = frameTex,
            FaceTextures = new Godot.Collections.Array<Texture2D> { faceTex1, faceTex2 },
            ForegroundTexture = fgTex,
            YakumonoScale = 0.5f
        };

        yakumono.ConfigOverride = config;
        yakumono.ApplyVisualConfig(config);

        Assert(!frameProcedural.Visible && !faceProcedural.Visible && !fgProcedural.Visible, "Procedural nodes should be hidden when textures are set.");
        Assert(frameSprite.Visible && faceSprite.Visible && fgSprite.Visible, "Sprites should be visible when textures are set.");
        Assert(frameSprite.Texture == frameTex, "FrameSprite texture should match FrameTexture.");
        Assert(faceSprite.Texture == faceTex1, "FaceSprite texture should match FaceTextures[0] for face index 0.");
        Assert(fgSprite.Texture == fgTex, "ForegroundSprite texture should match ForegroundTexture.");
        Assert(frameSprite.Scale == new Vector2(0.5f, 0.5f), "FrameSprite scale should match YakumonoScale.");
        Assert(faceSprite.Scale == new Vector2(0.5f, 0.5f), "FaceSprite scale should match YakumonoScale.");
        Assert(fgSprite.Scale == new Vector2(0.5f, 0.5f), "ForegroundSprite scale should match YakumonoScale.");
    }

    public static void TestYakumonoFaceStateTransitions()
    {
        var yakumono = new Yakumono();
        var faceTex1 = new ImageTexture();
        var faceTex2 = new ImageTexture();
        var faceTex3 = new ImageTexture();
        var jackpotTex = new ImageTexture();

        var config = new VisualConfig
        {
            FaceTextures = new Godot.Collections.Array<Texture2D> { faceTex1, faceTex2, faceTex3 },
            JackpotFaceTexture = jackpotTex
        };

        yakumono.ConfigOverride = config;

        int stateChangeCount = 0;
        int paidOutCount = 0;

        GlobalEvents.YakumonoStateChangedEventHandler stateHandler = (node, state) =>
        {
            if (node == yakumono)
            {
                stateChangeCount++;
            }
        };
        GlobalEvents.YakumonoPaidOutEventHandler paidOutHandler = (node) =>
        {
            if (node == yakumono)
            {
                paidOutCount++;
            }
        };

        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.YakumonoStateChanged += stateHandler;
            GlobalEvents.Instance.YakumonoPaidOut += paidOutHandler;
        }

        try
        {
            yakumono.TransitionToFaceState(1);
            Assert(yakumono.CurrentFaceIndex == 1, "CurrentFaceIndex should be 1.");
            Assert(!yakumono.IsJackpotState, "IsJackpotState should be false for state 1.");

            yakumono.TransitionToRandomFaceState();
            Assert(yakumono.CurrentFaceIndex >= 0 && yakumono.CurrentFaceIndex < 3, "Random face state should be within bounds.");

            yakumono.TransitionToJackpotState();
            Assert(yakumono.CurrentFaceIndex == Yakumono.JackpotFaceIndex, "CurrentFaceIndex should match JackpotFaceIndex.");
            Assert(yakumono.IsJackpotState, "IsJackpotState should be true in Jackpot state.");
        }
        finally
        {
            if (GlobalEvents.Instance != null)
            {
                GlobalEvents.Instance.YakumonoStateChanged -= stateHandler;
                GlobalEvents.Instance.YakumonoPaidOut -= paidOutHandler;
            }
        }
    }

    public static void TestNullConfigGracefulHandling()
    {
        var boundary = new BoundaryRect();
        boundary.ApplyVisualConfig(null);

        var pin = new Pin();
        pin.ApplyVisualConfig(null);

        var pocket = new Pocket();
        pocket.ApplyVisualConfig(null);

        var indicator = new PocketBallsIndicator();
        indicator.ApplyVisualConfig(null);

        var cardUI = new CardUI();
        cardUI.ApplyVisualConfig(null);

        var env = new EnvironmentBackground();
        env.ApplyVisualConfig(null);

        var yakumono = new Yakumono();
        yakumono.ApplyVisualConfig(null);
    }
}
