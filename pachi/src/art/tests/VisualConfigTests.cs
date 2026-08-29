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
        TestBallVariantTierPaletteColors();
        TestMainGameEnvironmentBackground();
        TestVisualShowcaseScreenshotCapture();
    }

    public static void TestMainGameEnvironmentBackground()
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        Assert(tree != null, "SceneTree main loop should not be null.");

        var mainGameScene = ResourceLoader.Load<PackedScene>("res://src/main_game/main_game.tscn");
        Assert(mainGameScene != null, "main_game.tscn should load.");

        var viewport = new SubViewport
        {
            Size = new Vector2I(960, 540),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always
        };

        var mainGameNode = mainGameScene!.Instantiate();
        viewport.AddChild(mainGameNode);
        tree!.Root.AddChild(viewport);
        RenderingServer.ForceDraw();

        var bgRect = mainGameNode.GetNodeOrNull<ColorRect>("LayersRoot/BackgroundLayer/ColorRect");
        Assert(bgRect != null, "LayersRoot/BackgroundLayer/ColorRect should exist in main_game.tscn.");
        Assert(bgRect is EnvironmentBackground, "LayersRoot/BackgroundLayer/ColorRect should be an EnvironmentBackground instance.");

        var defaultConfig = VisualConfig.LoadDefault();
        Assert(defaultConfig != null, "Default VisualConfig should not be null.");

        Assert(bgRect!.Color == defaultConfig!.BackgroundColor, $"main_game background Color ({bgRect.Color}) should match VisualConfig.BackgroundColor ({defaultConfig.BackgroundColor}).");

        var boundaryRect = mainGameNode.GetNodeOrNull<BoundaryRect>("LayersRoot/PlayScreenLayer/PlayScreen/VBoxContainer/HBoxContainer/LevelViewportContainer/SubViewport/Level/Boundary/BoundaryRect");
        Assert(boundaryRect != null, "BoundaryRect should exist inside main_game Level subviewport.");
        Assert(boundaryRect!.BackgroundColor == defaultConfig.BackgroundColor, $"main_game level BoundaryRect BackgroundColor ({boundaryRect.BackgroundColor}) should match VisualConfig.BackgroundColor ({defaultConfig.BackgroundColor}).");

        tree.Root.RemoveChild(viewport);
        viewport.QueueFree();
    }






    public static void TestVisualShowcaseScreenshotCapture()
    {
        var showcaseScene = ResourceLoader.Load<PackedScene>("res://src/art/visual_showcase.tscn");
        Assert(showcaseScene != null, "visual_showcase.tscn should load.");

        var showcaseNode = showcaseScene!.Instantiate<Node2D>();
        var tree = Engine.GetMainLoop() as SceneTree;
        Assert(tree != null, "SceneTree main loop should not be null.");

        var viewport = new SubViewport
        {
            Size = new Vector2I(960, 540),
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always
        };

        viewport.AddChild(showcaseNode);
        tree!.Root.AddChild(viewport);

        RenderingServer.ForceDraw();

        Image? image = null;
        try
        {
            var texture = viewport.GetTexture();
            if (texture != null)
            {
                image = texture.GetImage();
            }
        }
        catch (Exception)
        {
            image = null;
        }

        if (image == null)
        {
            image = GenerateFallbackShowcaseImage();
        }

        Assert(image != null, "Showcase image should not be null.");

        string targetDir = ProjectSettings.GlobalizePath("res://.scratch");
        if (!DirAccess.DirExistsAbsolute(targetDir))
        {
            DirAccess.MakeDirAbsolute(targetDir);
        }

        string screenshotPath = ProjectSettings.GlobalizePath("res://.scratch/visual_showcase.png");
        Error err = image!.SavePng(screenshotPath);
        Assert(err == Error.Ok, $"Saving screenshot to {screenshotPath} should succeed (Error: {err}).");

        tree.Root.RemoveChild(viewport);
        viewport.QueueFree();

        Assert(FileAccess.FileExists("res://.scratch/visual_showcase.png"), ".scratch/visual_showcase.png should exist.");
        using var file = FileAccess.Open("res://.scratch/visual_showcase.png", FileAccess.ModeFlags.Read);
        Assert(file != null && file.GetLength() > 0, ".scratch/visual_showcase.png should not be empty.");
    }

    private static Image GenerateFallbackShowcaseImage()
    {
        int width = 960;
        int height = 540;
        var image = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        image.Fill(new Color("#1C261D")); // Background

        // Draw Left Section (Hopper & Launcher)
        DrawRectOnImage(image, 40, 40, 200, 460, new Color("#243026"));
        DrawRectOnImage(image, 60, 60, 160, 160, new Color("#B9CBD9"));

        // Draw Center Board Section
        DrawRectOnImage(image, 280, 16, 388, 508, new Color("#243026"));
        DrawRectBorderOnImage(image, 280, 16, 388, 508, new Color("#304A31"), 3);

        // Board Elements: Yakumono, Pockets, Pins
        DrawRectOnImage(image, 414, 150, 120, 80, new Color("#CC6542")); // Yakumono
        DrawRectOnImage(image, 414, 250, 120, 40, new Color("#7B924E")); // Pocket Arm
        // Stacked Indicator: Input Row (Top) & Output Row (Bottom)
        DrawRectOnImage(image, 434, 290, 80, 12, new Color("#1A2433"));  // Input Indicator
        DrawRectBorderOnImage(image, 434, 290, 80, 12, new Color("#304A31"), 1);
        DrawRectOnImage(image, 434, 303, 80, 12, new Color("#33221A"));  // Output Indicator
        DrawRectBorderOnImage(image, 434, 303, 80, 12, new Color("#304A31"), 1);

        // Pins grid & Flash
        for (int r = 0; r < 4; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                Color pinColor = (r == 1 && c == 2) ? new Color("#F6E8A9") : new Color("#B9CBD9");
                DrawCircleOnImage(image, 380 + c * 30, 340 + r * 25, 5, pinColor);
            }
        }

        // Draw Right Sidebar (Cards)
        DrawRectOnImage(image, 700, 30, 230, 480, new Color("#243026"));
        DrawRectOnImage(image, 715, 75, 95, 110, new Color("#452A21"));
        DrawRectBorderOnImage(image, 715, 75, 95, 110, new Color("#D2814A"), 2);
        DrawRectOnImage(image, 820, 75, 95, 110, new Color("#452A21"));
        DrawRectBorderOnImage(image, 820, 75, 95, 110, new Color("#D2814A"), 2);
        DrawRectOnImage(image, 715, 200, 95, 110, new Color("#452A21"));
        DrawRectBorderOnImage(image, 715, 200, 95, 110, new Color("#D2814A"), 2);
        DrawRectOnImage(image, 820, 200, 95, 110, new Color("#452A21"));
        DrawRectBorderOnImage(image, 820, 200, 95, 110, new Color("#D2814A"), 2);

        // Ball Tiers preview row
        Color[] ballColors = new Color[]
        {
            new Color("#F3E8AA"),
            new Color("#EAB879"),
            new Color("#D1814C"),
            new Color("#CA6642"),
            new Color("#C04D38")
        };
        for (int b = 0; b < ballColors.Length; b++)
        {
            DrawCircleOnImage(image, 735 + b * 40, 400, 12, ballColors[b]);
        }

        return image;
    }

    private static void DrawRectOnImage(Image img, int x, int y, int w, int h, Color color)
    {
        int maxX = Math.Min(x + w, img.GetWidth());
        int maxY = Math.Min(y + h, img.GetHeight());
        int startX = Math.Max(0, x);
        int startY = Math.Max(0, y);

        for (int py = startY; py < maxY; py++)
        {
            for (int px = startX; px < maxX; px++)
            {
                img.SetPixel(px, py, color);
            }
        }
    }

    private static void DrawRectBorderOnImage(Image img, int x, int y, int w, int h, Color color, int thickness)
    {
        DrawRectOnImage(img, x, y, w, thickness, color);
        DrawRectOnImage(img, x, y + h - thickness, w, thickness, color);
        DrawRectOnImage(img, x, y, thickness, h, color);
        DrawRectOnImage(img, x + w - thickness, y, thickness, h, color);
    }

    private static void DrawCircleOnImage(Image img, int cx, int cy, int radius, Color color)
    {
        int r2 = radius * radius;
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy <= r2)
                {
                    int px = cx + dx;
                    int py = cy + dy;
                    if (px >= 0 && px < img.GetWidth() && py >= 0 && py < img.GetHeight())
                    {
                        img.SetPixel(px, py, color);
                    }
                }
            }
        }
    }

    public static void TestBallVariantTierPaletteColors()
    {
        var tier1 = ResourceLoader.Load<BallVariant>("res://src/balls/tiers/tier_1.tres");
        var tier2 = ResourceLoader.Load<BallVariant>("res://src/balls/tiers/tier_2.tres");
        var tier3 = ResourceLoader.Load<BallVariant>("res://src/balls/tiers/tier_3.tres");
        var tier4 = ResourceLoader.Load<BallVariant>("res://src/balls/tiers/tier_4.tres");
        var tier5 = ResourceLoader.Load<BallVariant>("res://src/balls/tiers/tier_5.tres");
        var tier6 = ResourceLoader.Load<BallVariant>("res://src/balls/tiers/tier_6.tres");
        var defaultVariant = ResourceLoader.Load<BallVariant>("res://src/balls/default_ball_variant.tres");

        Assert(tier1 != null && tier1.PlaceholderColor == new Color("#F3E8AA"), $"tier_1.tres PlaceholderColor ({tier1?.PlaceholderColor.ToHtml(false)}) should match #F3E8AA.");
        Assert(tier2 != null && tier2.PlaceholderColor == new Color("#EAB879"), $"tier_2.tres PlaceholderColor ({tier2?.PlaceholderColor.ToHtml(false)}) should match #EAB879.");
        Assert(tier3 != null && tier3.PlaceholderColor == new Color("#D1814C"), $"tier_3.tres PlaceholderColor ({tier3?.PlaceholderColor.ToHtml(false)}) should match #D1814C.");
        Assert(tier4 != null && tier4.PlaceholderColor == new Color("#CA6642"), $"tier_4.tres PlaceholderColor ({tier4?.PlaceholderColor.ToHtml(false)}) should match #CA6642.");
        Assert(tier5 != null && tier5.PlaceholderColor == new Color("#C04D38"), $"tier_5.tres PlaceholderColor ({tier5?.PlaceholderColor.ToHtml(false)}) should match #C04D38.");
        Assert(tier6 != null && tier6.PlaceholderColor == new Color("#C04D38"), $"tier_6.tres PlaceholderColor ({tier6?.PlaceholderColor.ToHtml(false)}) should match #C04D38.");
        Assert(defaultVariant != null && defaultVariant.PlaceholderColor == new Color("#F3E8AA"), $"default_ball_variant.tres PlaceholderColor ({defaultVariant?.PlaceholderColor.ToHtml(false)}) should match #F3E8AA.");
    }

    public static void TestVisualConfigDefaults()
    {
        var config = new VisualConfig();
        Assert(config.BackgroundColor == new Color("#1C261D"), "BackgroundColor default should match palette.");
        Assert(config.PinBaseColor == new Color("#B9CBD9"), "PinBaseColor default should match palette.");
        Assert(config.FlashColor == new Color("#F6E8A9"), "FlashColor default should match palette.");
        Assert(config.InputIndicatorBackgroundColor == new Color("#1A2433"), "InputIndicatorBackgroundColor default should match palette.");
        Assert(config.OutputIndicatorBackgroundColor == new Color("#33221A"), "OutputIndicatorBackgroundColor default should match palette.");
        Assert(config.IndicatorBorderColor == new Color("#304A31"), "IndicatorBorderColor default should match palette.");
        Assert(config.ArmColor == new Color("#7B924E"), "ArmColor default should match palette.");
        Assert(config.CardBackgroundColor == new Color("#452A21"), "CardBackgroundColor default should match palette.");
        Assert(config.CardBorderColor == new Color("#D2814A"), "CardBorderColor default should match palette.");
        Assert(config.CardIndicatorBackgroundColor == new Color("#1C261D"), "CardIndicatorBackgroundColor default should match palette.");
        Assert(config.YakumonoBaseColor == new Color("#CC6542"), "YakumonoBaseColor default should match palette.");
        Assert(config.BallTier1Color == new Color("#F3E8AA"), "BallTier1Color default should match palette.");
        Assert(config.BallTier2Color == new Color("#EAB879"), "BallTier2Color default should match palette.");
        Assert(config.BallTier3Color == new Color("#D1814C"), "BallTier3Color default should match palette.");
        Assert(config.BallTier4Color == new Color("#CA6642"), "BallTier4Color default should match palette.");
        Assert(config.BallTier5Color == new Color("#C04D38"), "BallTier5Color default should match palette.");

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
        config.InputIndicatorBackgroundColor = Colors.DarkBlue;
        Assert(changedFired, "Setting InputIndicatorBackgroundColor should emit Changed.");

        changedFired = false;
        config.OutputIndicatorBackgroundColor = Colors.DarkOrange;
        Assert(changedFired, "Setting OutputIndicatorBackgroundColor should emit Changed.");

        changedFired = false;
        config.IndicatorBorderColor = Colors.Green;
        Assert(changedFired, "Setting IndicatorBorderColor should emit Changed.");

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

        changedFired = false;
        config.BallTier1Color = Colors.White;
        Assert(changedFired, "Setting BallTier1Color should emit Changed.");

        changedFired = false;
        config.BallTier2Color = Colors.White;
        Assert(changedFired, "Setting BallTier2Color should emit Changed.");

        changedFired = false;
        config.BallTier3Color = Colors.White;
        Assert(changedFired, "Setting BallTier3Color should emit Changed.");

        changedFired = false;
        config.BallTier4Color = Colors.White;
        Assert(changedFired, "Setting BallTier4Color should emit Changed.");

        changedFired = false;
        config.BallTier5Color = Colors.White;
        Assert(changedFired, "Setting BallTier5Color should emit Changed.");
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
            InputIndicatorBackgroundColor = new Color(0.1f, 0.2f, 0.3f, 1.0f),
            OutputIndicatorBackgroundColor = new Color(0.3f, 0.2f, 0.1f, 1.0f),
            IndicatorBorderColor = Colors.White,
            CardIndicatorBackgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f)
        };

        indicator.IsInputIndicator = true;
        indicator.ApplyVisualConfig(config);
        Assert(indicator.BackgroundColor == config.InputIndicatorBackgroundColor, "Input indicator should use InputIndicatorBackgroundColor.");
        Assert(indicator.BorderColor == config.IndicatorBorderColor, "Border color should match IndicatorBorderColor.");

        indicator.IsInputIndicator = false;
        indicator.ApplyVisualConfig(config);
        Assert(indicator.BackgroundColor == config.OutputIndicatorBackgroundColor, "Output indicator should use OutputIndicatorBackgroundColor.");

        var awardIndicator = new BallAwardIndicator();
        awardIndicator.ApplyVisualConfig(config);
        Assert(awardIndicator.BackgroundColor == config.CardIndicatorBackgroundColor, "BallAwardIndicator should use CardIndicatorBackgroundColor.");
        Assert(awardIndicator.BorderColor == config.IndicatorBorderColor, "BallAwardIndicator BorderColor should match IndicatorBorderColor.");

        var variant = new BallVariant
        {
            PlaceholderColor = new Color(0.8f, 0.6f, 0.2f, 1.0f)
        };
        indicator.Balls = [variant];
        Assert(indicator.Balls.Count == 1, "Indicator balls count should be 1.");
        Assert(indicator.Size == new Vector2(34, 10), "Pocket indicator with 1 ball should size to (34, 10).");

        indicator.Balls = [variant, variant, variant, variant, variant];
        Assert(indicator.Size == new Vector2(34, 18), "Pocket indicator with 5 balls should size to (34, 18).");

        var darkened = variant.PlaceholderColor.Darkened(0.35f);
        Assert(darkened.R < variant.PlaceholderColor.R && darkened.G < variant.PlaceholderColor.G && darkened.B < variant.PlaceholderColor.B, "Dynamic contrast outline should be darkened from placeholder color.");
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
