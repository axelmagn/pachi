# Hex Grid System Design Brief

## 1. Overview & Goals

The Hex Grid system provides a discrete spatial substrate for the Pachinko board in Pachi. It enables level designers to define valid placement regions for board elements (such as peg assemblies and pockets) and lets players place multi-cell stamps during gameplay.

### Core Objectives
- **Discrete Board Substrate**: Replace ad-hoc pin placement with a structured hex grid matching physical pinball/pachinko flow staggering.
- **Placement Group Tagging**: Support paintable placement groups per cell using compact bitmasks.
- **Multi-Cell Assemblies**: Define peg patterns, pockets, and obstacles as reusable multi-cell stamps.
- **In-Editor Painting Tooling**: Provide direct click-and-drag painting in the Godot 2D editor via an `EditorPlugin`.
- **Runtime Snapping & Validation**: Provide snap-to-cell preview, valid/invalid feedback, and dynamic entity lifecycle management.

---

## 2. Geometry & Coordinate System

The board uses **pointy-topped hexagons** with **odd-r offset coordinates** `(col, row)`. Odd rows shift right by half a column width ($\frac{\sqrt{3}}{2} R$). Stamp rotation is **not supported**.

```
    Row 0:  (0,0)     (1,0)     (2,0)
               \       / \       /
    Row 1:      (0,1)     (1,1)     (2,1)
               /       \ /       \
    Row 2:  (0,2)     (1,2)     (2,2)
```

### Mathematical Dimensions
Given cell outer radius $R$ (distance from center to each vertex):
- **Cell Width**: $W = \sqrt{3} \cdot R$
- **Cell Height**: $H = 2 \cdot R$
- **Horizontal Step**: $\Delta x = \sqrt{3} \cdot R$
- **Vertical Step**: $\Delta y = 1.5 \cdot R$
- **Row Offset**: Odd rows shift horizontally by $+0.5 \cdot \Delta x$.

### Coordinate Conversions

#### Offset to World
Given column $c \in [0, \text{Columns})$ and row $r \in [0, \text{Rows})$:
$$\text{WorldX}(c, r) = c \cdot \Delta x + (r \bmod 2 == 1 \,?\, 0.5 \cdot \Delta x : 0)$$
$$\text{WorldY}(c, r) = r \cdot \Delta y$$

#### World to Offset (Hex Snapping)
To find the nearest hex coordinate $(c, r)$ from local position $(x, y)$:
1. Convert $(x, y)$ to axial coordinates $(q_f, r_f)$:
   $$r_f = \frac{y}{1.5 R}$$
   $$q_f = \frac{x}{\sqrt{3} R} - \frac{r_f}{2}$$
2. Convert axial to cube coordinates $(x_c, y_c, z_c)$:
   $$x_c = q_f,\quad z_c = r_f,\quad y_c = -x_c - z_c$$
3. Round to nearest integer cube coordinates $(\text{rx}, \text{ry}, \text{rz})$ such that $\text{rx} + \text{ry} + \text{rz} = 0$.
4. Convert back to odd-r offset coordinates $(c, r)$:
   $$r = \text{rz}$$
   $$c = \text{rx} + \lfloor \frac{\text{rz} - (\text{rz} \ \& \ 1)}{2} \rfloor$$

---

## 3. Data Model & Architecture

### 3.1 Placement Groups Bitmask
Placement permissions use a single-byte bitmask (`byte`), allowing composable region tags:

```csharp
[Flags]
public enum PlacementGroup : byte
{
    None     = 0,
    Pegs     = 1 << 0, // 0x01: Allows peg stamps
    Pockets  = 1 << 1, // 0x02: Allows pocket stamps
    Hazards  = 1 << 2, // 0x04: Reserved for bumpers / hazards
    Blocked  = 1 << 7  // 0x80: Explicitly blocks all placements
}
```

### 3.2 HexStampBlueprint Resource
Stamps represent multi-cell assemblies (e.g. 1-cell pegs, 3-cell wedge clusters, 7-cell pockets):

```csharp
[GlobalClass]
public partial class HexStampBlueprint : Resource
{
    [Export]
    public string Id { get; set; } = string.Empty;

    [Export]
    public string DisplayName { get; set; } = string.Empty;

    [Export]
    public PlacementGroup RequiredPlacementGroup { get; set; } = PlacementGroup.Pegs;

    /// <summary>
    /// Relative offset coordinates (dc, dr) from anchor cell (0, 0).
    /// </summary>
    [Export]
    public Godot.Collections.Array<Vector2I> RelativeOffsets { get; set; } = new();

    /// <summary>
    /// Scene prefab instantiated for this stamp (e.g. Pin, Pocket, or multi-pin assembly).
    /// </summary>
    [Export]
    public PackedScene? EntityScene { get; set; }
}
```

### 3.3 HexGrid Node2D Component
`HexGrid` owns the board layout, grid bounds, serialized cell mask data, and runtime stamp instances:

