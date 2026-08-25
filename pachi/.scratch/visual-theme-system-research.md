# Centralized Visual Theme & Art Pipeline Research for Pachi
**Engine Target:** Godot 4.7 (.NET 8.0 / C#) | **Focus:** Non-Technical Artist UX, Palette Management, Sprite Integration & Live Preview

---

## 1. Executive Summary & Problem Context

### The Challenge
As Pachi transitions from prototype geometry (procedural circles, capsules, rectangles via `shape_sprites`) to production-ready art, visual assets and color palettes must be configured across multiple interconnected game entities:
- **Balls**: 6 tier variants with distinct colors, textures, and motion trails.
- **Pins**: Pin sprites, hit flash colors, recoil tweens, and spark particle gradients.
- **Pockets**: Arm sprites (e.g. tulip/spinner textures), indicator box styling, and reward colors.
- **Board & Environment**: Background canvas colors, bumper boundary styling, hopper and launcher graphics.
- **User Interface**: Card UI backgrounds (`StyleBoxFlat`), borders, typography, and drag highlights.

The visual artist is **non-technical** (unfamiliar with Godot node trees, scene inheritance, C# scripts, or Git merge conflicts on `.tscn` files). Giving the artist direct write access to scene hierarchies risks accidental breaks to collision shapes, node paths, and physics configurations.

### Key Objectives
1. **Centralized Access**: A single point of control for all colors, sprites, and visual constants.
2. **Non-Technical Usability**: Foolproof, intuitive interface (no coding, no scene tree surgery).
3. **Immediate Visual Feedback**: Instant preview of color changes and sprite swaps without full build/run cycles.
4. **Safety & Robustness**: Decouple visual skinning from physics colliders, gameplay logic, and signals.

---

## 2. Analysis of Pachi's Current Architecture & Visual Touchpoints

An inspection of the Pachi codebase reveals how visual properties are currently distributed:

```
src/
├── balls/
│   ├── BallVariant.cs           -> Resource holding `PlaceholderColor`, `BasePrice` (TODO: sprite)
│   ├── Ball.cs                  -> Sets `PlaceholderSprite.Color`, manages `MotionTrail2D` & fade tweens
│   └── tiers/tier_1..6.tres     -> Individual Resource files per tier
├── pins/
│   ├── Pin.cs                   -> `Sprite` (CircleSprite), `SparkParticles`, `FlashColor`, `PulseScale`
│   └── pin.tscn                 -> ColliderCircleSprite with radius 2.0, CPUParticles2D color ramp
├── pockets/
│   ├── Pocket.cs                -> `LeftArm`/`RightArm` nodes with hardcoded Sprite2D child nodes
│   ├── PocketBallsIndicator.cs  -> [Tool] custom _Draw() drawing `BackgroundColor`, `BorderColor`, dots
│   └── pocket.tscn              -> Sprite2D referencing "res://assets/sprites/ellis/Tulip sprite v 1.png"
├── shape_sprites/               -> [Tool] procedural rendering (CircleSprite, RectSprite, CapsuleSprite)
├── cards/
│   ├── CardUI.cs                -> Programmatic StyleBoxFlat creation with `CardData.CardColor`
│   └── card_ui.tscn
├── main_game/
│   ├── GameConfig.cs            -> Singleton holding `BallTiers`, `CardArchetypes`, `BallScene`
│   └── main_game.tscn           -> BackgroundLayer/ColorRect (Color: #131313)
└── launcher/
    └── Launcher.cs              -> LauncherSprite and LauncherGhostSprite rotations and rendering
```

### Key Architectural Observations:
1. **Procedural Shape Sprites**: Nodes currently rely on `[Tool]` scripts inheriting from `CanvasItem` and calling `DrawCircle()`, `DrawRect()`, or `QueueRedraw()`. Replacing these with textures requires either dynamic switching between `Sprite2D` and procedural shapes, or a unified `ThemeableSprite` component.
2. **Scattered Color Definitions**: Ball colors live in `tier_*.tres`, pin flash colors live in `pin.tscn`, pocket indicator colors live in `pocket.tscn`, and board background lives in `main_game.tscn`. There is no central palette definition.
3. **Existing `[Tool]` Culture**: Pachi already uses `[Tool]` scripts extensively (e.g. `Pocket.cs`, `PocketBallsIndicator.cs`, `BoundaryRect.cs`, `ShapeSprites`). This makes real-time editor preview a natural fit for the codebase.

---

## 3. Comparative Evaluation of Candidate Architectures

---

### Approach A: Centralized Custom Resources (`VisualTheme`) + Godot Inspector & `[Tool]` Preview

#### How It Works
Create a hierarchical set of Godot custom resources (`[GlobalClass] public partial class VisualTheme : Resource`) structured with clean `[ExportGroup]` and `[ExportSubgroup]` annotations. Nodes across the game (`Ball`, `Pin`, `Pocket`, `CardUI`, `Level`) subscribe to the active theme resource (or query it via `GameConfig.Instance.VisualTheme`). 

Because scripts are marked with `[Tool]`, when an artist edits color swatches or assigns `Texture2D` files in the Godot Inspector, the resource emits the built-in `Resource.Changed` signal, triggering immediate visual updates in the 2D viewport.

```
[ VisualTheme.tres ] (Central Resource File)
   ├── Palette (Global Background, Pin Base, Flash Color, Indicator BG, Border)
   ├── Ball Tiers (Tier 1..6: Colors, Textures, Trail Gradients)
   ├── Pin Visuals (Default Texture, Scale, Pulse Duration, Spark Gradient)
   ├── Pocket Visuals (Arm Textures, Hole Modulation, Indicator Margins)
   └── UI Theme (Panel StyleBoxes, Card Backgrounds, Typography)
         │ (Resource.Changed Signal)
         ▼
[ Open Godot Viewports / Scenes ] (Instant Real-Time Redraw)
```

#### Artist Workflow
1. The artist opens Godot and clicks on `res://themes/active_theme.tres` in the FileSystem dock.
2. The Godot Inspector displays an organized, categorized panel with color pickers and drag-and-drop texture slots.
3. The artist drags a PNG from their assets folder into a texture slot or picks a new hex color with the color picker.
4. Any open level scene (`level.tscn` or `main_game.tscn`) updates instantly in the editor viewport without running the game.

#### Technical Implementation Sketch (Godot 4 C#)

```csharp
// src/theme/VisualTheme.cs
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class VisualTheme : Resource
{
    [ExportGroup("Palette - Global Colors")]
    [Export] public Color BoardBackgroundColor { get; set; } = new Color("131313");
    [Export] public Color PinDefaultColor { get; set; } = Colors.White;
    [Export] public Color PinFlashColor { get; set; } = new Color(1.0f, 0.85f, 0.2f, 1.0f);
    [Export] public Color PocketIndicatorBg { get; set; } = new Color("161616");
    [Export] public Color PocketIndicatorBorder { get; set; } = Colors.Black;

    [ExportGroup("Ball Tiers")]
    [Export] public Array<BallVisualData> BallTiers { get; set; } = new();

    [ExportGroup("Pin Art Assets")]
    [Export] public Texture2D PinTexture { get; set; }
    [Export] public Vector2 PinSpriteScale { get; set; } = Vector2.One;
    [Export] public Gradient PinSparkGradient { get; set; }

    [ExportGroup("Pocket Art Assets")]
    [Export] public Texture2D PocketLeftArmTexture { get; set; }
    [Export] public Texture2D PocketRightArmTexture { get; set; }
    [Export] public Vector2 PocketArmScale { get; set; } = Vector2.One;

    [ExportGroup("UI & Cards")]
    [Export] public StyleBox CardBackgroundStyle { get; set; }
    [Export] public Font CardTitleFont { get; set; }
}

[GlobalClass]
public partial class BallVisualData : Resource
{
    [Export] public Color BallColor { get; set; } = Colors.White;
    [Export] public Texture2D BallTexture { get; set; }
    [Export] public Gradient TrailGradient { get; set; }
}
```

```csharp
// src/pins/Pin.cs (Integration)
[Tool]
[GlobalClass]
public partial class Pin : StaticBody2D
{
    [Export]
    public VisualTheme Theme
    {
        get => _theme;
        set
        {
            if (_theme == value) return;
            if (_theme != null) _theme.Changed -= OnThemeChanged;
            _theme = value;
            if (_theme != null) _theme.Changed += OnThemeChanged;
            ApplyTheme();
        }
    }
    private VisualTheme _theme;

    public override void _Ready()
    {
        base._Ready();
        ApplyTheme();
    }

    private void OnThemeChanged()
    {
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (_theme == null) return;
        FlashColor = _theme.PinFlashColor;
        if (Sprite is Sprite2D sprite2D && _theme.PinTexture != null)
        {
            sprite2D.Texture = _theme.PinTexture;
            sprite2D.Scale = _theme.PinSpriteScale;
        }
        else if (Sprite is CircleSprite circleSprite)
        {
            circleSprite.Color = _theme.PinDefaultColor;
        }
    }
}
```

#### Pros & Cons
- **Pros:**
  - 100% native Godot architecture with zero maintenance of custom editor GUI code.
  - Built-in Undo/Redo support provided by the Godot Inspector.
  - Clean version control: `.tres` files are text-based and merge easily in Git.
  - Supports multiple theme presets (`theme_retro.tres`, `theme_neon.tres`, `theme_pastel.tres`).
- **Cons:**
  - Standard Inspector shows properties vertically in standard Godot styles; lacks custom visual layouts or embedded mini-simulations.
  - Requires the artist to be comfortable navigating the Godot editor interface and FileSystem dock.

---

### Approach B: Custom Godot Editor Plugin / Main Screen Dock / Inspector Plugin

#### How It Works
Build a dedicated Godot Editor Plugin (`EditorPlugin`) in C# that registers either:
1. **A Main Screen Plugin**: A custom top-level tab alongside `2D`, `3D`, `Script`, and `AssetLib` (`_HasMainScreen() => true`).
2. **A Dock / Bottom Panel**: A dedicated side-dock (`AddControlToDock`) or expandable bottom panel (`AddControlToBottomPanel`) titled "Pachi Theme Studio".
3. **An `EditorInspectorPlugin`**: Custom property drawers for `VisualTheme` rendering color palette swatches, asset preview thumbnails, and quick batch-apply buttons.

Inside the plugin's `Control` UI, we embed a `SubViewport` rendering isolated game pieces (a bouncing ball, an animated pocket arm, a flashing pin). The artist interacts with large color swatches, drag-and-drop sprite slots, and theme preset selectors.

```
┌────────────────────────────────────────────────────────────────────────┐
│  Godot Top Bar: [ 2D ] [ 3D ] [ Script ] [ AssetLib ] [ Pachi Studio ] │
├────────────────────────────────────────────────────────────────────────┤
│ ┌───────────────────────────┐ ┌──────────────────────────────────────┐ │
│ │ PALETTE & ASSET CONFIG    │ │ LIVE ISOLATED PREVIEW                │ │
│ │                           │ │                                      │ │
│ │ • Board Color: [ #131313] │ │   [SubViewport: Auto-Bouncing Ball]  │ │
│ │ • Pin Color:   [ #E0E0E0] │ │                  ●                   │ │
│ │ • Pin Sprite:  [pin.png ] │ │                 / \                  │ │
│ │ • Pocket Arm:  [tulip.png]│ │             [ Pocket ]               │ │
│ │                           │ │                                      │ │
│ │ Tier 1 Ball: [● Color] [Texture Slot]                              │ │
│ │ Tier 2 Ball: [● Color] [Texture Slot]                              │ │
│ │                           │ │ [ Trigger Test Hit ] [ Toggle Arms ] │ │
│ │ [ Save Theme ] [ Export ] │ └──────────────────────────────────────┘ │
│ └───────────────────────────┘                                          │
└────────────────────────────────────────────────────────────────────────┘
```

#### Artist Workflow
1. The artist opens Godot and clicks the **"Pachi Studio"** tab at the top.
2. They see a clean, dedicated control panel tailored specifically to Pachi art assets without any extraneous physics or collision properties visible.
3. They adjust colors with custom swatch palettes and assign textures into visual drop boxes.
4. An embedded preview panel immediately displays how the ball looks against the board background, how the pin flashes on bounce, and how the pocket arms rotate.
5. Edits are saved directly to the project's active `VisualTheme.tres`.

#### Technical Implementation Sketch (Godot 4 C#)

```csharp
// src/addons/pachi_theme_studio/PachiThemeStudioPlugin.cs
#if TOOLS
using Godot;

[Tool]
public partial class PachiThemeStudioPlugin : EditorPlugin
{
    private Control _studioScreen;

    public override void _EnterTree()
    {
        var studioScene = GD.Load<PackedScene>("res://addons/pachi_theme_studio/theme_studio_view.tscn");
        if (studioScene != null)
        {
            _studioScreen = studioScene.Instantiate<Control>();
            EditorInterface.Singleton.GetEditorMainScreen().AddChild(_studioScreen);
            _studioScreen.Hide();
        }
    }

    public override void _ExitTree()
    {
        if (_studioScreen != null)
        {
            _studioScreen.QueueFree();
        }
    }

    public override bool _HasMainScreen() => true;
    public override string _GetPluginName() => "Pachi Studio";
    public override Texture2D _GetPluginIcon() => EditorInterface.Singleton.GetBaseControl().GetThemeIcon("ColorPick", "EditorIcons");

    public override void _MakeVisible(bool visible)
    {
        if (_studioScreen != null)
        {
            _studioScreen.Visible = visible;
        }
    }
}
#endif
```

#### Pros & Cons
- **Pros:**
  - Best-in-class UX within the Godot editor: hides all technical clutter and presents a focused tool.
  - Can embed isolated interactive previews with custom animations and particle triggers.
  - Can include project validation (e.g. warning if sprite resolution is too high or format is uncompressed).
- **Cons:**
  - High initial development overhead (requires building a complete custom UI and wiring `EditorUndoRedoManager`).
  - C# editor plugins in Godot 4 require project recompilation (`dotnet build`) when changed and must be wrapped in `#if TOOLS`.
  - Ongoing maintenance burden when updating Godot versions.

---

### Approach C: In-Game / Runtime "Design Sandbox" Scene

#### How It Works
Create a dedicated playable Godot scene (e.g. `res://src/tools/design_sandbox.tscn`). This scene instantiates the actual game components (Hopper, Launcher with Auto-Fire enabled, Pins, Pockets, Drains, and Balls) side-by-side with an in-game UI overlay built with standard Godot `Control` nodes (`ColorPickerButton`, `OptionButton`, file selectors, sliders).

The artist runs this scene directly (or it can be launched via F6 / standalone desktop executable). As the artist tweaks colors and selects textures, the running game updates live in real-time. When satisfied, the artist clicks a **"Save Theme to Disk"** button, which calls `ResourceSaver.Save(activeTheme, "res://themes/active_theme.tres")`.

```
┌────────────────────────────────────────────────────────────────────────┐
│ PACHI DESIGN SANDBOX (Running Game Simulation)                         │
├──────────────────────────────────┬─────────────────────────────────────┤
│ LIVE PLAYFIELD (Full Physics)    │ LIVE DESIGNER PANEL                 │
│                                  │                                     │
│  [Hopper]                        │ Active Theme: [ Default Theme ▼ ]   │
│     │                            │                                     │
│     ▼ (Auto-launching balls)     │ ── Background & Board ──            │
│   ●     ●                        │ Board Color:   [ 🎨 ColorPicker ]   │
│     \  /                         │ Boundary Tint: [ 🎨 ColorPicker ]   │
│   [Pin Grid]                     │                                     │
│    *  *  *  (Sparks & Pulses!)   │ ── Ball Tiers ──                    │
│   *  *  *  *                     │ Tier 1: [ 🎨 ] Texture: [ Select ]  │
│                                  │ Tier 2: [ 🎨 ] Texture: [ Select ]  │
│        [Pocket]                  │                                     │
│         \    /                   │ ── Pin Particles & Glow ──          │
│          \  /                    │ Flash Color:   [ 🎨 ColorPicker ]   │
│                                  │ Particle Rate: [───●─────] (4 sparks)│
│                                  │                                     │
│ [Auto-Fire: ON] [Physics: 1x]    │ [ 💾 SAVE CHANGES TO PROJECT ]      │
└──────────────────────────────────┴─────────────────────────────────────┘
```

#### Artist Workflow
1. The artist opens the project and presses **Play** (or runs a standalone desktop debug build on their machine).
2. The game is already running with auto-fire launching balls through the pins and pockets.
3. The artist adjusts colors on the right-hand panel and immediately watches how the balls look in motion, how readable the pins are against the background, and how particles feel on impact.
4. When happy with the look, the artist clicks **"Save to Project"**. The `.tres` file is saved, ready to be committed to Git.

#### Technical Implementation Sketch (Godot 4 C#)

```csharp
// src/tools/DesignSandbox.cs
using Godot;

public partial class DesignSandbox : Control
{
    [Export] public VisualTheme CurrentTheme { get; set; }
    [Export] public Node2D PlayfieldRoot { get; set; }
    [Export] public ColorPickerButton BoardColorPicker { get; set; }
    [Export] public ColorPickerButton PinFlashColorPicker { get; set; }
    [Export] public OptionButton BallTierSelector { get; set; }
    [Export] public ColorPickerButton BallColorPicker { get; set; }
    [Export] public Button SaveButton { get; set; }

    public override void _Ready()
    {
        if (CurrentTheme == null)
        {
            CurrentTheme = GD.Load<VisualTheme>("res://themes/active_theme.tres");
        }

        // Initialize UI values from theme
        BoardColorPicker.Color = CurrentTheme.BoardBackgroundColor;
        PinFlashColorPicker.Color = CurrentTheme.PinFlashColor;

        // Wire event handlers
        BoardColorPicker.ColorChanged += OnBoardColorChanged;
        PinFlashColorPicker.ColorChanged += OnPinFlashColorChanged;
        SaveButton.Pressed += OnSaveThemePressed;
    }

    private void OnBoardColorChanged(Color newColor)
    {
        CurrentTheme.BoardBackgroundColor = newColor;
        CurrentTheme.EmitChanged();
    }

    private void OnPinFlashColorChanged(Color newColor)
    {
        CurrentTheme.PinFlashColor = newColor;
        CurrentTheme.EmitChanged();
    }

    private void OnSaveThemePressed()
    {
        Error err = ResourceSaver.Save(CurrentTheme, "res://themes/active_theme.tres");
        if (err == Error.Ok)
        {
            GD.Print("VisualTheme successfully saved to disk!");
        }
        else
        {
            GD.PrintErr($"Failed to save VisualTheme: {err}");
        }
    }
}
```

#### Pros & Cons
- **Pros:**
  - **Unmatched Artist Experience**: Highest fidelity preview possible—sees physics, motion trails, particles, audio sync, and animations under actual gameplay conditions.
  - Zero learning curve for non-technical artists: completely custom, foolproof UI.
  - Can be exported as a standalone executable for an artist who does not want to use Godot at all.
  - Built using standard runtime C# and Godot `Control` nodes (no complex `EditorPlugin` APIs).
- **Cons:**
  - Saving via `ResourceSaver.Save()` to `res://` only works when running inside the Godot Editor (debug mode). In an exported `.pck` package, `res://` is read-only, so it must save to `user://` or export an external JSON/TRES file.
  - Adds a developer tool scene to maintain in the repository.

---

### Approach D: Global Shader Parameters & Palette Look-Up Table (LUT) Swapping

#### How It Works
Utilize Godot 4's **Global Shader Parameters** (`ProjectSettings` -> `Shader Globals` or `RenderingServer.GlobalShaderParameterSet`) combined with indexed palette shaders. Sprites are authored with grayscale or indexed color values. A single 1D or 2D Palette Look-Up Table (LUT) texture maps these indices to the active RGB palette in the GPU fragment shader.

```glsl
// shader_type canvas_item;
global uniform sampler2D active_palette_texture;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    // Use red channel as normalized index into 1D palette texture
    vec4 mapped_color = texture(active_palette_texture, vec2(tex_color.r, 0.5));
    COLOR = vec4(mapped_color.rgb, tex_color.a * mapped_color.a);
}
```

```
[ Grayscale Sprite ] ──▶ [ Fragment Shader ] ◀── [ Palette LUT Texture (1x16 PNG) ]
                               │
                               ▼
                    [ Fully Tinted Render ]
```

#### Artist Workflow
1. Artist designs sprites in Aseprite / Photoshop using a fixed indexing method (or indexed color mode).
2. Artist exports a 1x16 or 1x32 PNG strip containing the color palette.
3. Swapping themes (e.g. Default -> Cyberpunk -> GameBoy -> Pastel) involves replacing a single palette texture or changing a global parameter in Project Settings.

#### Pros & Cons
- **Pros:**
  - Instantaneous, zero-overhead theme swapping across the entire game in a single GPU call.
  - Ideal for retro/pixel-art aesthetics and dynamic visual effects (flashes, color cycling).
- **Cons:**
  - **Does not solve sprite asset replacement**: Cannot replace geometric shapes with distinct textures, animations, or differing dimensions.
  - Imposes strict asset authoring rules on the artist (grayscale indexing, shader alignment).
  - Unnecessarily rigid for Pachi's hybrid UI and vector/raster components.

---

### Approach E: External Data-Driven Asset Pipeline (JSON / Lospec GPL Palettes & Auto-Importers)

#### How It Works
Allow the artist to work entirely in their native digital art software (Aseprite, Photoshop, Lospec, Figma). The artist exports palettes in open formats (e.g. `.gpl` GIMP Palette, Lospec JSON, or `.hex`) and saves PNG sprites into structured directories (e.g. `assets/sprites/balls/tier_1.png`). 

A Godot C# `EditorFileSystem` watcher or `EditorScenePostImport` script automatically detects changes on disk, parses the palette file, and updates the Godot Resources (`BallVariant`, `VisualTheme`) without requiring manual assignment in the Inspector.

```
[ Aseprite / Lospec ] ──Export──▶ [ assets/palettes/autumn.gpl ]
                                             │
                                             ▼ (C# Auto-Importer)
                                  [ VisualTheme.tres Updated ]
                                             │
                                             ▼
                                  [ In-Game Visuals Updated ]
```

#### Pros & Cons
- **Pros:**
  - Non-technical artist never has to touch Godot at all if they prefer working exclusively in external art software.
  - Seamless integration with external palette libraries (Lospec, Adobe Color).
- **Cons:**
  - Does not provide live in-engine feedback on particle parameters, tweens, or UI layouts.
  - Strict naming conventions required for file detection.

---

## 4. Comprehensive Comparison Matrix

| Criteria | Approach A: Central Custom Resources + `[Tool]` | Approach B: Custom Editor Plugin / Studio Dock | Approach C: In-Game Runtime Design Sandbox | Approach D: Global Shader Parameters & LUT | Approach E: External Asset Pipeline (GPL/JSON) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Non-Technical Artist Friendliness** | ⭐⭐⭐ (Moderate: Uses Godot Inspector) | ⭐⭐⭐⭐⭐ (Excellent: Custom Editor Tab) | ⭐⭐⭐⭐⭐ (Outstanding: Full interactive game) | ⭐⭐ (Poor: Requires shader indexing) | ⭐⭐⭐⭐ (Very Good: Stays in Aseprite) |
| **Visual Feedback Speed / Fidelity** | ⭐⭐⭐⭐ (Instant editor viewport redraw) | ⭐⭐⭐⭐ (Instant preview subviewport) | ⭐⭐⭐⭐⭐ (100% Real-game physics & particles) | ⭐⭐⭐⭐⭐ (Instant GPU swap) | ⭐⭐ (Delayed until import/reload) |
| **Sprite & Asset Flexibility** | ⭐⭐⭐⭐⭐ (Full Texture2D, Font, Audio support) | ⭐⭐⭐⭐⭐ (Full Texture2D, Font, Audio support) | ⭐⭐⭐⭐⭐ (Full Texture2D, Font, Audio support) | ⭐⭐ (Colors only; no sprite replacement) | ⭐⭐⭐ (Requires strict folder conventions) |
| **Implementation Complexity** | ⭐⭐⭐⭐⭐ (Low: Pure Godot Resource model) | ⭐⭐ (High: EditorPlugin & UndoRedo GUI) | ⭐⭐⭐ (Medium: Standard Control scene) | ⭐⭐⭐ (Medium: Custom Shaders & Globals) | ⭐⭐⭐ (Medium: Custom Parser/Watcher) |
| **Maintenance Overhead** | ⭐⭐⭐⭐⭐ (Near Zero: Engine native) | ⭐⭐ (Moderate: May break across Godot updates)| ⭐⭐⭐⭐ (Low: Standard gameplay scene) | ⭐⭐⭐ (Medium: Shader compatibility) | ⭐⭐⭐⭐ (Low: Standalone parsers) |
| **Undo / Redo & Version Control** | ⭐⭐⭐⭐⭐ (Built-in Godot UndoRedo + Clean Git TRES) | ⭐⭐⭐ (Requires manual UndoRedo plumbing) | ⭐⭐⭐⭐ (One-click save to versioned TRES) | ⭐⭐⭐⭐ (Project settings / texture commit) | ⭐⭐⭐⭐⭐ (Standard Git file commits) |

---

## 5. Recommended Architecture for Pachi: The Hybrid Theme System

To provide the ideal balance between **non-technical artist usability**, **immediate visual feedback**, and **low engineering overhead**, we recommend a **Three-Tier Hybrid Architecture**:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          PACHI HYBRID THEME SYSTEM                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  [ Tier 1: Core Foundation ]                                                │
│  • VisualTheme.cs (Master Resource) containing categorized sub-resources:   │
│    - PaletteConfig (Board background, boundary colors, UI base colors)      │
│    - BallVisualConfig (6 Tiers: colors, textures, trail gradients)          │
│    - PinVisualConfig (Texture, scale, flash color, spark particle ramp)     │
│    - PocketVisualConfig (Arm sprites, indicator style, hole modulation)     │
│    - UIThemeConfig (Card styles, font overrides, drag highlights)           │
│                                                                             │
│                                      │                                      │
│                ┌─────────────────────┴─────────────────────┐                │
│                ▼                                           ▼                │
│  [ Tier 2: Editor Experience ]             [ Tier 3: Runtime Sandbox ]      │
│  • Single centralized active_theme.tres    • Dedicated design_sandbox.tscn  │
│  • ExportGroup organized Inspector         • Real-time auto-fire physics    │
│  • [Tool] live viewport updates            • WYSIWYG sliders & pickers      │
│  • Zero-risk for scene tree breakage       • 1-Click "Save Theme" button    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Why this Hybrid Solution is Ideal:
1. **Zero Technical Risk**: All visual data is decoupled from scene nodes and colliders into a single `VisualTheme` resource. The artist never needs to edit `.tscn` files or touch physics layers.
2. **Dual-Authoring Options**:
   - **For quick tweaks**: The artist can open `active_theme.tres` directly in the Godot Inspector and see updates live in any open scene.
   - **For comprehensive tuning**: The artist launches `design_sandbox.tscn` (by pressing F6 or running a debug build) and tweaks colors, sprites, trails, and sparks while balls bounce in real-time simulation.
3. **Pachi Codebase Synergy**:
   - Directly fulfills the `// TODO: add sprite attribute when art is ready` in [BallVariant.cs](file:///home/axel/workspace/axelmagn/pachi/pachi/src/balls/BallVariant.cs).
   - Replaces hardcoded arm textures in [pocket.tscn](file:///home/axel/workspace/axelmagn/pachi/pachi/src/pockets/pocket.tscn) with themeable properties.
   - Integrates cleanly with `GameConfig.Instance` as the single source of truth for runtime theming.

---

## 6. Concrete Implementation Blueprint for Pachi

### Step 1: Define Theme Resources in C#

```csharp
// src/theme/VisualTheme.cs
using Godot;
using Godot.Collections;

namespace Pachi.Theme;

[GlobalClass]
public partial class VisualTheme : Resource
{
    [ExportGroup("Environment & Board")]
    [Export] public Color BoardBackgroundColor { get; set; } = new Color("131313");
    [Export] public Color BoundaryWallColor { get; set; } = new Color("2A2A2A");

    [ExportGroup("Ball Tiers")]
    [Export] public Array<BallVisualData> BallTiers { get; set; } = new();

    [ExportGroup("Pins")]
    [Export] public Texture2D PinTexture { get; set; }
    [Export] public Vector2 PinScale { get; set; } = Vector2.One;
    [Export] public Color PinDefaultColor { get; set; } = Colors.White;
    [Export] public Color PinFlashColor { get; set; } = new Color(1.0f, 0.85f, 0.2f, 1.0f);
    [Export] public Gradient PinSparkGradient { get; set; }

    [ExportGroup("Pockets")]
    [Export] public Texture2D PocketLeftArmTexture { get; set; }
    [Export] public Texture2D PocketRightArmTexture { get; set; }
    [Export] public Vector2 PocketArmScale { get; set; } = new Vector2(0.035f, 0.035f);
    [Export] public Color IndicatorBackgroundColor { get; set; } = new Color("161616");
    [Export] public Color IndicatorBorderColor { get; set; } = Colors.Black;

    [ExportGroup("Card UI")]
    [Export] public Color CardDefaultColor { get; set; } = new Color("1E1E1E");
    [Export] public Color CardBorderColor { get; set; } = new Color(1, 1, 1, 0.4f);
    [Export] public Font CardFont { get; set; }
}

[GlobalClass]
public partial class BallVisualData : Resource
{
    [Export] public string TierName { get; set; } = "Tier 1";
    [Export] public Color PlaceholderColor { get; set; } = Colors.White;
    [Export] public Texture2D SpriteTexture { get; set; }
    [Export] public Vector2 SpriteScale { get; set; } = Vector2.One;
    [Export] public Gradient MotionTrailGradient { get; set; }
}
```

### Step 2: Update `GameConfig.cs` to Host the Active Theme

```csharp
// src/main_game/GameConfig.cs
using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;
using Pachi.Theme;

public partial class GameConfig : Node
{
    public static GameConfig Instance { get; private set; }

    [Export]
    public VisualTheme ActiveTheme { get; set; }

    [Export]
    public PackedScene BallScene { get; set; }

    [Export]
    public Array<BallVariant> BallTiers { get; set; }

    [Export]
    public Array<CardArchetype> CardArchetypes { get; set; }

    public Random Rng { get; set; } = new Random();

    public override void _EnterTree()
    {
        Debug.Assert(Instance == null);
        Instance = this;
    }

    public override void _Ready()
    {
        Debug.Assert(BallScene != null, "BallScene must be configured on GameConfig");
        Debug.Assert(BallTiers != null && BallTiers.Count > 0, "BallTiers must be configured on GameConfig");
        Debug.Assert(CardArchetypes != null && CardArchetypes.Count > 0, "CardArchetypes must be configured on GameConfig");
    }
}
```

### Step 3: Refactor Entities for Visual Decoupling

#### 1. Ball Rendering (`BallVariant.cs` and `Ball.cs`)
```csharp
// In BallVariant.cs
[GlobalClass]
public partial class BallVariant : Resource
{
    [Export]
    public BallVisualData VisualData { get; set; }

    [Export]
    public int BasePrice = 10;
}
```

```csharp
// In Ball.cs _Ready()
public override void _Ready()
{
    // ... assertions ...
    if (Variant?.VisualData != null)
    {
        if (Variant.VisualData.SpriteTexture != null)
        {
            // If sprite exists, configure Texture2D
            PlaceholderSprite.Visible = false;
            // Set up Sprite2D with Variant.VisualData.SpriteTexture
        }
        else
        {
            PlaceholderSprite.Visible = true;
            PlaceholderSprite.Color = Variant.VisualData.PlaceholderColor;
        }

        if (MotionTrail != null && Variant.VisualData.MotionTrailGradient != null)
        {
            MotionTrail.Gradient = Variant.VisualData.MotionTrailGradient;
        }
    }
}
```

#### 2. Pocket Rendering (`Pocket.cs` and `PocketBallsIndicator.cs`)
```csharp
// In Pocket.cs
private void ApplyVisualTheme()
{
    var theme = GameConfig.Instance?.ActiveTheme;
    if (theme == null) return;

    if (LeftArm?.GetNodeOrNull<Sprite2D>("Sprite2D2") is Sprite2D leftSprite && theme.PocketLeftArmTexture != null)
    {
        leftSprite.Texture = theme.PocketLeftArmTexture;
        leftSprite.Scale = theme.PocketArmScale;
    }

    if (RightArm?.GetNodeOrNull<Sprite2D>("CollisionShape2D/Sprite2D") is Sprite2D rightSprite && theme.PocketRightArmTexture != null)
    {
        rightSprite.Texture = theme.PocketRightArmTexture;
        rightSprite.Scale = theme.PocketArmScale;
    }

    if (InputsIndicator != null)
    {
        InputsIndicator.BackgroundColor = theme.IndicatorBackgroundColor;
        InputsIndicator.BorderColor = theme.IndicatorBorderColor;
    }
    if (OutputsIndicator != null)
    {
        OutputsIndicator.BackgroundColor = theme.IndicatorBackgroundColor;
        OutputsIndicator.BorderColor = theme.IndicatorBorderColor;
    }
}
```

---

## 7. Migration & Rollout Plan

1. **Phase 1: Resource Model & Architecture Setup**
   - Create `src/theme/VisualTheme.cs` and `src/theme/BallVisualData.cs`.
   - Create `res://assets/themes/default_theme.tres` populated with Pachi's current colors and placeholder assets.
   - Attach `VisualTheme` to `GameConfig.cs`.

2. **Phase 2: Entity Decoupling & `[Tool]` Viewport Wiring**
   - Update `BallVariant.cs` and `Ball.cs` to read visual properties from `BallVisualData`.
   - Update `Pin.cs`, `Pocket.cs`, and `PocketBallsIndicator.cs` to apply theme colors and textures.
   - Update `CardUI.cs` and `BackgroundLayer` to source colors from `VisualTheme`.

3. **Phase 3: Design Sandbox Scene**
   - Build `res://src/tools/design_sandbox.tscn` featuring side-by-side auto-firing playfield and `Control` design panel.
   - Add color pickers, sprite selectors, and the `ResourceSaver.Save()` trigger.

4. **Phase 4: Artist Onboarding & Documentation**
   - Provide a 1-page visual guide for the artist explaining how to launch the Sandbox or edit `default_theme.tres`.

---

## 8. Primary Sources & First-Party References

1. **Godot 4 Custom Resources & Data Modeling**
   - Official Godot Resource System Guide: [https://docs.godotengine.org/en/stable/tutorials/scripting/resources.html](https://docs.godotengine.org/en/stable/tutorials/scripting/resources.html)
   - C# Custom Resources with `[GlobalClass]`: [https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_global_classes.html](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_global_classes.html)
   - `ResourceSaver` Class Reference: [https://docs.godotengine.org/en/stable/classes/class_resourcesaver.html](https://docs.godotengine.org/en/stable/classes/class_resourcesaver.html)

2. **Godot 4 Editor Execution & `[Tool]` Mode**
   - Running Code in the Editor: [https://docs.godotengine.org/en/stable/tutorials/plugins/running_code_in_the_editor.html](https://docs.godotengine.org/en/stable/tutorials/plugins/running_code_in_the_editor.html)
   - Godot EditorPlugin Architecture: [https://docs.godotengine.org/en/stable/tutorials/plugins/editor/making_plugins.html](https://docs.godotengine.org/en/stable/tutorials/plugins/editor/making_plugins.html)
   - `EditorInspectorPlugin` API: [https://docs.godotengine.org/en/stable/classes/class_editorinspectorplugin.html](https://docs.godotengine.org/en/stable/classes/class_editorinspectorplugin.html)
   - Main Screen Plugins: [https://docs.godotengine.org/en/stable/tutorials/plugins/editor/making_main_screen_plugins.html](https://docs.godotengine.org/en/stable/tutorials/plugins/editor/making_main_screen_plugins.html)

3. **Godot 4 Shaders, GUI Theming & Palette Management**
   - Global Shader Parameters: [https://docs.godotengine.org/en/stable/tutorials/shaders/shader_reference/shading_language.html#global-uniforms](https://docs.godotengine.org/en/stable/tutorials/shaders/shader_reference/shading_language.html#global-uniforms)
   - GUI Theme System & `ThemeDB`: [https://docs.godotengine.org/en/stable/tutorials/ui/gui_theme.html](https://docs.godotengine.org/en/stable/tutorials/ui/gui_theme.html)
   - `StyleBox` Reference: [https://docs.godotengine.org/en/stable/classes/class_stylebox.html](https://docs.godotengine.org/en/stable/classes/class_stylebox.html)
