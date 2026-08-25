# 01: VisualConfig Resource & Settings Schema

**What to build:** Centralized visual customization settings resource located at `res://src/art/visual_config.tres` and its corresponding C# resource class. Provides organized Inspector dock categorization (Environment, Pins, Pockets, Cards & UI) and fires change notifications whenever visual properties are adjusted.

**Blocked by:** None (can start immediately)

**Status:** completed

- [x] `VisualConfig` is defined as a Godot `[GlobalClass]` custom resource in `src/art/VisualConfig.cs` inheriting from `Resource`.
- [x] Inspector export groups are established for `Environment` (Background Color, Boundary Color), `Pins` (Pin Base Color, Pin Texture, Flash Color), `Pockets` (Indicator Background Color, Indicator Border Color, Arm Color, Arm Texture), and `Cards & UI` (Card Background Color, Card Border Color, Indicator Background Color).
- [x] Modifying any exported property triggers `EmitChanged()`.
- [x] Default resource instance is created and saved at `res://src/art/visual_config.tres`.
