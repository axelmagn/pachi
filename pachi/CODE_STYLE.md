# Pachi C# & Godot Coding Standards

Authoritative coding standard for Pachi. All code must pass `./scripts/verify.sh` with zero warnings and zero errors (`TreatWarningsAsErrors=true`).

---

## 1. Syntax & Formatting

Formatting is enforced via `.editorconfig` and auto-fixed with `./scripts/verify.sh --fix` (`dotnet format`).

- **Bracing**: Allman style (opening brace on a new line for types, methods, control flow, accessors, and object initializers).
- **Indentation & Encoding**: 4 spaces, UTF-8 (no BOM), Unix LF line endings.
- **Expression Bodies (`=>`)**: Use for single-line properties, getters/setters, single-line methods, and local functions. Use block bodies for constructors and multi-line logic.
- **Keywords**: Use C# type aliases (`int`, `float`, `string`, `bool`) rather than BCL names (`Int32`, `Single`). Do not qualify members with `this.` unless required to resolve ambiguity.

---

## 2. Naming Conventions

Enforced as errors via Roslyn analyzer rules:

| Symbol Kind | Convention | Example |
| :--- | :--- | :--- |
| **Interfaces** | `IPascalCase` | `IPocketTarget`, `IPoolable` |
| **Classes, Structs, Enums, Delegates** | `PascalCase` | `PocketController`, `BallVariant` |
| **Methods & Properties** | `PascalCase` | `CalculateTrajectory()`, `MaxCapacity` |
| **Signal Delegates** | `[Signal]` `PascalCaseEventHandler` | `BallAwardedEventHandler` |
| **Constants & Public Static Readonly** | `PascalCase` | `MaxInputCapacity`, `GroupPockets` |
| **Private / Internal Fields** | `_camelCase` | `_linearVelocity`, `_isInitialized` |
| **Parameters & Locals** | `camelCase` | `ballVariant`, `streamIndex`, `delta` |
| **C# Files** | `PascalCase.cs` matching class | `PocketBallsIndicator.cs` (preserve `.cs.uid`) |
| **Godot Scenes & Resources** | `snake_case.tscn`, `snake_case.tres` | `pocket.tscn`, `ball_theme.tres` |
| **Acronyms** | 2-letter uppercase (`UI`, `IO`), 3+ letter capitalized (`Aabb`, `Json`), `Id`/`id` as word | `UIManager`, `InstanceId`, `ballId` |

---

## 3. Godot Engine Idioms & Architecture

### 3.1 Partial Classes
All classes inheriting `GodotObject` (including `Node`, `Resource`) and any outer enclosing classes must be declared `partial`.

```csharp
public partial class OuterContainer
{
    public partial class Pocket : Node2D { }
}
```

### 3.2 Global Classes (`[GlobalClass]`)
Use `[GlobalClass]` on non-generic `GodotObject` subclasses to register them in the Godot Editor.

```csharp
[GlobalClass]
[Icon("res://assets/icons/pocket.svg")]
public partial class Pocket : Node2D { }
```

### 3.3 Node Initialization & Null Safety (`_Ready`)
- Declare optional node references as nullable (`Type?`).
- Declare required references initialized in `_Ready()` as `= null!` and assert non-null with `Debug.Assert(...)`.
- Skip initialization logic in editor runs via `Engine.IsEditorHint()`.

```csharp
public partial class Pocket : Node2D
{
    [Export]
    public Hole? CatchHole { get; set; }

    private Label _scoreLabel = null!;
    private CollisionShape2D _collider = null!;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        _scoreLabel = GetNode<Label>("MarginContainer/ScoreLabel");
        _collider = GetNode<CollisionShape2D>("CatchHole/CollisionShape2D");

        System.Diagnostics.Debug.Assert(CatchHole is not null, "Pocket requires CatchHole reference.");
        System.Diagnostics.Debug.Assert(_scoreLabel is not null, "ScoreLabel child node is missing.");
        System.Diagnostics.Debug.Assert(_collider is not null, "CollisionShape2D child node is missing.");
    }
}
```

### 3.4 Value Types & Struct Mutation (`CS1612`)
Godot math structs (`Vector2`, `Vector3`, `Transform2D`, `Color`, `Rect2`) are value types. Modify properties using C# `with` expressions and identity constants:

```csharp
Position = Position with { X = 150.0f };
Transform2D transform = Transform2D.Identity;
Color defaultColor = Colors.Black; // Note: default 'new Color()' is transparent (0, 0, 0, 0)
```

### 3.5 Tool Scripts (`[Tool]`)
- Scripts and helper classes executing in the editor viewport must be marked `[Tool]`.
- Properties affecting custom drawing (`_Draw()`) must call `QueueRedraw()` inside the property setter.
- When spawning child nodes in tool scripts, assign `child.Owner = GetTree().EditedSceneRoot` for proper `.tscn` serialization.

```csharp
[Tool]
public partial class PocketBallsIndicator : Node2D
{
    private Color _fillColor = Colors.White;

    [Export]
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            if (_fillColor == value) return;
            _fillColor = value;
            QueueRedraw();
        }
    }

    public override void _Draw() => DrawCircle(Vector2.Zero, 16.0f, _fillColor);
}
```

### 3.6 Interop & Property Marshalling
Cache `GodotObject` properties in local variables inside performance-sensitive loops to avoid repeated P/Invoke boundaries:

```csharp
Vector2 currentPos = Position;
for (int i = 0; i < count; i++)
{
    currentPos += stepOffsets[i];
}
Position = currentPos;
```

---

## 4. Type Safety, Signals & Async

### 4.1 Type-Safe API References
Use source-generated `PropertyName`, `SignalName`, `nameof(...)`, or cached `StringName` constants instead of string literals:

```csharp
EmitSignal(SignalName.BallAwarded, (int)variant);
StringName prop = PropertyName.RotationDegrees;

public static class GameGroups
{
    public static readonly StringName Pockets = new("pockets");
}
```

### 4.2 Collections
- **Internal Domain Logic**: Use .NET BCL collections (`List<T>`, `Dictionary<K, V>`, `HashSet<T>`) for zero-marshalling overhead and full LINQ performance.
- **Inspector & Serialization**: Use `Godot.Collections.Array<T>` and `Godot.Collections.Dictionary<K, V>` only for `[Export]` properties and engine API boundaries.

```csharp
private readonly List<Ball> _activeBalls = [];

[Export]
public Godot.Collections.Array<string> TagList { get; set; } = ["Standard", "Bonus"];
```

### 4.3 Signals & Lifetime Management
- **Declaration**: Signal delegates must be named with suffix `EventHandler`, return `void`, and take `Variant`-compatible parameters (`[Signal] public delegate void BallAwardedEventHandler(int count);`).
- **Unsubscription**: Always unsubscribe (`-=`) event handlers in `_ExitTree()`, especially for singletons, autoloads, and global event buses.
- **Lambdas & One-Shot**: Avoid unbounded lambda subscriptions; use `Connect(..., (uint)ConnectFlags.OneShot)` for single-fire events.

```csharp
public override void _EnterTree()
{
    if (GlobalEvents.Instance is not null)
    {
        GlobalEvents.Instance.BallAwarded += OnBallAwarded;
    }
}

public override void _ExitTree()
{
    if (GlobalEvents.Instance is not null)
    {
        GlobalEvents.Instance.BallAwarded -= OnBallAwarded;
    }
}
```

### 4.4 Async Signatures
- Async methods must return `async Task` or `async ValueTask` (never `async void`).
- Await Godot signals using `ToSignal(emitter, SignalName.SignalName)`.
- Never block asynchronously via `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.

```csharp
public async Task PlaySequenceAsync(Timer timer, CancellationToken ct = default)
{
    timer.Start(1.5f);
    await ToSignal(timer, Timer.SignalName.Timeout);
}
```

---

## 5. Analyzer Severities Reference

Configured in `.editorconfig` to align Roslyn with Godot runtime lifecycles:

| Diagnostic | Severity | Rationale |
| :--- | :--- | :--- |
| `CA1050` (*Namespaces*) | `none` | `[GlobalClass]` nodes reside in global namespace for `.tscn` editor resolution. |
| `CA1707` (*Underscores*) | `none` | Godot virtual callbacks use leading underscores (`_Ready`, `_Process`, `_Draw`). |
| `CA1822` (*Static members*) | `suggestion` | Engine invokes lifecycle callbacks on node instances. |
| `CA1001` / `CA2000` (*Disposables*) | `suggestion` | Node lifecycle is managed by Scene Tree and `QueueFree()`, not .NET `IDisposable`. |