```csharp
[Tool]
[GlobalClass]
public partial class HexGrid : Node2D
{
    [Export]
    public int Columns { get; set; } = 15;

    [Export]
    public int Rows { get; set; } = 20;

    [Export]
    public float Radius { get; set; } = 16.0f;

    /// <summary>
    /// Flat array of size Columns * Rows storing PlacementGroup masks per cell.
    /// Index: row * Columns + col.
    /// </summary>
    [Export]
    public byte[] CellPlacementGroups { get; set; } = System.Array.Empty<byte>();

    [Export]
    public Node2D? EntitiesContainer { get; set; }

    /// <summary>
    /// Tracks active stamp occupancies: map of cell coordinate -> root anchor coordinate.
    /// </summary>
    private readonly Dictionary<Vector2I, Vector2I> _occupiedCells = new();
    private readonly Dictionary<Vector2I, InstalledStamp> _installedStamps = new();
}
```

---

## 4. Placement & Validation Logic

### 4.1 Multi-Cell Validation Algorithm
To check if a stamp can be placed at anchor $(c, r)$:

```csharp
public bool CanPlaceStamp(Vector2I anchor, HexStampBlueprint blueprint)
{
    foreach (Vector2I offset in blueprint.RelativeOffsets)
    {
        Vector2I target = anchor + offset;

        // 1. Boundary check
        if (target.X < 0 || target.X >= Columns || target.Y < 0 || target.Y >= Rows)
        {
            return false;
        }

        // 2. Occupancy check
        if (_occupiedCells.ContainsKey(target))
        {
            return false;
        }

        // 3. Placement group mask check
        byte cellMask = GetCellGroup(target.X, target.Y);
        byte requiredMask = (byte)blueprint.RequiredPlacementGroup;

        if ((cellMask & (byte)PlacementGroup.Blocked) != 0)
        {
            return false;
        }

        if ((cellMask & requiredMask) != requiredMask)
        {
            return false;
        }
    }

    return true;
}
```

### 4.2 Placement & Removal Lifecycle
1. **`PlaceStamp(Vector2I anchor, HexStampBlueprint blueprint)`**:
   - Validates via `CanPlaceStamp(...)`.
   - Records every target cell in `_occupiedCells` mapping to `anchor`.
   - Instantiates `blueprint.EntityScene` under `EntitiesContainer`.
   - Sets instance local position to `OffsetToWorld(anchor.X, anchor.Y)`.
   - Stores `InstalledStamp` metadata in `_installedStamps[anchor]`.
   - Emits `StampPlaced(anchor, blueprint)`.
2. **`RemoveStamp(Vector2I anchor)`**:
   - Retrieves `InstalledStamp` at `anchor`.
   - Frees spawned entity node.
   - Clears associated entries from `_occupiedCells` and `_installedStamps`.
   - Emits `StampRemoved(anchor)`.

---

## 5. In-Editor Tooling & Viewport Workflow

### 5.1 Editor Plugin (`HexGridEditorPlugin`)
A dedicated Godot `EditorPlugin` activates when selecting a `HexGrid` node in the scene tree:
- **Canvas Input Forwarding**: Implements `_ForwardCanvasGuiInput(InputEvent @event)`.
- **Viewport Painting**: Captures left-click drag to paint the selected `PlacementGroup` bitmask, and right-click drag to erase (`PlacementGroup.None`).
- **Editor Toolbar / Palette**: Adds a clean editor bottom panel or inspector enum selector to pick the active paint brush:
  - `Paint Peg Region` (`PlacementGroup.Pegs`)
  - `Paint Pocket Region` (`PlacementGroup.Pockets`)
  - `Paint Blocked` (`PlacementGroup.Blocked`)
  - `Erase` (`PlacementGroup.None`)
- **Undo / Redo**: Integrates with `EditorUndoRedoManager` for painting operations.

### 5.2 Visualization (`_Draw()`)
`HexGrid` implements `_Draw()` with tool-mode awareness:
- Draws pointy-topped 6-vertex polygon outlines for all cells.
- Draws semi-transparent filled hexagons with distinct palette tints:
  - Cyan / Blue: Peg placement regions.
  - Orange / Amber: Pocket placement regions.
  - Dark Red / Crosshatch: Blocked cells.
- Toggleable inspector flags for coordinate labels and grid lines.

---

## 6. Prototype Scene & Runtime Controls

A standalone prototype scene (`src/levels/hex_grid_prototype.tscn`) validates the system:

1. **Ghost Preview**:
   - Spawns a semi-transparent preview node of the currently active stamp blueprint.
   - Snaps to the hex cell under the mouse cursor in real time.
   - Modulates green (`#00FF8880`) when valid, red (`#FF333380`) when invalid.
2. **Controls**:
   - **Mouse Move**: Move cursor to update ghost position and validity.
   - **Left Click**: Place current stamp if valid.
   - **Right Click / Backspace**: Remove stamp at cursor cell.
   - **Keys 1–9**: Cycle through available test stamps (Single Pin, 3-Pin Cluster, Wide Pocket).
3. **Physics Integration**:
   - Balls spawned from launcher collide against placed pins and trigger pockets normally.
